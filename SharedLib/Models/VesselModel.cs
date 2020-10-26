using System;

namespace SharedLib.Models
{
    public class VesselModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Lat { get; set; }
        public int Long { get; set; }
        public VesselType Type { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public enum VesselType
    {
        Sail = 0,
        Speed = 1,
        Cargo = 2
    }
}