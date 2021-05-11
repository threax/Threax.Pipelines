using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.CheapAzure.Services
{
    public class MachineIpManager : IMachineIpManager
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<MachineIpManager> logger;

        public MachineIpManager(HttpClient httpClient, ILogger<MachineIpManager> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<String> GetExternalIp()
        {
            var ipInfoHost = "http://ipinfo.io/json";
            using (var result = await httpClient.GetAsync(ipInfoHost))
            {
                if (!result.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Could not get public ip from '{ipInfoHost}'.");
                }
                var json = await result.Content.ReadAsStringAsync();
                var jobj = JObject.Parse(json);
                return jobj["ip"]?.ToString();
            }
        }
    }
}
