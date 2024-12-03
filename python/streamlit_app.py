import streamlit as st
import pandas as pd
import os
import io
import xlsxwriter
import csv
import re
import requests
import json

# directory reach
current = os.path.dirname(os.path.realpath(__file__))

# # for streamlit temp directory
# temp_dir = st.session_state.get("temp_dir", "/tmp")

# for localhost temp directory
temp_dir = os.path.join(current, "temp")

from helpers import process_csv, init_ai

client = init_ai()

# Set BASE_URL using the function to get the dynamic port
BASE_URL = "http://localhost:8080"
if not BASE_URL:
    st.error("Error: Unable to determine the Revit server base URL. Please ensure the Revit plugin is running.")

# Streamlit App Title
st.title("AI Bscore Label Prediction App")

st.header("Functions:")
st.write("""
    - **AI Chatbot**: Chat with the AI to get help any queries you may have.
    - **Revit Information Retrieval**: Retrieve all wall parameters from the Revit project.
    - **Filter Columns**: Fill in key words of columns that you wish to filter out (it does not have to be the entire full name of the column).
    - **Bscore Prediction**: Process the file schedule data from Revit through AI.
""")

data = os.path.join(current, "temp", "Wall_data.csv")

# Function to call the "Get Revit Version" API
def get_revit_version():
    if not BASE_URL:
        return "Error: Revit server base URL is not set."
    try:
        response = requests.get(f"{BASE_URL}/getrevitversion")
        if response.status_code == 200:
            return response.json()
        else:
            return f"Error: {response.status_code} - {response.text}"
    except requests.exceptions.ConnectionError:
        return "Connection error: Unable to reach the server."
    except requests.exceptions.Timeout:
        return "Request timed out. The server might be busy or unresponsive."
    except requests.exceptions.RequestException as e:
        return f"An error occurred: {e}"

# Function to call the "Export Wall Data" API
def export_wall_data():
    if not BASE_URL:
        return "Error: Revit server base URL is not set."
    try:
        params = {"filepath": data}
        response = requests.get(f"{BASE_URL}/getwalldata", params=params)
        if response.status_code == 200:
            # Read the CSV response
            csv_file = io.StringIO(response.text)
            wall_data_df = pd.read_csv(csv_file)
            return wall_data_df
        
        else:
            return f"Error: {response.status_code} - {response.text}"
    except requests.exceptions.ConnectionError:
        return "Connection error: Unable to reach the server."
    except requests.exceptions.Timeout:
        return "Request timed out. The server might be busy or unresponsive."
    except requests.exceptions.RequestException as e:
        return f"An error occurred: {e}"

def send_bscore_data_to_revit(bscore_df):
    if not BASE_URL:
        return "Error: Revit server base URL is not set."
    try:
        # Convert DataFrame to CSV
        csv_data = bscore_df.to_csv(index=False)
        # Define the API endpoint (replace with the correct endpoint)
        endpoint = f"{BASE_URL}/importbscoredata"
        # Define headers
        headers = {
            "Content-Type": "text/csv",  # Adjust if using JSON
            "Host": "localhost",
        }
        # Send POST request to Revit API
        response = requests.post(endpoint, headers=headers, data=csv_data)
        if response.status_code == 200:
            return "Data successfully sent to Revit."
        else:
            return f"Error {response.status_code}: {response.text}"
    except requests.exceptions.ConnectionError:
        return "Connection error: Unable to reach the server."
    except requests.exceptions.Timeout:
        return "Request timed out. The server might be busy or unresponsive."
    except requests.exceptions.RequestException as e:
        return f"An error occurred: {e}"

if 'show_chat' not in st.session_state:
    st.session_state['show_chat'] = False
if 'messages' not in st.session_state:
    st.session_state['messages'] = []

st.header("AI Chatbot")

# Button to open/close chatbot
if st.button("Open Chatbot"):
    st.session_state['show_chat'] = not st.session_state['show_chat']

# Check if 'chat_input' is in session_state, initialize if not
if 'chat_input' not in st.session_state:
    st.session_state['chat_input'] = ''

# Check if 'reset_chat_input' flag is set, reset input if needed
if st.session_state.get('reset_chat_input', False):
    st.session_state['chat_input'] = ''
    st.session_state['reset_chat_input'] = False  # Reset the flag

# Conditional display of chatbot
if st.session_state['show_chat']:
    # Create a container for the chat window
    with st.container():
        # Create columns for the header and close button
        header_col, close_col = st.columns([8, 2])

        with header_col:
            st.subheader("Type in your queries below:")

        with close_col:
            # Align the 'Close Chatbot' button to the right
            if st.button("Close Chatbot", key="close_button", type="primary"):
                st.session_state['show_chat'] = False
                st.rerun()

        # Display previous messages
        for msg in st.session_state['messages']:
            st.write(f"**{msg['role'].title()}**: {msg['content']}")

        # Input for the user message
        user_input = st.text_input("Ask GPT something!", key="chat_input", type="default")

        # Create columns for Send button and potential future alignment
        send_col, spacer_col = st.columns([1, 8])

        addon = "If you are unable to determine the answer based on the data given to your context, say ' I do not have a conclusive answer. '. Do not try to guess any answers."

        with send_col:
            if st.button("Send", key="send_button"):
                if user_input:
                    st.session_state['messages'].append({"role"   : "system",
                                                        "content": "You are a helpful assistant, trying to help answer users' questions related to the app workings, data uploaded from Revit and processed data from AI response, and the project data in general!"},
                                                        {"role": "user", "content": user_input+addon})

                    with st.spinner('Generating response...'):
                        response = client.chat.completions.create(
                            model="gpt-4o-mini",
                            messages=st.session_state['messages']
                        )
                        reply = response.choices[0].message.content

                    st.session_state['messages'].append({"role": "assistant", "content": reply})

                    # Set flag to reset the input field
                    st.session_state['reset_chat_input'] = True

                    # Rerun the function so that new questions can be asked
                    st.rerun()

st.header("Revit Information Retrieval")

# Button to get Revit version
if st.button("Get Revit Version"):
    version = get_revit_version()
    st.success(f"Revit Version: {version}")

# Button to get Wall Element Types
if st.button("Get Wall Parameter Data"):
    wall_data = export_wall_data()
    if isinstance(wall_data, pd.DataFrame):
        st.write("### Wall Element Types:")
        st.dataframe(wall_data)  # Display the DataFrame directly
    else:
        st.error(wall_data)  # Display the error message if not a DataFrame

st.header("Filter Columns")

# Input for column names
columns_input = st.text_input(
    "Enter columns to filter (comma-separated, e.g., Family, BDAS, etc.):",
    ""
)

# Button to apply column filter
if st.button("Apply Filter"):
    selected_cols = []
    specific_columns = []

    # print(f"col: {colu}") # for terminal bug testing
    # print(f"data: {data}") # for terminal bug testing

    # need to update filepaths
    filepath = os.path.join(current, "temp", "WallDataExport.csv")
    temp3 = os.path.join(temp_dir, "filtered.csv")

    # Open the file and read the specific columns
    with open(filepath, mode='r', newline='', encoding='utf-8') as file:

        reader = csv.DictReader(file)  # DictReader allows reading by column names
        
        # Get the column headings from the CSV
        headings = reader.fieldnames
        
        # add input for user to use their own columns
        if columns_input != "":
            filter_words = [col.strip() for col in re.split(",|, ", columns_input) if col.strip()]
            # Filter column names that contain any of the words in the filter
            for heading in headings:
                if any(word in heading.lower() for word in filter_words):  # Case-insensitive search
                    specific_columns.append(heading)
        else:
            # If no filter input is provided, use all columns
            specific_columns = headings

        # print(f"filter words: {filter_words}") # for terminal bug testing

        # Filter rows based on the selected columns
        for row in reader:
            if row:
                selected_row = {col: row[col] for col in specific_columns if col in row}
                
                # Ensure that at least one column has a non-empty value
                if any(selected_row.values()):
                    selected_cols.append(selected_row)

    # print(selected_cols) # for terminal bug testing

    with open(temp3, mode='w', newline='', encoding='utf-8') as output_file:
        writer = csv.DictWriter(output_file, fieldnames=specific_columns)
        writer.writeheader()  # Write the column headers
        writer.writerows(selected_cols)  # Write each filtered row
    
    st.session_state['filtered_columns'] = selected_cols
    
    st.success("Columns filtered successfully!")
    print("Columns filtered successfully!")

# Initialize session state variables if they don't exist
if 'bscore_df' not in st.session_state:
    st.session_state['bscore_df'] = None

st.header("Bscore Prediction")
if st.button("Predict Bscore"):
    if 'filtered_columns' in st.session_state:
        cols = st.session_state['filtered_columns']
        st.write("Bscore prediction is in progress...")
        process_csv(cols)
        st.write("Bscore prediction is complete!")
        df = pd.read_csv(os.path.join(temp_dir , "Wall_results.csv"))
        
        # Store the DataFrame in session_state
        st.session_state['bscore_df'] = df

        # Display the DataFrame in Streamlit
        st.dataframe(df)
        print("Bscore prediction is complete!")
        
        # Convert the DataFrame to an Csv file in memory
        towrite = io.BytesIO()
        df.to_excel(towrite, index=False, engine='xlsxwriter')
        towrite.seek(0)  # Reset the pointer to the beginning of the stream
        
        # Provide a download button for the Csv file
        st.download_button(
            label="Download Results as Excel",
            data=towrite,
            file_name='Wall_results.xlsx',
            mime='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        )
        # Button to send BScore data back to Revit
    else:
        st.warning("Please filter columns first. If you do not have any filters in mind just click the 'Apply Filter' button eitherway.")
    
if st.session_state['bscore_df'] is not None:
    st.header("Send BScore Predictions to Revit")
    if st.button("Send BScore Data to Revit"):
        result = send_bscore_data_to_revit(st.session_state['bscore_df'])
        if "successfully" in result.lower():
            st.success(result)
        else:
            st.error(result)