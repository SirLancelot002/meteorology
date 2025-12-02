using ClassLibrary;
using ClassLibrary.DataNodes;
using System.IO;

namespace Tester
{
    public class DataNodeTests
    {
        //=================Tester is working=================
        [Fact(DisplayName = "01. Tester is working")]
        public void Test1()
        {
            Assert.True(true);
        }

        #region Pressure
        //=================Testing Pressure=================
        [Fact(DisplayName = "02. Creating Pressure Node")]
        public void Test2()
        {
            Pressure testPressure = new Pressure(DateTime.Now, 100, SourceType.GENERATED, "Geza");
            Assert.True(testPressure.Sensor == "Geza", "Name miss-match");
        }

        [Fact(DisplayName = "03. Converting from base hPa to other units")]
        public void Test3()
        {
            Pressure p = new Pressure(DateTime.Now, 1000.0, SourceType.GENERATED, "Bela");

            // From base (hPa) -> others
            Assert.Equal(1000.0, p.GetIn("hPa"), 5);
            Assert.Equal(100000.0, p.GetIn("pa"), 5);
            Assert.Equal(100.0, p.GetIn("kPa"), 5);
            Assert.Equal(1.0, p.GetIn("bAR"), 5);

            double atm = p.GetIn("atm");
            double torr = p.GetIn("tor");
            double psi = p.GetIn("psi");

            Assert.InRange(atm, 0.9869, 0.9870);     // 1000 / 1013.25
            Assert.InRange(torr, 750.06, 750.07);    // 1000 / 1.33322
            Assert.InRange(psi, 14.50, 14.51);       // 1000 / 68.9476
        }

        [Fact(DisplayName = "04. Converting from custom units into base hPa")]
        public void Test4()
        {
            Pressure p = new Pressure(DateTime.Now, 0.0, SourceType.IMPORTED, "Laci");

            // Pa -> hPa
            p.SetFrom("Pa", 100000.0);
            Assert.Equal(1000.0, p.Value, 5);

            // kPa -> hPa
            p.SetFrom("kPa", 100.0);
            Assert.Equal(1000.0, p.Value, 5);

            // bar -> hPa
            p.SetFrom("bar", 1.0);
            Assert.Equal(1000.0, p.Value, 5);

            // atm -> hPa
            p.SetFrom("atm", 0.9869233);
            Assert.InRange(p.Value, 999.9, 1000.1);

            // torr -> hPa
            p.SetFrom("tor", 750.0638);
            Assert.InRange(p.Value, 999.9, 1000.1);

            // psi -> hPa
            p.SetFrom("psi", 14.5038);
            Assert.InRange(p.Value, 999.9, 1000.1);
        }
        #endregion

        #region Temprature
        //=================Testing Temperature=================
        [Fact(DisplayName = "05. Creating Temperature Node")]
        public void Test5()
        {
            TemperatureNode t = new TemperatureNode(DateTime.Now, 300.0, SourceType.GENERATED, "David");
            Assert.True(t.Sensor == "David", "Name miss-match");
        }

        [Fact(DisplayName = "06. Converting from base Kelvin to other units")]
        public void Test6()
        {
            // base value in Kelvin
            TemperatureNode t = new TemperatureNode(DateTime.Now, 300.0, SourceType.GENERATED, "Martin");

            Assert.Equal(300.0, t.GetIn("K"), 5);

            double c = t.GetIn("°C");
            double f = t.GetIn("°F");

            // 300 K = 26.85 °C ≈ 80.33 °F
            Assert.Equal(26.85, c, 2);
            Assert.Equal(80.33, f, 2);
        }

        [Fact(DisplayName = "07. Converting from custom units into base Kelvin")]
        public void Test7()
        {
            TemperatureNode t = new TemperatureNode(DateTime.Now, 0.0, SourceType.IMPORTED, "Tibi");

            // K -> K
            t.SetFrom("K", 300.0);
            Assert.Equal(300.0, t.Value, 5);

            // °C -> K
            t.SetFrom("°C", 0.0);
            Assert.Equal(273.15, t.Value, 2);

            t.SetFrom("°C", 100.0);
            Assert.Equal(373.15, t.Value, 2);

            // °F -> K
            t.SetFrom("°F", 32.0);
            Assert.Equal(273.15, t.Value, 2);

            t.SetFrom("°F", 212.0);
            Assert.Equal(373.15, t.Value, 2);
        }
        #endregion

        #region Humidity
        //=================Testing Humidity=================
        [Fact(DisplayName = "08. Creating Humidity Node")]
        public void Test8()
        {
            Humidity h = new Humidity(DateTime.Now, 55.0, SourceType.GENERATED, "Pisti");
            Assert.True(h.Sensor == "Pisti", "Name miss-match");
        }

        [Fact(DisplayName = "09. Converting from base % to other units")]
        public void Test9()
        {
            Humidity h = new Humidity(DateTime.Now, 75.0, SourceType.GENERATED, "Patrik");

            Assert.Equal(75.0, h.GetIn("%"), 5);

            Assert.Equal(0.75, h.GetIn("fraction"), 5);
        }

        [Fact(DisplayName = "10. Converting from custom units into base %")]
        public void Test10()
        {
            Humidity h = new Humidity(DateTime.Now, 0.0, SourceType.IMPORTED, "Gyuri");

            // fraction -> %
            h.SetFrom("fraction", 0.45);
            Assert.Equal(45.0, h.Value, 5);

            h.SetFrom("fraction", 0.99);
            Assert.Equal(99.0, h.Value, 5);

            // % -> %
            h.SetFrom("%", 33.0);
            Assert.Equal(33.0, h.Value, 5);
        }
        #endregion

        #region WindSpeed
        //=================Testing Humidity=================
        [Fact(DisplayName = "11. Creating WindSpeed Node")]
        public void Test11()
        {
            WindSpeed w = new WindSpeed(DateTime.Now, 10.0, SourceType.GENERATED, "Feri");
            Assert.True(w.Sensor == "Feri", "Milyen Feri?");
        }

        [Fact(DisplayName = "12. Converting from base m/s to other units")]
        public void Test12()
        {
            WindSpeed w = new WindSpeed(DateTime.Now, 10.0, SourceType.GENERATED, "Cecil");

            // From base (m/s) -> others
            Assert.Equal(10.0, w.GetIn("m/s"), 5);
            Assert.Equal(36.0, w.GetIn("km/h"), 5);  // 10 * 3.6

            double mph = w.GetIn("mph");   // 1 mph = 0.44704 m/s
            double knots = w.GetIn("knot");   // 1 kt  = 0.514444 m/s
            double ftps = w.GetIn("ft/s");  // 1 ft/s = 0.3048 m/s

            Assert.InRange(mph, 22.36, 22.38);      // ≈ 22.369
            Assert.InRange(knots, 19.43, 19.45);    // ≈ 19.438
            Assert.InRange(ftps, 32.80, 32.82);     // ≈ 32.808
        }

        [Fact(DisplayName = "13. Converting from custom units into base m/s")]
        public void Test13()
        {
            WindSpeed w = new WindSpeed(DateTime.Now, 0.0, SourceType.IMPORTED, "Marci");

            // km/h -> m/s
            w.SetFrom("km/h", 36.0);
            Assert.InRange(w.Value, 9.999, 10.001);   // should be exactly 10

            // mph -> m/s  (1 mph = 0.44704 m/s)
            w.SetFrom("mph", 22.36936);
            Assert.InRange(w.Value, 9.999, 10.001);

            // knots -> m/s (1 kt = 0.514444 m/s)
            w.SetFrom("knot", 19.43846);
            Assert.InRange(w.Value, 9.999, 10.001);

            // ft/s -> m/s (1 ft/s = 0.3048 m/s)
            w.SetFrom("ft/s", 32.8084);
            Assert.InRange(w.Value, 9.999, 10.001);
        }
        #endregion

        [Fact(DisplayName = "14. Unit checking")]
        public void Test14()
        {
            Assert.True(DataNode.IsUnitSupportedForType(typeof(TemperatureNode), "°C"), "For some reason Temperature didn't recognize Celsius.");
            Assert.True(DataNode.IsUnitSupportedForType(typeof(Humidity), "%"), "For some reason Humidity didn't recognize %.");
            Assert.True(DataNode.IsUnitSupportedForType(typeof(Pressure), "bar"), "For some reason Pressure didn't recognize bar.");
            Assert.True(DataNode.IsUnitSupportedForType(typeof(WindSpeed), "mph"), "For some reason Windspeed didn't recognize mph.");

            Assert.False(DataNode.IsUnitSupportedForType(typeof(TemperatureNode), "Diddy"), "For some reason Temperature recognize Diddy as a valid unit.");
            Assert.False(DataNode.IsUnitSupportedForType(typeof(Humidity), "Epstein"), "For some reason Humidity recognize Epstein as a valid unit.");
            Assert.False(DataNode.IsUnitSupportedForType(typeof(Pressure), "EDP"), "For some reason Pressure recognize EDP as a valid unit.");
            Assert.False(DataNode.IsUnitSupportedForType(typeof(WindSpeed), "Mizkif"), "For some reason WindSpeed recognize Mizkif as a valid unit.");

            var subTypes = typeof(DataNode).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DataNode))).ToList();

            string found = "";
            foreach (var subType in subTypes)
            {
                if (DataNode.IsUnitSupportedForType(subType, "knot"))
                {
                    found = subType.FullName;
                }
            }

            Assert.True(found == "ClassLibrary.DataNodes.WindSpeed");
        }
    }

    public class MeasurementSystemTests
    {
        string projectDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        //I've tried using a less aggressive solution, but those did not suffice

        #region BasicFileReading
        [Fact(DisplayName = "01. Can Read")]
        public void Test1()
        {
            string filePath = Path.Combine(projectDir, "TestFiles", "sample_measurements_1.json");
            Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

            MeasurementSystem testSystem = new MeasurementSystem();
            testSystem.ImportFromFile(filePath);
            Assert.Equal(4, testSystem.DataNodesByType.Count);
        }

        [Fact(DisplayName = "02. Perfectly Formated File was read correctly")]
        public void Test2()
        {
            string filePath = Path.Combine(projectDir, "TestFiles", "sample_measurements_1.json");

            MeasurementSystem testSystem = new MeasurementSystem();
            testSystem.ImportFromFile(filePath);


            var minTemp = testSystem.DataNodesByType[typeof(TemperatureNode)]
                             .MinBy(t => t.Value);
            var maxHum = testSystem.DataNodesByType[typeof(Humidity)]
                             .MaxBy(t => t.Value);
            var sensorTest = testSystem.DataNodesByType[typeof(Pressure)].First(t => Math.Abs(t.GetIn("atm") - 1.01) < 1e-6);

            Assert.Equal(-2.5, minTemp.GetIn("°C"));
            Assert.Equal(82.0, maxHum.GetIn("%"));
            Assert.Equal("baro_sec", sensorTest.Sensor);
            Assert.Equal(3, testSystem.DataNodesByType[typeof(WindSpeed)].Count);
        }

        [Fact(DisplayName = "03. Perfectly Formated Bigger File was read correctly")]
        public void Test3()
        {
            string filePath = Path.Combine(projectDir, "TestFiles", "sample_measurements_2.json");

            MeasurementSystem testSystem = new MeasurementSystem();
            testSystem.ImportFromFile(filePath);

            bool success = true;
            foreach (var nodelist in testSystem.DataNodesByType) {
                success = success && nodelist.Value.Count == 5;
            }

            Assert.True(success, "");
        }

        [Fact(DisplayName = "04. Test on non-one-line-one-node File")]
        public void Test4()
        {
            string filePath = Path.Combine(projectDir, "TestFiles", "sample_measurements_3.json");

            MeasurementSystem testSystem = new MeasurementSystem();
            testSystem.ImportFromFile(filePath);

            bool success = true;
            foreach (var nodelist in testSystem.DataNodesByType)
            {
                success = success && nodelist.Value.Count == 5;
            }

            Assert.True(success, "Wrong amount of nodes were stored");
        }

        [Fact(DisplayName = "05. Bad file read correctly")]
        public void Test5()
        {
            string filePath = Path.Combine(projectDir, "TestFiles", "sample_measurements_4.json");

            MeasurementSystem testSystem = new MeasurementSystem();
            testSystem.ImportFromFile(filePath);

            int countOfNodes = 0;
            foreach (var nodeList in testSystem.DataNodesByType)
            {
                foreach (var node in nodeList.Value)
                {
                    countOfNodes++;
                }
            }
            Assert.Equal(1, countOfNodes);
        }
        #endregion
    }
}