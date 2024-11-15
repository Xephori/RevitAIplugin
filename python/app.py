from flask import Flask, request, jsonify
import pandas as pd
import os
from flask_cors import CORS
import csv
from datetime import datetime
from openai import OpenAI
from dotenv import load_dotenv

# get the virtual environment directory
venv = os.environ['VIRTUAL_ENV']
dir = os.path.dirname(venv)

# Load variables from .env file
dotenv_path = os.path.join(dir , '.env')
load_dotenv(dotenv_path)

# Access your API key
key = os.environ.get("OPENAI_API_KEY")

# initialise the flask app
app = Flask(__name__)
CORS(app)

# need to add these paths to a temp folder in the environment
path = os.path.join(dir , "files", "B-score mapping for revit.xlsx")
temp1 = os.path.join(dir , "temp", "intermediates", "noblanks.csv")
temp2 = os.path.join(dir , "temp", "intermediates", "add.csv")
temp3 = os.path.join(dir , "temp", "intermediates", "filtered.csv")
out_path = os.path.join(dir , "files", "results", "Wall_results.csv")

colu = []

# Function to parse user input for columns
@app.route('/api/filter_columns', methods=['POST'])
def filter_input():
    # Get the JSON data sent from the client
    data = request.get_json()

    if not data or "columns" not in data:
        return jsonify({"error": "Invalid request, 'columns' field is required"}), 400

    columns = data["columns"]
    if not isinstance(columns, str):
        return jsonify({"error": "'columns' field must be a string"}), 400

    # Process the columns data (split into a list, filter columns, etc.)
    column_list = [col.strip("") for col in columns.split(", ")]
    # print("Received columns:", column_list) # for terminal bug testing

    for item in column_list:
        if item not in colu:
            colu.append(item)
    # print(f"col: {colu}") # for terminal bug testing

    return jsonify({"data": column_list}), 200

# Endpoint to receive the CSV and process it
# @app.route('/api/upload', methods=['POST'])
@app.route('/api/process_csv', methods=['POST'])
def process_csv():
    # Search for a CSV file in the directory
    # csv_directory = os.path.join(dir, 'files')
    csv_directory = os.path.join(dir, 'temp')
    # print(csv_directory) # for terminal bug testing
    
    csv_files = [f for f in os.listdir(csv_directory) if f.endswith('.csv')]

    if not csv_files:
        return jsonify({'error': 'No CSV file found in the directory'}), 400

    # Use the first CSV file found
    csv_filename = csv_files[0]
    # print("CSV file found:", csv_filename) # for terminal bug testing   

    filepath = os.path.join(csv_directory, csv_filename)
    # print("CSV file path:", filepath) # for terminal bug testing

    try:
        # Initialize AI pipeline
        pipeline = init_ai()
        # print("AI pipeline initialized") # for terminal bug testing

        cols = col_filter(filepath, temp3)
        # print("Columns filtered") # for terminal bug testing

        labels = label()
        # print("labelling done") # for terminal bug testing

        eler = Ai_gen(pipeline, cols, labels)
        # print("AI generated") # for terminal bug testing

        add_cols(eler, temp2)
        # print("Columns added") # for terminal bug testing

        merge_csv(temp2, temp3, out_path)
        # print("CSV merged") # for terminal bug testing

        data_final = result_csv(out_path)
        # print("CSV data ready") # for terminal bug testing  

        return jsonify(data_final), 200
    
    except Exception as e:

        return jsonify({'error': str(e)}), 500


def result_csv(filepath):
    with open(filepath, mode = 'r') as file:
        csv_reader = csv.reader(file)

        data_list = []

        for row in csv_reader:
            if any(row):
                data_list.append(row)

    return data_list

# Function to filter out selected columns
def col_filter(filepath, temp3):
    selected_cols = []
    headings = []
    specific_columns = []
    data = colu  

    # print(f"col: {colu}") # for terminal bug testing
    # print(f"data: {data}") # for terminal bug testing

    filter_words = []
    
    # Open the file and read the specific columns
    with open(filepath, mode='r', newline='', encoding='utf-8') as file:

        reader = csv.DictReader(file)  # DictReader allows reading by column names
        
        # Get the column headings from the CSV
        headings = reader.fieldnames
        
        # add input for user to use their own columns
        if data != []:
            filter_words.extend(col for col in data)  # Use extend instead of append

        else:
            # By default filter
            # Words to filter for in the column names
            filter_words.extend(item for item in ("buildability", "bdas", "bscore"))  # Use extend instead of append

        # print(f"filter words: {filter_words}") # for terminal bug testing

        # Filter column names that contain any of the words in the filter
        for heading in headings:
            if any(word in heading.lower() for word in filter_words):  # Case-insensitive search
                specific_columns.append(heading)
    
        specific_columns.append("BuiltInParameters.Family and Type")
        # print(f"columns: {specific_columns}") # for terminal bug testing

        # Filter rows based on the selected columns
        for row in reader:
            if row:
                selected_row = {col: row[col] for col in specific_columns if col in row}
                
                # Ensure that at least one column has a non-empty value
                if any(selected_row.values()):
                    selected_cols.append(selected_row)

    # print(len(selected_cols)) # for terminal bug testing

    with open(temp3, mode='w', newline='', encoding='utf-8') as output_file:
        writer = csv.DictWriter(output_file, fieldnames=specific_columns)
        writer.writeheader()  # Write the column headers
        writer.writerows(selected_cols)  # Write each filtered row

    return selected_cols

# Function to get the labels
def label():
    df = pd.read_excel(path)
    # Filter rows based on the "Element/Family type" column containing the word "Wall"
    filtered_df = df[df["Element/Family type"].str.contains("Wall", case=False, na=False)]

    # Extract the "Bscore Item" column and convert it to a list
    bscore_items_list = filtered_df["Bscore Item"].tolist()

    return bscore_items_list

# Function to generate AI output
def Ai_gen(client, selected_cols, label):
    ele_row = []

    limiter = "Give your answer in two separate sentences, the first sentence for the predicted answer in this form: 'Predicted answer: <answer>', in as few words as possible, but not too simplistic; and the other sentence for the reasoning in this form 'Reasoning: <answer>' , in as much detail as possible, both sentences separated by a single new line."

    ele_list = "Also, give your predicted answer only within the scope of this list:" + str(label) + ". If you are unable to determine the answer based on the list given, say ' I do not have a conclusive answer.'. "
    # need to further optimise - some if else statements to account for the different lists for diff elements

    for delist in selected_cols:

        thingy = []

        reasonz = []

        preds = []

        full_message = "What is the construction element of this row given the information after this?"+str(delist)+limiter+ele_list

        messages = [
            {"role"   : "system",
            "content": "You are a helpful assistant, trying to guess accurately the classification of the construction elements!"},
            {"role"   : "user",
            "content": full_message},
        ]

        # for chatgpt
        chat_completion = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=messages,
            temperature=0,
            # adjust persona and temperature
        )
        # print(chat_completion.choices[0].message.content) # for terminal bug testing
        sentence = chat_completion.choices[0].message.content.split('\n\n') # for gpt
        # print(sentence) # for terminal bug testing
        # sentence = outputs[0]["generated_text"][-1]['content'].split('\n\n') # for llama3

        reasonz.append(sentence[1].split(': ')[1])
        preds.append(sentence[0].split(': ')[1])
        # del preds[0]

        thingy.append(preds)
        thingy.append(reasonz)

        ele_row.append(thingy) # list in a list

        # print('one row done') # for terminal bug testing

        # # For terminal bug testing time taken
        # # Get the current timestamp
        # current_timestamp = datetime.now()

        # # Print the formatted timestamp
        # formatted_timestamp = current_timestamp.strftime("%Y-%m-%d %H:%M:%S")
        # print(formatted_timestamp)

    # print("all rows done") # for terminal bug testing
    return ele_row

# Function to clean the blank rows in the csv file
def clean_blank_rows(filepath, temp1):
    with open(filepath, newline='') as in_file:
        with open(temp1, 'w', newline='') as temp_file:
            writer = csv.writer(temp_file)
            for row in csv.reader(in_file):
                if any(row):
                    writer.writerow(row)
        temp_file.close()

# Function to make a csv file with just the 2 additional columns
def add_cols(ele_row, temp2):
    ele_row.insert(0, ['Predicted answer', 'Reasoning'])
    with open(temp2, 'w', newline='') as add:
        writer = csv.writer(add)
        for i in ele_row:
            writer.writerow(i)
    
# Function to merge the original file and the file with just the 2 additional columns
# merge only the cols that is filtered out
def merge_csv(temp1, temp2, outpath):
    # Open the first CSV file for reading
    with open(temp1, 'r') as in1, open(temp2, 'r') as in2, open(outpath, 'w', newline='') as outfile:
        reader1 = csv.reader(in1)
        reader2 = csv.reader(in2)
        writer = csv.writer(outfile)
        
        # Iterate through both files row by row
        for row1, row2 in zip(reader1, reader2):
            # Append the columns from file2 (row2) to file1's row (row1)
            merged_row = row1 + row2
            # Write the merged row to the output file
            writer.writerow(merged_row)
    # print("csv merged!") # for terminal bug testing

# Function to display the csv in table form
@app.route('/csv_data')
def csv_data(outpath):
    csv_file = outpath
    data = []
    
    # Read CSV file and convert to dictionary
    with open(csv_file, newline='') as file:
        reader = csv.DictReader(file)
        for row in reader:
            data.append(row)
    # print("csv print done!") # for terminal bug testing
    return jsonify(data)


# Function to init AI model
def init_ai():
    client = OpenAI(api_key=key)
    return client
    
if __name__ == '__main__':  
    app.run(debug=True)
