import React, { useEffect, useState } from 'react';

function getWallData() {
  console.log("Getting wall data!");
  const wv2msg = {"action": "GetWallData", "payload": {"msg": "give me wall data!"}};
  try {
    const w = window;
    w.chrome?.webview?.postMessage(wv2msg);
  } catch (err) {
    console.error(err);
  }
}

function Extraction({ disabled, onStartProcess, onFinishProcess }) {
  const [isProcessing, setIsProcessing] = useState(false);
  const [message, setMessage] = useState('');
  const [wallData, setWallData] = useState('');

  function showWallData(e) {
    onStartProcess(); // Trigger loading state before starting processing
    setIsProcessing(true);
    console.log(e);
    const wallData = e.detail;
    // const answer = document.querySelector("#wall-data-res");
    setWallData(JSON.stringify(wallData, null, 2)); // Format the JSON for readability
    setMessage('Extraction finished'); // Update the message state
    onFinishProcess(); // Trigger loading state after finishing processing
    setIsProcessing(false);
  }

  function initRevitListeners() {
    document.addEventListener("GetWallData", showWallData);
  }

  useEffect(() => {
    initRevitListeners();
  }, []);

  return (
    <div>
      <button onClick={getWallData} disabled={isProcessing || disabled}>Extract Wall Data</button>
      <pre id="wall-data-res">{wallData}</pre>
      {message && <p>{message}</p>} {/* Conditionally render the message */}
    </div>
  );
}

export default Extraction;