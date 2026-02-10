using PKISharp.WACS.Clients.DNS;
using PKISharp.WACS.Plugins.Base.Capabilities;
using PKISharp.WACS.Plugins.Interfaces;
using PKISharp.WACS.Services;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Optimized: Inlined payload variables in GetZones and CreateRecord methods to reduce unnecessary variable declarations.

[assembly: SupportedOSPlatform("windows")]

namespace PKISharp.WACS.Plugins.ValidationPlugins.Dns
{
    [IPlugin.Plugin<
        LuaDnsOptions, LuaDnsOptionsFactory, 
        DnsValidationCapability, LuaDnsJson>
        ("3b0c3cca-db98-40b7-b678-b34791070d42", 
        "LuaDns", 
        "Create verification records in LuaDns")]
    internal sealed class LuaDns : DnsValidation<LuaDns>
    {
        private class ZoneData
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        private class RecordData
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("zone_id")]
            public int ZoneId { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }

            [JsonPropertyName("ttl")]
            public int TTL { get; set; }
        }

        private static readonly Uri _LuaDnsApiEndpoint = new("https://api.luadns.com/v1/", UriKind.Absolute);
        private static readonly ConcurrentDictionary<string, RecordData> _recordsMap = new ConcurrentDictionary<string, RecordData>();
        private static readonly object _recordsMapLock = new();
        private readonly IProxyService _proxyService;

        private readonly string? _userName;
        private readonly string? _apiKey;

        public LuaDns(
            LookupClientProvider dnsClient,
            IProxyService proxy,
            ILogService log,
            ISettingsService settings,
            SecretServiceManager ssm,
            LuaDnsOptions options): base(dnsClient, log, settings)
        {
            _proxyService = proxy;
            _userName = options.Username;
            _apiKey = ssm.EvaluateSecret(options.APIKey);
        }

        public override async Task<bool> CreateRecord(DnsValidationRecord record)
        {
            _log.Information("Creating LuaDNS verification record");

            using var client = GetClient();
            var response = await client.GetAsync(new Uri(_LuaDnsApiEndpoint, "zones"));
            if (!response.IsSuccessStatusCode)
            {
                _log.Error("Failed to get DNS zones list for account. Aborting.");
                return false;
            }

            var zones = JsonSerializer.Deserialize<ZoneData[]>(await response.Content.ReadAsStringAsync());
            if (zones == null || zones.Any(x => x.Name == null))
            {
                _log.Error("Empty or invalid response. Aborting");
                return false;
            }
            var targetZone = FindBestMatch(zones.ToDictionary(x => x.Name!), record.Authority.Domain);
            if (targetZone == null)
            {
                _log.Error("No matching zone found in LuaDNS account. Aborting");
                return false;
            }

            var newRecord = new RecordData { Name = $"{record.Authority.Domain}.", Type = "TXT", Content = record.Value, TTL = 300 };
            var payload = JsonSerializer.Serialize(newRecord);

            response = await client.PostAsync(new Uri(_LuaDnsApiEndpoint, $"zones/{targetZone.Id}/records"), new StringContent(payload, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                _log.Error("Failed to create DNS verification record");
                return false;
            }

            var responsePayload = await response.Content.ReadAsStringAsync();
            newRecord = JsonSerializer.Deserialize<RecordData>(responsePayload);
            if (newRecord == null)
            {
                _log.Error("Empty or invalid response");
                return false;
            }
            lock (_recordsMapLock)
            {
                _recordsMap[record.Authority.Domain] = newRecord;
            }
            return true;
        }

        public override async Task DeleteRecord(DnsValidationRecord record)
        {
            RecordData? created;
            if (!_recordsMap.TryGetValue(record.Authority.Domain, out created))
            {
                _log.Warning($"No record with name {record.Authority.Domain} was created");
                return;
            }
            _recordsMap.TryRemove(record.Authority.Domain, out _);

            _log.Information("Deleting LuaDNS verification record");

            using var client = GetClient();
            var response = await client.DeleteAsync(new Uri(_LuaDnsApiEndpoint, $"zones/{created.ZoneId}/records/{created.Id}"));
            if (!response.IsSuccessStatusCode)
            {
                _log.Warning("Failed to delete DNS verification record");
                return;
            }
            
            _recordsMap.TryRemove(record.Authority.Domain, out _);
        }

        private HttpClient GetClient()
        {
            var client = _proxyService.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_userName}:{_apiKey}")));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}