using PKISharp.WACS.Configuration;
using PKISharp.WACS.Plugins.Base.Factories;
using PKISharp.WACS.Services;
using PKISharp.WACS.Services.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PKISharp.WACS.Plugins.ValidationPlugins.Dns
{
    public class HurricaneOptionsFactory : PluginOptionsFactory<HurricaneOptions>
    {
        private readonly ArgumentsInputService _arguments;
        public HurricaneOptionsFactory(ArgumentsInputService arguments) => _arguments = arguments;

        private ArgumentResult<ProtectedString?> DdnsKey => _arguments.
            GetProtectedString<HurricaneArguments>(a => a.HurricaneElectricDdnsKey).
            Required();

        public override async Task<HurricaneOptions?> Aquire(IInputService inputService, RunLevel runLevel)
        {
            return new HurricaneOptions
            {
                DdnsKey = await DdnsKey.Interactive(inputService, "Hurricane Electric DDNS Key").GetValue()
            };
        }

        public override async Task<HurricaneOptions?> Default()
        {
            return new HurricaneOptions
            {
                DdnsKey = await DdnsKey.GetValue()
            };
        }

        public override IEnumerable<(CommandLineAttribute, object?)> Describe(HurricaneOptions options)
        {
            yield return (DdnsKey.Meta, options.DdnsKey);
        }

    }
}