using HarbourControlAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.OData;

namespace HarbourControlAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VesselsController : ControllerBase
    {
        private readonly VesselContext _context;

        public VesselsController(VesselContext context)
        {
            _context = context;
        }

        // GET: api/Vessels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VesselModel>>> GetVessels()
        {
            return await _context.Vessels.ToListAsync();
        }

        // GET: api/Vessels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VesselModel>> GetVessel(Guid id)
        {
            var vessel = await _context.Vessels.FindAsync(id);

            if (vessel == null)
            {
                return NotFound();
            }

            return vessel;
        }

        // PUT: api/Vessels/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVessel(Guid id, VesselModel vessel)
        {
            if (id != vessel.Id)
            {
                return BadRequest();
            }

            var v = await _context.Vessels.FindAsync(id);

            v.Lat = vessel.Lat;
            v.Long = vessel.Long;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VesselExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Vessels
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<VesselModel>> PostVessel(VesselModel vessel)
        {
            if (_context.Vessels.Any(e => e.Name == vessel.Name && e.Lat == vessel.Lat && e.Long == vessel.Long))
                return BadRequest();

            _context.Vessels.Add(vessel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVessel), new { id = vessel.Id }, vessel);
        }

        // DELETE: api/Vessels/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<VesselModel>> DeleteVessel(Guid id)
        {
            var vessel = await _context.Vessels.FindAsync(id);
            if (vessel == null)
            {
                return NotFound();
            }

            _context.Vessels.Remove(vessel);
            await _context.SaveChangesAsync();

            return vessel;
        }

        private bool VesselExists(Guid id)
        {
            return _context.Vessels.Any(e => e.Id == id);
        }

        [HttpGet("ActiveVessels")]
        public async Task<ActionResult<IEnumerable<VesselModel>>> GetActiveVessels()
        {
            return await _context.Vessels.Where(v => v.Lat > 300 && v.Long > 300).ToListAsync();
        }

        [EnableQuery]
        [HttpGet("NextDockVessel")]
        public ActionResult<VesselModel> NextDockVessel([FromQuery] VesselFilterModel filter)
        {
            var vessels = _context.Vessels.Where(v => v.Lat > 300 && v.Long > 300).AsQueryable();

            if (filter.Type != null)
            {
                if (filter.isInclusive)
                    vessels = vessels.Where(v => v.Type.Equals(filter.Type));
                else
                    vessels = vessels.Where(v => !v.Type.Equals(filter.Type));
            }

            return vessels.FirstOrDefault();
        }
    }
}