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
        private static readonly string host = @"127.0.0.1";
        private static readonly string port = @"8989";

        public async Task<string> TestPostAsync(Dictionary<string, string> payload)
        {
            var content = new FormUrlEncodedContent(payload);
            var response = await client.PostAsync(host, content);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public async Task<string> TestGet()
        {
            var responseString = await client.GetStringAsync(host);
            return responseString;
        }
    }
}
