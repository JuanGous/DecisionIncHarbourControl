using SharedLib.Models;

namespace HarbourControlAPI.Models
{
    public class VesselFilterModel
    {
        public VesselType? Type { get; set; } = null;
        public bool isInclusive { get; set; } = true;
    }
}