using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public abstract class DataNode
    {
        public DateTime Date { get; set; }
        public double Value { get; protected set; }   // stored in base unit
        public SourceType Source { get; set; }
        public string? Sensor { get; set; }

        protected DataNode(DateTime date, double value, SourceType source, string? sensor)
        {
            Date = date;
            Value = value;
            Source = source;
            Sensor = sensor;
        }

        // Each subclass must provide its unit -> converters map
        protected abstract IReadOnlyDictionary<string, (Func<double, double> ToBase, Func<double, double> FromBase)> UnitConverters { get; }

        protected static string NormalizeUnit(string unit) => unit.Trim().ToLowerInvariant();

        public IEnumerable<string> ValidUnits => UnitConverters.Keys;

        public bool IsUnitSupported(string unit) => UnitConverters.ContainsKey(NormalizeUnit(unit));

        /// Convert INTO the base unit.
        public void SetFrom(string unit, double input)
        {
            var key = NormalizeUnit(unit);
            if (!UnitConverters.TryGetValue(key, out var conv)) throw new ArgumentOutOfRangeException(nameof(unit), "Unknown unit for this node type.");

            Value = conv.ToBase(input);
        }

        /// Get current Value converted FROM base unit into the requested unit.
        public double GetIn(string unit)
        {
            var key = NormalizeUnit(unit);
            if (!UnitConverters.TryGetValue(key, out var conv)) throw new ArgumentOutOfRangeException(nameof(unit), "Unknown unit for this node type.");

            return conv.FromBase(Value);
        }
    }
}
