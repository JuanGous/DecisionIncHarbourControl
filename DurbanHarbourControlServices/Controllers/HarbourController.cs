using DurbanHarbourControlServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DurbanHarbourControlServices.Controllers
{
    public class HarbourController : Controller
    {
        private readonly ILogger<HarbourController> _logger;

        public HarbourController(ILogger<HarbourController> logger)
        {
            _logger = logger;
        }

        public IActionResult Harbour()
        {
            IEnumerable<VesselModel> vessels = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44390/api/Vessels/"); //TODO: Change URL and Port
                //HTTP GET
                var responseTask = client.GetAsync("ActiveVessels");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    vessels = JsonConvert.DeserializeObject<IEnumerable<VesselModel>>(readTask.Result);
                }
                else //web api sent error response
                {
                    vessels = new List<VesselModel>();

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }

            VesselListModel vl = new VesselListModel();
            vl.list = vessels.ToList();

            return View(vl);
        }
    }
}