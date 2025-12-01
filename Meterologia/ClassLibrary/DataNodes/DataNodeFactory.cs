using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DataNodes
{
    public static class DataNodeFactory
    {
        private static readonly IReadOnlyList<Type> _subclasses;

        static DataNodeFactory() {
            _subclasses = typeof(DataNode).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DataNode))).ToList();
        }

        private static string NormalizeUnit(string unit) => unit.Trim().ToLowerInvariant();

        // Returns true if the concrete subclass supports the unit (by checking its static _converters)
        private static bool TypeSupportsUnit(Type nodeType, string unit) {
            var field = nodeType.GetField("_converters", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null) return false;

            var converters = field.GetValue(null) as IReadOnlyDictionary<string, (Func<double, double>, Func<double, double>)>;
            if (converters == null) return false;

            return converters.ContainsKey(NormalizeUnit(unit));
        }

        public static DataNode? TryCreate(RawMeasurement raw)
        {
            try {
                if (!Enum.TryParse<SourceType>(raw.source, true, out var source)) {
                    return null;
                }

                var unitNorm = NormalizeUnit(raw.unit);

                //var nodeType = _subclasses.FirstOrDefault(t => TypeSupportsUnit(t, unitNorm));
                var nodeType = _subclasses.FirstOrDefault(t => DataNode.IsUnitSupportedForType(t, unitNorm));
                if (nodeType == null) return null;

                var instance = Activator.CreateInstance(nodeType, raw.timestamp, 0.0, source, raw.sensor) as DataNode;
                if (instance == null) return null;

                instance.SetFrom(raw.unit, raw.value);
                return instance;
            }
            catch {
                return null;
            }
        }
    }
}
