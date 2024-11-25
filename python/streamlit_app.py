import streamlit as st
import pandas as pd
import os
import sys
import io
import xlsxwriter
import csv
import re
import requests

# directory reach
current = os.path.dirname(os.path.realpath(__file__))

# parent = os.path.dirname(current)

# sys.path.append(parent)

from helpers import process_csv

# Streamlit App Title
st.title("AI Bscore Label Prediction App")

st.header("Functions:")
st.write("""
    - **Filter Columns**: Fill in key words of columns that you wish to filter out (it does not have to be the entire full name of the column).
    - **Revit Information Retrieval**: Retrieve all wall parameters from the Revit project.
    - **Bscore Prediction**: Process the file schedule data from Revit through AI.
""")

st.header("Filter Columns")

# Input for column names
columns_input = st.text_input(
    "Enter columns to filter (comma-separated, e.g., Family, BDAS, etc.):",
    ""
)

# Button to apply column filter
if st.button("Apply Filter"):
    selected_cols = []
    headings = []
    specific_columns = []

    # print(f"col: {colu}") # for terminal bug testing
    # print(f"data: {data}") # for terminal bug testing

    filter_words = []
    filepath = os.path.join(current, "temp", "Wall_uh.csv")
    temp3 = os.path.join(current, "temp", "intermediates", "filtered.csv")

    # Open the file and read the specific columns
    with open(filepath, mode='r', newline='', encoding='utf-8') as file:

        reader = csv.DictReader(file)  # DictReader allows reading by column names
        
        # Get the column headings from the CSV
        headings = reader.fieldnames
        
        # add input for user to use their own columns
        if columns_input != "":
            filter_words.extend(col for col in re.split(",|, ", columns_input))  # Use extend instead of append
            # print(filter_words)

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

    # print(selected_cols) # for terminal bug testing

    with open(temp3, mode='w', newline='', encoding='utf-8') as output_file:
        writer = csv.DictWriter(output_file, fieldnames=specific_columns)
        writer.writeheader()  # Write the column headers
        writer.writerows(selected_cols)  # Write each filtered row
    
    st.session_state['filtered_columns'] = selected_cols
    
    st.success("Columns filtered successfully!")
    print("Columns filtered successfully!")

st.header("Revit Information Retrieval")

# Base URL of the Revit HTTP server
BASE_URL = "http://127.0.0.1:8080"

data = os.path.join(current, "temp", "Wall_data.csv")

headers = {
    "Content-Type": "application/json",  # Not always required for GET, but some servers check this
    "Host": "localhost",
}

# Function to call the "Get Revit Version" API
def get_revit_version():
    try:
        response = requests.get(f"{BASE_URL}/get-revit-version", headers=headers)
        if response.status_code == 200:
            return response.text
        else:
            return f"Error: {response.status_code} - {response.text}"
    except requests.exceptions.RequestException as e:
        return f"Error connecting to Revit API: {e}"

# Function to call the "Export Wall Data" API
def export_wall_data():
    try:
        params = {"filepath": data}
        response = requests.get(f"{BASE_URL}/export-wall-data", headers=headers, params=params)
        if response.status_code == 200:
            return response.text
        else:
            return f"Error: {response.status_code} - {response.text}"
    except requests.exceptions.RequestException as e:
        return f"Error connecting to Revit API: {e}"

# Button to get Revit version
if st.button("Get Revit Version"):
    version = get_revit_version()
    st.success(f"Revit Version: {version}")

# Button to get Wall Element Types
if st.button("Get Wall Parameter Data"):
    wall_types = export_wall_data()
    if isinstance(wall_types, list):
        st.write("### Wall Element Types:")
        for wt in wall_types:
            st.write(f"- {wt}")
    else:
        st.error(wall_types)

st.header("Bscore Prediction")
if st.button("Predict Bscore"):
    if 'filtered_columns' in st.session_state:
        cols = st.session_state['filtered_columns']
        st.write("Bscore prediction is in progress...")
        process_csv(cols)
        st.write("Bscore prediction is complete!")
        df = pd.read_csv(os.path.join(current, "temp", "results", "Wall_results.csv"))
        
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
    else:
        st.warning("Please filter columns first. If you do not have any filters in mind just click the 'Apply Filter' button eitherway.")
    

