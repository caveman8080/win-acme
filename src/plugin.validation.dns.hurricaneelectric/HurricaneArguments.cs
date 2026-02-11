using PKISharp.WACS.Configuration;
using PKISharp.WACS.Configuration.Arguments;

namespace PKISharp.WACS.Plugins.ValidationPlugins
{
    public class HurricaneArguments : BaseArguments
    {
        public override string Name => "HurricaneElectric";
        public override string Group => "Validation";
        public override string Condition => "--validation hurricaneelectric";

        [CommandLine(Description = "DDNS Key for Hurricane Electric DNS.", Secret = true)]
        public string? HurricaneElectricDdnsKey { get; set; }
    }
}