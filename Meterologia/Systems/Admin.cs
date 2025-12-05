using ClassLibrary.DataNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Program
{
    public class Admin : User
    {
        public Admin(User user) : base(user.mySystem, user.BaseUnits) {
            Commands.Add(ChangeBaseUnit);
            Commands.Add(ClearData);
        }

        private void ChangeBaseUnit()
        {
            int choice = 0;
            var bUnits = BaseUnits.Keys.ToList();
            do
            {
                Console.WriteLine("\nFor which type of data would you like to change the base unit (answer the index)?");
                int i = 1;
                foreach (var baseUnit in bUnits)
                {
                    Console.WriteLine($"\t{i}\t{baseUnit.Name}");
                    i++;
                }
                int.TryParse(Console.ReadLine().Trim(), out choice);
            } while (!(choice >= 1 && choice <= bUnits.Count));

            var field = bUnits[choice-1].GetField("_converters", BindingFlags.NonPublic | BindingFlags.Static);

            var converters = field.GetValue(null) as System.Collections.Generic.IReadOnlyDictionary<string, (Func<double, double>, Func<double, double>)>;
            var units = converters.Keys.ToList();
            int toChange = -1;

            do
            {
                Console.WriteLine("\nTo which unit would you like to change the base unit (answer the index)?");
                int i = 1;
                foreach (var unit in units)
                {
                    Console.WriteLine($"\t{i}\t{unit}");
                    i++;
                }
                int.TryParse(Console.ReadLine().Trim(), out toChange);
            } while (!(toChange >= 1 && toChange <= units.Count));

            BaseUnits[bUnits[choice - 1]] = units[toChange - 1];
        }

        private void ClearData()
        {
            string answer = "";
            do
            {
                Console.WriteLine("\nAre you sure you want to delete all data (y/n)?");
                answer = Console.ReadLine().ToLower().Trim();
            } while (!(answer == "y" || answer == "n"));
            if (answer == "y")
            {
                mySystem.Clear();
                Console.WriteLine($"System was cleared, new node count: {mySystem.count}");
            }
        }
    }
}
