using Microsoft.EntityFrameworkCore;
using SharedLib.Models;

namespace HarbourControlAPI.Models
{
    public class VesselContext : DbContext
    {
        public VesselContext(DbContextOptions<VesselContext> options) : base(options)
        {
        }

        public DbSet<VesselModel> Vessels { get; set; }
    }
}