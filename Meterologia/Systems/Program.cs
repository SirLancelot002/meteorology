using ClassLibrary;
using ClassLibrary.DataNodes;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UserHandler handler = new UserHandler();
            handler.run();
        }
    }
}