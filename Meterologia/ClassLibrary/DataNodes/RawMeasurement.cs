using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DataNodes
{
    public class RawMeasurement()
    {//I wanted a stuct at first, but other lines needed it to be a class
        public DateTime timestamp { get; set; }
        public double value { get; set; }
        public string unit { get; set; } = string.Empty;
        public string source { get; set; } = string.Empty;
        public string? sensor { get; set; }
    }
}
