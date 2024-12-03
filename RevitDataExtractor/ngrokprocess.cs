using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// For ngrok port forwarding for deployed streamlit.
/// </summary>

namespace RevitDataExtractor
{
    public class NgrokHelper
    {
        private Process ngrokProcess;

        public async Task<string> StartNgrok(int port)
        {
            // Start ngrok process
            ngrokProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ngrok",
                    Arguments = $"http {port} --host-header=\"localhost:{port}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            ngrokProcess.Start();

            // Wait for ngrok to initialize and get the public URL
            string url = await GetNgrokUrl();
            return url;
        }

        private async Task<string> GetNgrokUrl()
        {
            string url = null;
            int retries = 10;
            int delay = 500; // milliseconds

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        var response = await client.GetAsync("http://127.0.0.1:4040/");
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonString = await response.Content.ReadAsStringAsync();
                            var json = JsonConvert.DeserializeObject<NgrokApiResponse>(jsonString);
                            if (json.Tunnels.Count > 0)
                            {
                                url = json.Tunnels[0].PublicUrl;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore exceptions and retry
                }
                await Task.Delay(delay);
            }

            return url;
        }

        public void StopNgrok()
        {
            if (ngrokProcess != null && !ngrokProcess.HasExited)
            {
                ngrokProcess.Kill();
                ngrokProcess.Dispose();
                ngrokProcess = null;
            }
        }

        public class NgrokApiResponse
        {
            [JsonProperty("tunnels")]
            public List<Tunnel> Tunnels { get; set; }
        }

        public class Tunnel
        {
            [JsonProperty("public_url")]
            public string PublicUrl { get; set; }
        }
    }
}
