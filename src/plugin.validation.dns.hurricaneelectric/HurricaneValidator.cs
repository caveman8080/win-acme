using PKISharp.WACS.Clients.DNS;
using PKISharp.WACS.Plugins.Base.Capabilities;
using PKISharp.WACS.Plugins.Interfaces;
using PKISharp.WACS.Services;
using System;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Web;

// Optimized: Replaced generic Exception throw with InvalidOperationException for better exception specificity. Inlined content variable in GetTxtRecord method.

[assembly: SupportedOSPlatform("windows")]

namespace PKISharp.WACS.Plugins.ValidationPlugins.Dns
{
    [IPlugin.Plugin<
        HurricaneOptions, HurricaneOptionsFactory,
        DnsValidationCapability, HurricaneJson>
        ("a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "Hurricane Electric", "Create verification records in Hurricane Electric DNS using DynDNS API")]
    public class HurricaneValidator : DnsValidation<HurricaneValidator>, IDisposable
    {
        private readonly HurricaneOptions _options;
        private readonly SecretServiceManager _ssm;
        private readonly HttpClient _hc;

        public HurricaneValidator(
            HurricaneOptions options,
            IProxyService proxyService,
            LookupClientProvider dnsClient,
            SecretServiceManager ssm,
            ILogService log,
            ISettingsService settings) : base(dnsClient, log, settings)
        {
            _options = options;
            _hc = proxyService.GetHttpClient();
            _ssm = ssm;
        }

        private string GetKey() => _ssm.EvaluateSecret(_options.DdnsKey) ?? throw new InvalidOperationException("DDNS key is not set");

        private async Task<string> SendRequest(string hostname, string txt)
        {
            var key = GetKey();
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["hostname"] = hostname;
            query["password"] = key;
            query["txt"] = txt;
            var url = $"https://dyn.dns.he.net/nic/update?{query}";
            var response = await _hc.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadAsStringAsync()).Trim();
        }

        public override async Task<bool> CreateRecord(DnsValidationRecord record)
        {
            try
            {
                var response = await SendRequest(record.Authority.Domain, record.Value);
                if (response.StartsWith("good") || response.StartsWith("nochg"))
                {
                    _log.Information("Successfully created TXT record for {domain}", record.Authority.Domain);
                    return true;
                }
                else
                {
                    _log.Error("Failed to create TXT record for {domain}: {response}", record.Authority.Domain, response);
                    if (response.Contains("abuse"))
                    {
                        throw new InvalidOperationException("Abuse detected in Hurricane Electric API response");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error creating TXT record for {domain}: {message}", record.Authority.Domain, ex.Message);
                throw;
            }
        }

        public override async Task DeleteRecord(DnsValidationRecord record)
        {
            try
            {
                var response = await SendRequest(record.Authority.Domain, "");
                if (response.StartsWith("good") || response.StartsWith("nochg"))
                {
                    _log.Information("Successfully deleted TXT record for {domain}", record.Authority.Domain);
                }
                else
                {
                    _log.Warning("Failed to delete TXT record for {domain}: {response}", record.Authority.Domain, response);
                }
            }
            catch (Exception ex)
            {
                _log.Warning("Error deleting TXT record for {domain}: {message}", record.Authority.Domain, ex.Message);
            }
        }

        public void Dispose() => _hc.Dispose();
    }
}