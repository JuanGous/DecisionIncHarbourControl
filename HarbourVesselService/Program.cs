using HarbourVesselService.Models;
using Newtonsoft.Json;
using SharedLib.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace HarbourVesselService
{
    internal class Program
    {
        private const string weatherAPIKey = "";
        private static bool dockingInProgress = false;
        private static WeatherModel wm = null;

        public static void Main(string[] args)
        {
            int interval = 1;

            Console.WriteLine("Please enter the interval in minutes for when new vessels should be generated:");
            var inp = Console.ReadLine();

            if (!int.TryParse(inp, out interval))
            {
                Console.WriteLine("Invalid value!");
                Environment.Exit(0);
            }

            Timer tmv = new Timer(GenerateVessel, null, 0, interval * 60000); //Longest vessel dock time 120min Cargo
            Timer tmp = new Timer(DockVessel, null, 61000, 5 * 60000); //5min

            Console.WriteLine("Press any key to close");
            Console.ReadLine();
        }

        private static void GenerateVessel(Object o)
        {
            Random r = new Random(DateTime.Now.Second);
            var i = r.Next(0, 3);

            VesselModel v = new VesselModel();
            Point p = RandomPointGenerator();

            v.Name = RandomString(8);
            v.Lat = p.X;
            v.Long = p.Y;
            v.Type = (VesselType)i;
            v.CreateDateTime = DateTime.Now;

            var httpContent = new StringContent(JsonConvert.SerializeObject(v), Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44390/api/"); //TODO: Change URL and Port

                //HTTP POST
                var postTask = client.PostAsync("Vessels", httpContent);
                postTask.Wait();

                var result = postTask.Result;
                if (!result.IsSuccessStatusCode)
                {
                    Console.WriteLine("Server Error.");
                }
            }
        }

        public static string RandomString(int length)
        {
            Random random = new Random(DateTime.Now.Second);

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static Point RandomPointGenerator()
        {
            Random random = new Random(DateTime.Now.Second);

            int r = random.Next(160, 200);
            int a = random.Next(180, 360);

            double x = Math.Abs(r * Math.Cos(a)) + 300;
            double y = Math.Abs(r * Math.Sin(a)) + 300;

            return new Point((int)x, (int)y);
        }

        private static VesselModel GetDockVessel()
        {
            VesselModel vessel = null;
            var wind = GetWindSpeed();
            var url = "NextDockVessel";

            if (wind != null)
            {
                var filter = "";
                if (wind < 10 || wind > 30)
                {
                    filter = string.Format("?{0}", "Type=Sail&IsInclusive=false");
                }
                url = "NextDockVessel" + filter;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44390/api/Vessels/"); //TODO: Change URL and Port
                //HTTP GET
                var responseTask = client.GetAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    vessel = JsonConvert.DeserializeObject<VesselModel>(readTask.Result);
                }
                else //web api sent error response
                {
                    Console.WriteLine("Server error.");
                    return vessel;
                }
            }

            return vessel;
        }

        public static void DockVessel(Object o)
        {
            if (dockingInProgress)
                return; //Exit if vessel is busy docking

            dockingInProgress = true;

            var kmMultiplier = 16; //simulated 16px = 1km
            var speed = 60000; //simulated speed

            //Get next vessel to dock
            var vessel = GetDockVessel();

            if (vessel != null)
            {
                switch (vessel.Type)
                {
                    case VesselType.Speed:
                        speed = speed / 30; //30km/h
                        break;

                    case VesselType.Sail:
                        speed = speed / 15; //15km/h
                        break;

                    case VesselType.Cargo:
                        speed = speed / 5; //5km/h
                        break;

                    default:
                        break;
                }

                while (vessel.Lat > 300 && vessel.Long > 300)
                {
                    vessel = MoveDockingVessel(vessel, kmMultiplier);
                    Thread.Sleep(speed);
                }
            }

            dockingInProgress = false;
        }

        public static VesselModel MoveDockingVessel(VesselModel vessel, int movementDistance)
        {
            int x = vessel.Lat;
            int y = vessel.Long;

            //Origin is (300:300)
            if (x > 300)
                x = x - movementDistance;
            else if (x < 300)
                x = x + movementDistance;

            if (y > 300)
                y = y - movementDistance;
            else if (y < 300)
                y = y + movementDistance;

            vessel.Lat = x;
            vessel.Long = y;

            //Update API
            var httpContent = new StringContent(JsonConvert.SerializeObject(vessel), Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44390/api/"); //TODO: Change URL and Port

                //HTTP PUT
                var putTask = client.PutAsync(String.Format("Vessels/{0}", vessel.Id), httpContent);
                putTask.Wait();

                var result = putTask.Result;
                if (!result.IsSuccessStatusCode)
                {
                    Console.WriteLine("Server Error.");
                }
            }

            return vessel;
        }

        public static double? GetWindSpeed()
        {
            double? wind = null;

            if (wm == null || ((TimeSpan)DateTime.Now.Subtract(wm.LastCheck)).TotalMinutes > 60)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/"); //TODO: Change URL and Port
                                                                                             //HTTP GET
                    var responseTask = client.GetAsync(string.Format("weather?id={0}&appid={1}&units=metric", "1007311", weatherAPIKey)); //1007311 - Durban
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        wm = JsonConvert.DeserializeObject<WeatherModel>(readTask.Result);
                    }
                    else //web api sent error response
                    {
                        Console.WriteLine("Server error.");
                    }
                }
            }

            if (wm.wind.speed > 0)
                wind = (wm.wind.speed * 18) / 5;

            return wind;
        }
    }
}