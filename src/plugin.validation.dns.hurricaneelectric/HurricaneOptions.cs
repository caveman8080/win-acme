using PKISharp.WACS.Plugins.Base.Options;
using PKISharp.WACS.Services.Serialization;
using System.Text.Json.Serialization;

namespace PKISharp.WACS.Plugins.ValidationPlugins.Dns
{
    [JsonSerializable(typeof(HurricaneOptions))]
    internal partial class HurricaneJson : JsonSerializerContext
    {
        public HurricaneJson(WacsJsonPluginsOptionsFactory optionsFactory) : base(optionsFactory.Options) { }
    }

    public class HurricaneOptions : ValidationPluginOptions
    {
        public ProtectedString? DdnsKey { get; set; }
    }
}