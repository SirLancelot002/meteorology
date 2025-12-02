using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DataNodes
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

        public static bool IsUnitSupportedForType(Type nodeType, string unit)
        {//Sadly I could not turn "IsUnitSupported" into a static function, so I had to make this...
            if (!typeof(DataNode).IsAssignableFrom(nodeType))
                return false;

            var field = nodeType.GetField("_converters",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (field == null)
                return false;

            var converters =
                field.GetValue(null) as IReadOnlyDictionary<string, (Func<double, double>, Func<double, double>)>;

            if (converters == null)
                return false;

            return converters.ContainsKey(NormalizeUnit(unit));
        }

        public override string ToString()
        {//This function is for debugging
            string nodeType = GetType().Name;

            return $"{nodeType}\t\"Timestamp\" : {Date:O}\t\"Value\" : {Value}\t\"Source\" : {Source}\t\"Sensor\" : {Sensor}";
        }

        public string ToFileLine(string unit)
        {
            string sensorPart = Sensor != null ? $", \"sensor\": \"{Sensor}\"" : "";
            return $"{{ \"timestamp\": \"{Date.ToString("yyyy-MM-ddTHH:mm:ssK")}\", \"value\": {GetIn(unit).ToString("F2", CultureInfo.InvariantCulture)}, \"unit\": \"{unit}\"{sensorPart} }}";
        }
    }
}