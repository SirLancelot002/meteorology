using ClassLibrary;
using ClassLibrary.DataNodes;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
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
        public Dictionary<Type, string> BaseUnits = new Dictionary<Type, string>();

        private void InitCommands()
        {
            Commands.Add(LoadData);
            Commands.Add(ExportData);
            Commands.Add(ShowData);
            Commands.Add(Analyse);
        }//Helper function to be called in the Constructors.

        public User(MeasurementSystem mySystem)
        {
            this.mySystem = mySystem;
            InitCommands();

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
            InitCommands();
        }

        public User(User myAdmin)
        {
            this.mySystem = myAdmin.mySystem;
            InitCommands();

            BaseUnits = myAdmin.BaseUnits;
        }

        public int choose()
        {
            int choice = int.MinValue;

            do
            {
                Console.WriteLine("\nChoose an action (write it's index:)\n\t1\tExit\n\t2\tLog in/out Admin");
                int i = 3;
                foreach (var command in Commands)
                {
                    Console.WriteLine($"\t{i}\t{command.Method.Name}");
                    i++;
                }
                int.TryParse(Console.ReadLine().Trim(), out choice);
            } while (!(choice >= 1 && choice <= (Commands.Count+2)));

            if (choice == 1)
            {
                return 0;
            }
            else if (choice == 2) return 1;
            else {
                Commands[choice - 3]();
                return int.MinValue;
            }
        }

        protected void Analyse()
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

            Dictionary<string, Func<IEnumerable<double>, double>> analTypes = new()
            {
                ["min"] = seq => seq.Min(),
                ["max"] = seq => seq.Max(),
                ["avg"] = seq => seq.Average(),
                ["count"] = seq => seq.Count()
            };

            if (answer1 == "overall") {
                foreach (var Llist in mySystem.DataNodesByType){
                    Console.WriteLine($"\n{Llist.Key}");
                    var numbers = Llist.Value.Select(node => node.GetIn(BaseUnits[Llist.Key]));
                    var result = analTypes[answer2](numbers);
                    var ourUnit = answer2 != "count" ? BaseUnits[Llist.Key] : "";
                    Console.WriteLine($"{answer2}: {result.ToString("F2", CultureInfo.InvariantCulture)} {ourUnit}");
                }
            }
            else {
                foreach (var Llist in mySystem.DataNodesByType)
                {
                    int width = Console.WindowWidth;
                    int height = Console.WindowHeight; //This is inside the loop, bacause the user can change the window size while we are still using this loop
                    Console.WriteLine($"\n{Llist.Key}");

                    var groups = Llist.Value.GroupBy(node => node.Date.Date);
                    int cycle = 2;
                    var ourUnit = answer2 != "count" ? BaseUnits[Llist.Key] : "";

                    foreach (var g in groups)
                    {
                        if (cycle > height - 2) {
                            Console.WriteLine("================================================");
                            Console.ReadLine();
                            cycle = 0;
                        }
                        cycle++;
                        var numbers = g.Select(node => node.GetIn(BaseUnits[Llist.Key]));
                        var result = analTypes[answer2](numbers);
                        Console.WriteLine($"{g.Key:yyyy-MM-dd} {answer2}: {result.ToString("F2", CultureInfo.InvariantCulture)} {ourUnit}");
                    }
                    Console.ReadLine();
                }
            }
        }

        protected void ShowData()
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
                    Console.WriteLine($"{index}\t{subType.Name}");
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
                    string unit = BaseUnits[node.GetType()];
                    double value = node.GetIn(unit);
                    if (minDateFilter <= node.Date && maxDateFilter >= node.Date && minFilter <= value && maxFilter >= value)
                    {
                        Console.WriteLine($"{node.Date}\t{value.ToString("F2", CultureInfo.InvariantCulture)} {unit}\t{node.Source}\t{node.Sensor}");
                        count++;
                    }
                }
                Console.ReadLine();

            }
        }

        #region FillingMySystem
        public void LoadData()
        {
            do
            {
                List<int> viableAnsers = new List<int>() { 1, 2 };
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
                if (mySystem.count < 1) Console.WriteLine("No Data was loaded into the system");
            }
            while (mySystem.count < 1);
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
            } while (unit == null);

            do
            {
                Console.WriteLine("\nEnter the minimum Value to generate");
                double.TryParse(Console.ReadLine().Trim(), out min);
            } while (!(min < double.MaxValue));

            do
            {
                Console.WriteLine("\nEnter the maximum Value to generate");
                double.TryParse(Console.ReadLine().Trim(), out max);
            } while (!(max > double.MinValue));

            do
            {
                Console.WriteLine("\nEnter the starting date to generate");
                DateTime.TryParse(Console.ReadLine().Trim(), out start);
            } while (!(start < DateTime.MaxValue));

            do
            {
                Console.WriteLine("\nEnter the end date to generate");
                DateTime.TryParse(Console.ReadLine().Trim(), out end);
            } while (!(end > DateTime.MinValue));

            mySystem.Generate(min, max, start, end, unit);
            Console.WriteLine($"\n\tThere were {mySystem.count - startOutCount} nodes generated\n");
        }
        #endregion

        private void ExportData()
        {
            string path = "";

            do
            {
                Console.WriteLine("\nEnter a valid path to Export the file");
                path = Console.ReadLine().Trim();
            } while (path == "");
            mySystem.ExportToFile(path);
        }
    }
}