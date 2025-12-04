using ClassLibrary;
using ClassLibrary.DataNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Program
{
    public class User
    {
        public MeasurementSystem mySystem;
        protected List<Action> Commands = new List<Action>();
        protected Dictionary<Type, string> BaseUnits = new Dictionary<Type, string>();

        public User(MeasurementSystem mySystem)
        {
            this.mySystem = mySystem;
            Commands.Add(loadData);
            Commands.Add(exportData);
            Commands.Add(show);

            var subTypes = typeof(DataNode).Assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DataNode))).ToList();
            foreach (var subType in subTypes) {
                var field = subType.GetField("_converters", BindingFlags.NonPublic | BindingFlags.Static);

                string unit = "";
                if (field != null)
                {
                    var converters = field.GetValue(null) as System.Collections.Generic.IReadOnlyDictionary<string, (Func<double, double>, Func<double, double>)>;
                    if (converters != null && converters.Keys.Any())
                    {
                        var keys = converters.Keys.ToList();
                        unit = keys[0];
                    }
                }
                BaseUnits.Add(subType, unit);
            }
        }

        public User(MeasurementSystem mySystem, Dictionary<Type, string> baseUnits)
        {
            this.mySystem = mySystem;
            BaseUnits = baseUnits;
            Commands.Add(loadData);
            Commands.Add(exportData);
            Commands.Add(show);

        }

        public User(Admin myAdmin)
        {
            this.mySystem = myAdmin.mySystem;
            Commands.Add(loadData);
            Commands.Add(exportData);
            Commands.Add(show);
            Commands.Add(analyse);

            BaseUnits = myAdmin.BaseUnits;
        }

        public bool choose()
        {
            return false;
        }

        protected void analyse()
        {
            string answer1 = "";

            do
            {
                Console.WriteLine("\nDo you want an overall analysis or a day-by-day (overall/day)?");
                answer1 = Console.ReadLine().ToLower().Trim();
            } while (!(answer1 == "overall" || answer1 == "day"));

            string answer2 = "";

            do
            {
                Console.WriteLine("\nEnter the type of analysis you want (min/max/avg/count)?");
                answer2 = Console.ReadLine().ToLower().Trim();
            } while (!(answer2 == "min" || answer2 == "max" || answer2 == "avg" || answer2 == "count"));
        }

        protected void show()
        {
            string answer1 = "";

            //Do you wanna filter
            do
            {
                Console.WriteLine("\nDo you want to filter to Data Type (y/n)?");
                answer1 = Console.ReadLine().ToLower().Trim();
            } while (!(answer1 == "y" || answer1 == "n"));

            var filter = typeof(DataNode);
            var subTypes = typeof(DataNode).Assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DataNode))).ToList();

            int answer1b = 0;

            if (answer1 == "y")
            {
                int index = 1;
                Console.WriteLine("\nChoose which type you wanna see");
                foreach (var subType in subTypes) {
                    Console.WriteLine($"{index}\t{subType}");
                    index++;
                }
                do
                {
                    Console.WriteLine("\nEnter the index of the type you want.");
                    int.TryParse(Console.ReadLine().Trim(), out answer1b);
                } while (!(answer1b >= 1 && answer1b <= subTypes.Count));
            }

            //Setting up minimum filter
            string answer2 = "";

            do
            {
                Console.WriteLine("\nDo you want to have a minimum value filter (y/n)?");
                answer2 = Console.ReadLine().ToLower().Trim();
            } while (!(answer2 == "y" || answer2 == "n"));

            double minFilter = double.MinValue;

            if (answer2 == "y")
            {
                do
                {
                    Console.WriteLine("\nEnter the minimum value for filtering.");
                    double.TryParse(Console.ReadLine().Trim(), out minFilter);
                } while (!(minFilter > double.MinValue));
            }

            //Setting up maximum filter
            string answer3 = "";

            do
            {
                Console.WriteLine("\nDo you want to have a maximum value filter (y/n)?");
                answer3 = Console.ReadLine().ToLower().Trim();
            } while (!(answer3 == "y" || answer3 == "n"));

            double maxFilter = double.MaxValue;

            if (answer3 == "y")
            {
                do
                {
                    Console.WriteLine("\nEnter the maximum value for filtering.");
                    double.TryParse(Console.ReadLine().Trim(), out maxFilter);
                } while (!(maxFilter < double.MaxValue));
            }

            //Setting up minimum date filter
            string answer4 = "";

            do
            {
                Console.WriteLine("\nDo you want to have a minimum Date filter (y/n)?");
                answer4 = Console.ReadLine().ToLower().Trim();
            } while (!(answer4 == "y" || answer4 == "n"));

            DateTime minDateFilter = DateTime.MinValue;

            if (answer4 == "y")
            {
                do
                {
                    Console.WriteLine("\nEnter the minimum Date for filtering.");
                    DateTime.TryParse(Console.ReadLine().Trim(), out minDateFilter);
                } while (!(minDateFilter > DateTime.MinValue));
            }

            //Setting up maximum date filter
            string answer5 = "";

            do
            {
                Console.WriteLine("\nDo you want to have a maximum Date filter (y/n)?");
                answer5 = Console.ReadLine().ToLower().Trim();
            } while (!(answer5 == "y" || answer5 == "n"));

            DateTime maxDateFilter = DateTime.MaxValue;

            if (answer5 == "y")
            {
                do
                {
                    Console.WriteLine("\nEnter the maximum Date for filtering.");
                    DateTime.TryParse(Console.ReadLine().Trim(), out maxDateFilter);
                } while (!(maxDateFilter < DateTime.MaxValue));
            }

            //Writing out the data
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            Dictionary<Type, LinkedList<DataNode>> dicToWrite;
            if (answer1 == "n") {
                dicToWrite = mySystem.DataNodesByType;
            }
            else
            {
                dicToWrite = new Dictionary<Type, LinkedList<DataNode>>();
                dicToWrite.Add(subTypes[answer1b-1], mySystem.DataNodesByType[subTypes[answer1b - 1]]);
            }

            foreach (var nodeList in dicToWrite)
            {
                int count = 1;
                Console.WriteLine($"\n{nodeList.Key}");
                foreach (var node in nodeList.Value)
                {
                    if (count >= (height - 3))
                    {
                        Console.WriteLine("================================================");
                        Console.ReadLine();
                        count = 0;
                    }
                    count++;
                    string unit = BaseUnits[node.GetType()];
                    double value = node.GetIn(unit);
                    if (minDateFilter <= node.Date && maxDateFilter >= node.Date && minFilter <= value && maxFilter >= value)
                        Console.WriteLine($"{node.Date}\t{value} {unit}\t{node.Source}\t{node.Sensor}");
                }

            }
        }

        #region FillingMySystem
        protected void loadData()
        {
            List<int> viableAnsers = new List<int>() {1,2};
            int answer = -1;

            do
            {
                Console.WriteLine("\nSellect the loading method.\n\tWrite 1 for Importing\n\tWrite 2 for Generating");
                int.TryParse(Console.ReadLine().Trim(), out answer);
            } while (!viableAnsers.Contains(answer));

            switch (answer)
            {
                case 1:
                    importData();
                    break;
                case 2:
                    generateData();
                    break;
            }

        }

        private void importData()
        {
            string path = "";

            do
            {
                Console.WriteLine("\nEnter a valid path to import the file");
                path = Console.ReadLine().Trim();
            } while (!File.Exists(path));
            mySystem.ImportFromFile(path);
        }

        private void generateData()
        {
            string unit = null;
            double min = double.MaxValue;
            double max = double.MinValue;
            int startOutCount = mySystem.count;

            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            do
            {
                Console.WriteLine("\nEnter the the unit to generate values in (This is gonna determin the type of date, like pressure or humidity):");
                unit = Console.ReadLine().Trim();
            } while (unit != null);

            do
            {
                Console.WriteLine("\nEnter the minimum Value to generate");
                double.TryParse(Console.ReadLine().Trim(), out min);
            } while (min < double.MaxValue);

            do
            {
                Console.WriteLine("\nEnter the maximum Value to generate");
                double.TryParse(Console.ReadLine().Trim(), out max);
            } while (max > double.MinValue);

            do
            {
                Console.WriteLine("\nEnter the starting date to generate");
                DateTime.TryParse(Console.ReadLine().Trim(), out start);
            } while (start < DateTime.MaxValue);

            do
            {
                Console.WriteLine("\nEnter the end date to generate");
                DateTime.TryParse(Console.ReadLine().Trim(), out end);
            } while (end > DateTime.MinValue);

            mySystem.Generate(min, max, start, end, unit);
            Console.WriteLine($"\n\tThere were {mySystem.count - startOutCount} nodes generated\n");
        }
        #endregion

        private void exportData()
        {
            string path = "";

            do
            {
                Console.WriteLine("\nEnter a valid path to Export the file");
                path = Console.ReadLine().Trim();
            } while (!File.Exists(path));
            mySystem.ExportToFile(path);
        }
    }
}