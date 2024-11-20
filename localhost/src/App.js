import React, { useState } from 'react';
import ColFilter from './ColFilter';
// import FileUpload from './FileUpload';
import Test from './Test';
import Extraction from './Extraction';
import Predict from './Predict';

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
      <Extraction 
        disabled={isProcessing}
        onStartProcess={handleStartProcess}
        onFinishProcess={handleFinishProcess}
      />
      <br />
      <Predict 
        disabled={isProcessing}
        onStartProcess={handleStartProcess}
        onFinishProcess={handleFinishProcess}
      />
      <Test />
      {isProcessing && <div id="loading-icon">Loading...</div>}
    </div>
  );
}

export default App;
