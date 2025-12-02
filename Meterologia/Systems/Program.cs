using ClassLibrary;
using ClassLibrary.DataNodes;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MeasurementSystem testSystem = new MeasurementSystem();
            testSystem.ImportFromFile("G:\\Dani_(idéglenes)\\Suli\\BME\\3. Félév\\SofTech\\Hazi 2\\Meterologia\\Tester\\TestFiles/sample_measurements_4.json");

            Console.WriteLine("");
            foreach (var nodeList in testSystem.DataNodesByType)
            {
                Console.WriteLine(nodeList.Key);
                foreach (var node in nodeList.Value) {
                    Console.WriteLine(node);
                }
            }
        }
    }
}