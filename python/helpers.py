# helpers.py
import streamlit as st
import pandas as pd
import os
from openai import OpenAI
# from dotenv import load_dotenv
import csv

# get the virtual environment directory
# venv = os.environ['VIRTUAL_ENV']
# dir = os.path.dirname(venv)

# Load variables from .env file
# dotenv_path = os.path.join(dir , '.env')
# load_dotenv(dotenv_path)

# Access your API key
# key = os.environ.get("OPENAI_API_KEY")
key = os.environ["OPENAI_API_KEY"] == st.secrets["OPENAI_API_KEY"]

path = os.path.join(dir , "files", "B-score mapping for revit.xlsx")
temp1 = os.path.join(dir , "temp", "intermediates", "noblanks.csv")
temp2 = os.path.join(dir , "temp", "intermediates", "add.csv")
temp3 = os.path.join(dir , "temp", "intermediates", "filtered.csv")
out_path = os.path.join(dir , "temp", "results", "Wall_results.csv")

colu = []

def init_ai():
    client = OpenAI(api_key=key)
    return client

def process_csv(cols):
    pipeline = init_ai()
    # print("AI pipeline initialized") # for terminal bug testing

    labels = label()
    # print("labelling done") # for terminal bug testing

    eler = Ai_gen(pipeline, cols, labels)
    # print("AI generated") # for terminal bug testing

    add_cols(eler, temp2)
    # print("Columns added") # for terminal bug testing

    merge_csv(temp2, temp3, out_path)
    # print("CSV merged") # for terminal bug testing

    return out_path

def label():
    df = pd.read_excel(path)
    # Filter rows based on the "Element/Family type" column containing the word "Wall"
    filtered_df = df[df["Element/Family type"].str.contains("Wall", case=False, na=False)]

    # Extract the "Bscore Item" column and convert it to a list
    bscore_items_list = filtered_df["Bscore Item"].tolist()

    return bscore_items_list

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

def csv_data(outpath):
    csv_file = outpath
    data = []
    
    # Read CSV file and convert to dictionary
    with open(csv_file, newline='') as file:
        reader = csv.DictReader(file)
        for row in reader:
            data.append(row)
    # print("csv print done!") # for terminal bug testing
