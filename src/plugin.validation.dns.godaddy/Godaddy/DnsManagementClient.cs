using PKISharp.WACS.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

// Optimized: Replaced generic Exception throws with InvalidOperationException for better exception specificity. Inlined content variables and removed unused reads in CreateRecord and DeleteRecord methods.

namespace PKISharp.WACS.Plugins.ValidationPlugins.Godaddy
{

    public class DnsManagementClient
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly ILogService _log;
        readonly IProxyService _proxyService;
        private readonly string uri = "https://api.godaddy.com/";

        public DnsManagementClient(string apiKey, string apiSecret, ILogService logService, IProxyService proxyService)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _log = logService;
            _proxyService = proxyService;
        }

        public async Task CreateRecord(string domain, string identifier, RecordType type, string value)
        {
            using (var client = _proxyService.GetHttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(_apiSecret))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"sso-key {_apiKey}:{_apiSecret}");
                } 
                else
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"sso-key {_apiKey}");
                }
                var putData = new List<object>() { new { ttl = 600, data = value } };
                var serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(putData);

                //Record successfully created
                // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                var typeTxt = type.ToString();
                var httpContent = new StringContent(serializedObject, Encoding.UTF8, "application/json");
                var buildApiUrl = $"v1/domains/{domain}/records/{typeTxt}/{identifier}";
                var response = await client.PutAsync(buildApiUrl, httpContent);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                {
                    // Response content not needed
                }
                else
                {
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
                }


            };
        }

        public async Task DeleteRecord(string domain, string identifier, RecordType type)
        {
            using (var client = _proxyService.GetHttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(_apiSecret))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"sso-key {_apiKey}:{_apiSecret}");
                }
                else
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"sso-key {_apiKey}");
                }
                var typeTxt = type.ToString();
                var buildApiUrl = $"v1/domains/{domain}/records/{typeTxt}/{identifier}";

                _log.Information("Godaddy API with: {0}", buildApiUrl);

                var response = await client.DeleteAsync(buildApiUrl);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                {
                    // Response content not needed (commented logging)
                }
                else
                {
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
                }
            };
        }
    }
}