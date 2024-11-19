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

parent = os.path.dirname(current)

sys.path.append(parent)

from helpers import process_csv

# Streamlit App Title
st.title("CSV Column Filter and Processor")

st.header("Welcome to the CSV Processor App!")
st.write("""
    - **Filter Columns**: Select specific columns from your CSV.
    - **Process CSV**: Upload and process your CSV files.
""")

st.header("Filter Columns")

# Input for column names
columns_input = st.text_input(
    "Enter columns to filter (comma-separated, e.g., name, age, city):",
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
    filepath = os.path.join(parent, "temp", "Wall_uh.csv")
    temp3 = os.path.join(parent, "temp", "intermediates", "filtered.csv")

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

st.header("Bscore prediction")
if st.button("Predict Bscore"):
    if 'filtered_columns' in st.session_state:
        cols = st.session_state['filtered_columns']
        st.write("Bscore prediction is in progress...")
        process_csv(cols)
        st.write("Bscore prediction is complete!")
        df = pd.read_csv(os.path.join(parent, "files", "results", "Wall_results.csv"))
        
        # Display the DataFrame in Streamlit
        st.dataframe(df)
        
        # Convert the DataFrame to an Excel file in memory
        towrite = io.BytesIO()
        df.to_excel(towrite, index=False, engine='xlsxwriter')
        towrite.seek(0)  # Reset the pointer to the beginning of the stream
        
        # Provide a download button for the Excel file
        st.download_button(
            label="Download Results as Excel",
            data=towrite,
            file_name='Wall_results.xlsx',
            mime='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        )
    else:
        st.warning("Please filter columns first. If you do not have any filters in mind just click the 'Apply Filter' button eitherway.")
    

st.header("Revit Information Retrieval")

def get_revit_version():
    url = "http://localhost:5000/api/Revit/GetVersion"
    try:
        response = requests.get(url)
        response.raise_for_status()
        return response.json().get("version", "No version returned")
    except requests.exceptions.RequestException as e:
        return f"Error: {e}"

def get_wall_element_types():
    url = "http://localhost:5000/api/Revit/GetWallElementTypes"
    try:
        response = requests.get(url)
        response.raise_for_status()
        wall_types = response.json().get("wallTypes", [])
        return wall_types
    except requests.exceptions.RequestException as e:
        return f"Error: {e}"

# Button to get Revit version
if st.button("Get Revit Version"):
    version = get_revit_version()
    st.success(f"Revit Version: {version}")

# Button to get Wall Element Types
if st.button("Get Wall Element Types"):
    wall_types = get_wall_element_types()
    if isinstance(wall_types, list):
        st.write("### Wall Element Types:")
        for wt in wall_types:
            st.write(f"- {wt}")
    else:
        st.error(wall_types)