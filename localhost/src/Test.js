// Test.js
import React, { useEffect } from 'react';
  
  function getRevitVersion() {
    console.log("Getting Revit version!");
    const wv2msg = {"action": "GetVersion", "payload": {"msg": "give me my version!"}};
    try {
      const w = window;
      w.chrome?.webview?.postMessage(wv2msg);
    } catch (err) {
      console.error(err);
    }
  }
  
  function showRevitVersion(e) {
    console.log(e);
    const ver = e.detail.version;
    const answer = document.querySelector("#ver-res");
    answer.value = ver;
  }
  
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
  
  function showWallData(e) {
    console.log(e);
    const wallData = e.detail;
    const answer = document.querySelector("#wall-data-res");
    answer.value = JSON.stringify(wallData, null, 2); // Format the JSON for readability
  }
  
  function initRevitListeners() {
    document.addEventListener("GetVersion", showRevitVersion);
    document.addEventListener("GetWallData", showWallData);
  }
  
  function Test() {
    useEffect(() => {
      initRevitListeners();
    }, []);
  
    return (
      <div>
        <button onClick={getRevitVersion}>Check Revit Version</button>
        <textarea id="ver-res" type="text"></textarea>
        <br />
        <button onClick={getWallData}>Get Wall Data</button>
      </div>
    );
  }
  
  export default Test;
