import React, { useState } from 'react';
import ColFilter from './ColFilter';
import FileUpload from './FileUpload';

function App() {
  const [isProcessing, setIsProcessing] = useState(false);

  const handleStartProcess = () => {
    if (isProcessing) return; // Prevent starting if already processing
    setIsProcessing(true);
  };

  const handleFinishProcess = () => {
    setIsProcessing(false);
  };

  return (
    <div className="App">
      <ColFilter
        disabled={isProcessing}
        onStartProcess={handleStartProcess}
        onFinishProcess={handleFinishProcess}
      />
      <br />
      <FileUpload
        disabled={isProcessing}
        onStartProcess={handleStartProcess}
        onFinishProcess={handleFinishProcess}
      />
      {isProcessing && <div id="loading-icon">Loading...</div>}
    </div>
  );
}

export default App;
