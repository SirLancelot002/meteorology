using ClassLibrary;
using ClassLibrary.DataNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Program
{
    public class User
    {
        public MeasurementSystem mySystem;
        protected List<Action> Commands = new List<Action>();

        public User(MeasurementSystem mySystem)
        {
            this.mySystem = mySystem;
            Commands.Add(loadData);
            Commands.Add(exportData);
        }

        public bool choose()
        {
            return false;
        }

        public void show()
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;
            string answer1 = "";

            do
            {
                Console.WriteLine("\nDo you want to filter to Data Type (y/n)?");
                answer1 = Console.ReadLine().ToLower().Trim();
            } while (answer1 == "y" || answer1 == "n");

            var filterType = typeof(DataNode);
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
