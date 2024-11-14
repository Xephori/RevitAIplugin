import React, { useState } from "react";

function ColFilter({ disabled, onStartProcess, onFinishProcess }) {
  const [columnInput, setColumnInput] = useState("");

  const handleInputChange = (event) => {
    setColumnInput(event.target.value);
  };

  const handleButtonClick = async () => {
    if (disabled) return; // Prevent action if disabled

    onStartProcess(); // Trigger loading state before fetching data

    try {
      const response = await fetch("/api/filter_columns", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ columns: columnInput }),
      });

      if (!response.ok) {
        throw new Error("Network response was not ok " + response.statusText);
      }
    } catch (error) {
      console.error("Error:", error);
    } finally {
      onFinishProcess(); // Trigger loading state after finishing processing
    }
  };

  return (
    <div id="colFilterContainer">
      <input
        type="text"
        value={columnInput}
        onChange={handleInputChange}
        placeholder="Enter columns (e.g., name, age, city)"
        disabled={disabled} // Disable input when necessary
      />
      <button onClick={handleButtonClick} disabled={disabled}>
        Filter Columns
      </button>
    </div>
  );
}

export default ColFilter;
