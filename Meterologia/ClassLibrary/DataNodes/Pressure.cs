using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DataNodes
{
    public class Pressure : DataNode
    {
        // Base unit: hPa
        private static readonly IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> _converters
            = new Dictionary<string, (Func<double, double>, Func<double, double>)>(StringComparer.OrdinalIgnoreCase)
            {
                //             toBase (-> hPa)             fromBase (hPa ->)
                ["hpa"] = (v => v, v => v),
                ["pa"] = (v => v / 100.0, v => v * 100.0),
                ["kpa"] = (v => v * 10.0, v => v / 10.0),
                ["bar"] = (v => v * 1000.0, v => v / 1000.0),
                ["atm"] = (v => v * 1013.25, v => v / 1013.25),
                ["tor"] = (v => v * 1.33322, v => v / 1.33322),
                ["psi"] = (v => v * 68.9476, v => v / 68.9476),
            };

        protected override IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> UnitConverters => _converters;

        public Pressure(DateTime date, double valueInHpa, SourceType source, string? sensor): base(date, valueInHpa, source, sensor) { }
    }
}