using ClassLibrary.DataNodes;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var subTypes = typeof(DataNode)
                .Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DataNode)))
                .ToList();

            foreach (var subType in subTypes)
            {
                Console.WriteLine(subType.FullName);
            }
        }
    }
}