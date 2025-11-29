using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Humidity : DataNode
    {
        private static readonly IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> _converters
            = new Dictionary<string, (Func<double, double>, Func<double, double>)>(StringComparer.OrdinalIgnoreCase)
            {
                ["%"] = (v => v, v => v), //base
                ["fraction"] = (v => v * 100.0, v => v / 100.0), // fraction 0–1
            };

        protected override IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> UnitConverters => _converters;

        public Humidity(DateTime date, double valueInPercent, SourceType source, string? sensor) : base(date, valueInPercent, source, sensor) { }
    }
}
