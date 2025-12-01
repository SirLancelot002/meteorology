using ClassLibrary.DataNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class MeasurementSystem
    {
        public Dictionary<Type, LinkedList<DataNode>> DataNodesByType { get; }

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

            var last = list.Last!;
            if (node.Date >= last.Value.Date) {list.AddLast(node); return;}

            var first = list.First!;
            if (node.Date <= first.Value.Date) {list.AddFirst(node); return;}

            var distanceToEnd = (last.Value.Date - node.Date).Ticks; //These are for getting the best way to insert it.
            var distanceToStart = (node.Date - first.Value.Date).Ticks;//These assume that nodes are about equally distributed in time

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
                            double.TryParse(data, out value);
                            break;
                        case "unit":
                            unit = data;
                            break;
                        case "sensor":
                            sensor = data;
                            break;
                    }
                }
                if (timestamp > DateTime.MinValue && unit != null && value > double.MinValue)
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

        public void ImportFromFileOLD(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) {
                Console.WriteLine("Invalid file name/path");
                return;
            }
            if (!File.Exists(path)) {
                Console.WriteLine("File could not be found");
                return;
            }

            string content;
            try {content = File.ReadAllText(path);}
            catch {
                Console.WriteLine("File could not be read for some reason");
                return;
            }

            var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
            List<RawMeasurement>? rows = null;

            // Try parse as full JSON array first
            try {rows = JsonSerializer.Deserialize<List<RawMeasurement>>(content, options);}
            catch {rows = null;}

            // If top-level array parse failed, attempt to parse line-by-line
            if (rows == null) {
                var candidates = new List<RawMeasurement>();
                using var sr = new StringReader(content);
                string? line;
                while ((line = sr.ReadLine()) != null) {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var trimmed = line.Trim();
                    if (trimmed.EndsWith(",")) trimmed = trimmed.Substring(0, trimmed.Length - 1);

                    try {
                        var obj = JsonSerializer.Deserialize<RawMeasurement>(trimmed, options);
                        if (obj != null) candidates.Add(obj);
                    }
                    catch {
                        // Try to be extra tolerant: if the line contains a JSON object somewhere inside, try to extract
                        int start = trimmed.IndexOf('{');
                        int end = trimmed.LastIndexOf('}');
                        if (start >= 0 && end > start) {
                            var sub = trimmed.Substring(start, end - start + 1);
                            try {
                                var obj2 = JsonSerializer.Deserialize<RawMeasurement>(sub, options);
                                if (obj2 != null) candidates.Add(obj2);
                            }
                            catch {
                                // give up on this line
                            }
                        }
                    }
                }
                if (candidates.Count > 0) rows = candidates;
            }

            if (rows == null || rows.Count == 0){
                Console.WriteLine("The File was read, but there was no useful data.");
                return;
            }

            // Process each row: try to create a DataNode and add it. Skip bad rows.
            foreach (var raw in rows) {
                try {
                    if (raw.timestamp == default || string.IsNullOrWhiteSpace(raw.unit)) continue;

                    var node = DataNodeFactory.TryCreate(raw);
                    if (node != null) {
                        AddNode(node);
                    }
                }
                catch {
                    // skip this row on any unexpected error
                }
            }
        }
    }
}
