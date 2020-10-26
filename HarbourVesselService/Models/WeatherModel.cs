using System;

namespace HarbourVesselService.Models
{
    internal class WeatherModel
    {
        public Wind wind { get; set; }
        public DateTime LastCheck { get; set; }
    }

    internal class Wind
    {
        public double speed { get; set; }
        public double deg { get; set; }
    }
}