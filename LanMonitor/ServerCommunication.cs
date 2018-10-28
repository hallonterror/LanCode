using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace LanMonitor
{
    class ServerCommunication
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string host = @"http://127.0.0.1:5000";

        public async Task<string> Post(string command, Dictionary<string, string> payload)
        {
            var content = new FormUrlEncodedContent(payload);
            var response = await client.PostAsync(host + command, content).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public async Task<string> Get(string command)
        {
            var responseString = await client.GetStringAsync(host + command).ConfigureAwait(false);
            return responseString;
        }
    }
}