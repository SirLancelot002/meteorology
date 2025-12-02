using ClassLibrary;
using ClassLibrary.DataNodes;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MeasurementSystem testSystem = new MeasurementSystem();
            testSystem.Generate(1,50,DateTime.Now,DateTime.Now.AddYears(1),"m/s");
            testSystem.Generate(-10, 15, DateTime.Now, DateTime.Now.AddYears(1), "°C");
            testSystem.Generate(0, 99, DateTime.Now, DateTime.Now.AddYears(1), "%");
            testSystem.Generate(0.8, 1.2, DateTime.Now, DateTime.Now.AddYears(1), "atm");

            testSystem.ExportToFile("./Generated_Export_Giga_1.json");
            /*Console.WriteLine("");
            foreach (var nodeList in testSystem.DataNodesByType)
            {
                Console.WriteLine(nodeList.Key);
                foreach (var node in nodeList.Value) {
                    Console.WriteLine(node);
                }
            }*/
        }
    }
}