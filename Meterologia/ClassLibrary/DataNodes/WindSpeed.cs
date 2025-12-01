using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DataNodes
{
    public class WindSpeed : DataNode
    {
        // Base unit: m/s
        private static readonly IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> _converters
            = new Dictionary<string, (Func<double, double>, Func<double, double>)>(StringComparer.OrdinalIgnoreCase)
            {
                ["m/s"] = (v => v, v => v),
                ["km/h"] = (v => v / 3.6, v => v * 3.6), // 1 m/s = 3.6 km/h
                ["mph"] = (v => v * 0.44704, v => v / 0.44704), // 1 mph = 0.44704 m/s
                ["knot"] = (v => v * 0.514444, v => v / 0.514444), // 1 knot = 0.514444 m/s
                ["ft/s"] = (v => v * 0.3048, v => v / 0.3048), // 1 ft/s = 0.3048 m/s
            };

        protected override IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> UnitConverters => _converters;
        
        public WindSpeed(DateTime date, double valueInMetersPerSecond, SourceType source, string? sensor): base(date, valueInMetersPerSecond, source, sensor) { }
    }
}