using ClassLibrary.DataNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class MeasurementSystem
    {
        public Dictionary<Type, LinkedList<DataNode>> DataNodesByType { get; }
        private static readonly Random rng = new Random();

        public MeasurementSystem() {
            DataNodesByType = new Dictionary<Type, LinkedList<DataNode>>();
        }

        private void AddNode(DataNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));//We should never get to this line, but a just-in-case defense
            var t = node.GetType();
            if (!DataNodesByType.TryGetValue(t, out var list))
            {
                list = new LinkedList<DataNode>();
                DataNodesByType[t] = list;
            }

            InsertInOrder(list, node);
        }

        private static void InsertInOrder(LinkedList<DataNode> list, DataNode node)
        {
            if (list.Count == 0) {list.AddFirst(node); return;} //If the list was just created

            var distanceToEnd = (list.Last!.Value.Date - node.Date).Ticks; //These are for getting the best way to insert it.
            var distanceToStart = (node.Date - list.First!.Value.Date).Ticks;//These assume that nodes are about equally distributed in time

            //I know a binary-halving strategy would be more effective, but that can't be done on linked list, but we need it for effective insert.
            if (distanceToEnd < distanceToStart) {
                var cur = list.Last;
                while (cur != null) {
                    if (cur.Value.Date <= node.Date) {list.AddAfter(cur, node); return;}
                    cur = cur.Previous;
                }
            }
            else {
                var cur = list.First;
                while (cur != null) {
                    if (cur.Value.Date >= node.Date) {list.AddBefore(cur, node); return;}
                    cur = cur.Next;
                }
            }

            // Fallback (shouldn't happen)
            list.AddLast(node);
        }
        public void Clear() => DataNodesByType.Clear();

        public void Generate(double minValue, double maxValue, DateTime start, DateTime end, string unit)
        {
            double stepsToEdge = 5; //This is the cycles the code needs to go from the avg value to an edge
            DateTime time = start;

            double diff = maxValue - minValue;//This segment is to make the random generated values a bit more belivable.
            double value = (rng.NextDouble() * diff) + minValue;
            double maxStep = diff / stepsToEdge;//Instead of having a bunch of random values, one random value is always modifed by a random delta
            int cycle = 0;

            while (time < end) {
                RawMeasurement raw = new RawMeasurement();
                raw.value = value;
                raw.unit = unit;
                raw.timestamp = time;
                raw.source = SourceType.GENERATED;
                raw.sensor = $"Generated_{cycle}";
                DataNode? node = DataNodeFactory.TryCreate(raw);
                if (node is not null) AddNode(node);

                cycle++;
                value += (rng.NextDouble() * 2.0 - 1.0) * maxStep;
                value = value < minValue ? minValue : (value > maxValue ? maxValue : value);
                time = time.AddHours(1);
            }
        }

        public void ImportFromFile(string path) {
            if (!File.Exists(path)) {
                Console.WriteLine("File could not be found");
                return;
            }

            string content;
            try { content = File.ReadAllText(path); }
            catch {
                Console.WriteLine("File could not be read for some reason");
                return;
            }

            int successes = 0;
            int fails = 0;
            string[] rows = content.Trim('[', ']').Trim().Split('{');
            foreach (string row in rows)
            {
                DateTime timestamp = DateTime.MinValue;
                double value = double.MinValue;
                string? unit = null;
                string? sensor = null;
                bool valueRead = false;

                string[] dataPairs = row.Split(',');
                foreach (string dataPair in dataPairs)
                {
                    string[] label_Data = dataPair.Split(":",2);
                    if (label_Data.Length < 2) { continue; }
                    string label = label_Data[0].Trim().Trim('\"');
                    string data = label_Data[1].Trim().Trim('\"');
                    data = data.TrimEnd(' ', '"', '}', ']', '\n', '\r', '\t');
                    switch (label.ToLower())
                    {
                        case "timestamp":
                            DateTime.TryParse(data, out timestamp);
                            break;
                        case "value":
                            valueRead = double.TryParse(data, out value);
                            break;
                        case "unit":
                            unit = data;
                            break;
                        case "sensor":
                            sensor = data;
                            break;
                    }
                }
                if (timestamp > DateTime.MinValue && unit != null && valueRead)
                {
                    RawMeasurement raw = new RawMeasurement();
                    raw.timestamp = timestamp;
                    raw.unit = unit;
                    raw.sensor = sensor;
                    raw.value = value;
                    raw.source = SourceType.IMPORTED;

                    var node = DataNodeFactory.TryCreate(raw);
                    if (node != null) { AddNode(node); successes++; }
                    else { fails++;}
                }
                else { fails++;}
            }

            Console.WriteLine($"Reading the file: {path}\nResulted in {successes} successes and {fails} failures");
        }

        public void ExportToFile(string path)
        {//I know we didn't had to do this, but we did not get large input files to test on, so I need this to make test files
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("[");
                bool first = true;
                Random rand = new Random();

                foreach (var kvp in DataNodesByType)
                {
                    foreach (var node in kvp.Value)
                    {
                        if (!first) writer.WriteLine(",");

                        var t = node.GetType();
                        var field = t.GetField("_converters", BindingFlags.NonPublic | BindingFlags.Static);

                        string unit = "";
                        if (field != null)
                        {
                            var converters = field.GetValue(null) as System.Collections.Generic.IReadOnlyDictionary<string, (Func<double, double>, Func<double, double>)>;
                            if (converters != null && converters.Keys.Any())
                            {
                                var keys = converters.Keys.ToList();
                                unit = keys[rand.Next(keys.Count)];
                            }
                        }

                        writer.Write("  " + node.ToFileLine(unit));
                        first = false;
                    }
                }

                writer.WriteLine();
                writer.WriteLine("]");
            }
        }
    }
}
