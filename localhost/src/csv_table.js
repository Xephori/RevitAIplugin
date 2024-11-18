import React from "react";

function CsvTable({ data }) {
  return (
    <table border="1">
      <thead>
        <tr>
          {Object.keys(data[0]).map((header) => (
            <th key={header}>{header}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {data.map((row, index) => (
          <tr key={index}>
            {Object.values(row).map((value, i) => (
              <td key={i}>{value}</td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default CsvTable;
