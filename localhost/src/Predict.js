import React, { useState } from 'react';
import CsvTable from './csv_table';

function Predict({ disabled, onStartProcess, onFinishProcess }) {
  const [tableData, setTableData] = useState([]);
  const [isProcessing, setIsProcessing] = useState(false);

  const fetchCsvData = async () => {
    onStartProcess(); // Trigger loading state before starting processing
    setIsProcessing(true);
    try {
      const response = await fetch('/api/process_csv', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        const data = await response.json();
        setTableData(data);
      } else {
        console.error("Failed to fetch CSV data");
      }
    } catch (error) {
      console.error("Error:", error);
    } finally {
      onFinishProcess(); // Trigger loading state after finishing processing
      setIsProcessing(false);
    }
  };

  return (
    <div>
      <button onClick={fetchCsvData} disabled={isProcessing || disabled}>
        {isProcessing ? 'Processing...' : 'Process CSV'}
      </button>
      {tableData.length > 0 ? (
        <CsvTable data={tableData} />
      ) : (
        <p>No data available</p>
      )}
    </div>
  );
}

export default Predict;