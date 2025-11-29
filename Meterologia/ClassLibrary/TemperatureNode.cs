using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class TemperatureNode : DataNode
    {
        // Base: Kelvin
        private static readonly IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> _converters
            = new Dictionary<string, (Func<double, double>, Func<double, double>)>(StringComparer.OrdinalIgnoreCase)
            {
                // Kelvin
                ["k"] = (v => v, v => v),
                ["°k"] = (v => v, v => v),  // optional
                ["°c"] = (c => c + 273.15, k => k - 273.15),
                ["°f"] = (f => (f - 32) * 5.0 / 9.0 + 273.15, k => (k - 273.15) * 9.0 / 5.0 + 32),
            };

        protected override IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> UnitConverters => _converters;

        public TemperatureNode(DateTime date, double valueInKelvin, SourceType source, string? sensor) : base(date, valueInKelvin, source, sensor) { }
    }
}
