import streamlit as st
import pandas as pd
import os
import sys
import io
import xlsxwriter
import csv
import re

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
    

# st.header("1. Filter Columns")

# # Input for column names
# columns_input = st.text_input(
#     "Enter columns to filter (comma-separated, e.g., name, age, city):",
#     ""
# )

# # Button to apply column filter
# if st.button("Apply Filter"):
#     if columns_input:
#         # Split and strip column names
#         columns = [col.strip() for col in columns_input.split(",")]
#         st.session_state['filtered_columns'] = columns
#         st.success(f"Columns to filter: {columns}")
#     else:
#         st.session_state['filtered_columns'] = ["buildability", "bdas", "bscore"]
    
# st.header("2. Upload and Process CSV")

# # File uploader for CSV
# uploaded_file = os.path.join(parent, "temp", "Wall_uh.csv")

# if uploaded_file:
#     try:
#         # Save uploaded file to a temporary location
#         df = pd.read_csv(uploaded_file)
#         st.session_state['original_df'] = df
#         st.write("### Original DataFrame", df)

#         # Apply column filter if columns are selected
#         if 'filtered_columns' in st.session_state:
#             filtered_columns = st.session_state['filtered_columns']
#             filtered_df = col_filter(df, filtered_columns)
#             st.session_state['filtered_df'] = filtered_df
#             st.write("### Filtered DataFrame", filtered_df)
#         else:
#             st.info("No columns filtered. Displaying original DataFrame.")
#     except Exception as e:
#         st.error(f"Error processing CSV file: {e}")
    
# st.header("3. Predict and Extract Data")

# if 'filtered_df' in st.session_state:
#     if st.button("Run Prediction"):
#         try:
#             # Perform AI predictions or data extraction
#             processed_df = label(st.session_state['filtered_df'])
#             st.session_state['processed_df'] = processed_df
#             st.write("### Processed DataFrame", processed_df)
#             st.success("Data processing complete.")
#         except Exception as e:
#             st.error(f"Error during data processing: {e}")
# else:
#     st.warning("Please upload and filter CSV columns first in the 'Process CSV' section.")
    
# st.header("4. Test Component")

# test_input = st.text_input("Enter some text for testing:", "")
# if st.button("Run Test"):
#     st.write(f"You entered: {test_input}")

# # Display Processed Data (Optional)
# if 'processed_df' in st.session_state:
#     st.header("5. View Processed Data")
#     st.dataframe(st.session_state['processed_df'])

#     # Download Processed Data as CSV
#     csv = st.session_state['processed_df'].to_csv(index=False)
#     st.download_button(
#         label="Download Processed CSV",
#         data=csv,
#         file_name='processed_data.csv',
#         mime='text/csv',
#     )