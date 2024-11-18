import { useState } from "react";
import CsvTable from "./csv_table"; // Import CsvTable component

function FileUpload({ disabled, onStartProcess, onFinishProcess }) {
  const [selectedFile, setSelectedFile] = useState(null);
  const [tableData, setTableData] = useState([]); // State to hold processed table data

  const handleFileChange = (event) => {
    if (!disabled) {
      setSelectedFile(event.target.files[0]);
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (disabled) return; // Prevent form submission if disabled

    onStartProcess(); // Trigger loading state before uploading

    if (!selectedFile) {
      alert("Please select a file first!");
      onFinishProcess();
      return;
    }

    const formData = new FormData();
    formData.append("file", selectedFile);

    try {
      const response = await fetch(`/api/upload`, {
        method: "POST",
        body: formData,
      });

      if (response.ok) {
        const data = await response.json();
        setTableData(data); // Set the processed data to be displayed in the table
      } else {
        console.error("Failed to upload file");
      }
    } catch (error) {
      console.error("Error:", error);
    } finally {
      onFinishProcess(); // Trigger loading state after finishing processing
    }
  };

  return (
    <div>
      <form onSubmit={handleSubmit}>
        <input
          type="file"
          accept=".csv"
          onChange={handleFileChange}
          disabled={disabled} // Disable file input
        />
        <button type="submit" disabled={disabled}>
          Upload and Process
        </button>
      </form>

      {tableData.length > 0 && <CsvTable data={tableData} />} {/* Pass tableData to CsvTable */}
    </div>
  );
}

export default FileUpload;
