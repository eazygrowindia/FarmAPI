using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly LgdLocationService _locationService;
        private readonly KarnatakaLocationService _karnatakaLocationService;

        public AddressController(LgdLocationService locationService, KarnatakaLocationService karnatakaLocationService)
        {
            _locationService = locationService;
            _karnatakaLocationService = karnatakaLocationService;
        }

        [HttpGet("by-pincode/{pincode}")]
        public async Task<ActionResult<PincodeResponse>> GetByPincode(string pincode)
        {
            var result = await _locationService.GetByPincodeAsync(pincode);
            if (result.Districts.Count == 0)
                return NotFound($"No locations found for pincode {pincode}");

            return Ok(result);
        }

        [HttpGet("districts")]
        public async Task<ActionResult<List<DistrictInfo>>> GetDistricts(string pincode)
        {
            var result = await _locationService.GetDistrictsAsync(pincode);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("subdistricts")]
        public async Task<ActionResult<List<SubdistrictInfo>>> GetSubdistricts(string pincode, int districtCode)
        {
            var result = await _locationService.GetSubdistrictsAsync(pincode, districtCode);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("villages")]
        public async Task<ActionResult<List<VillageInfo>>> GetVillages(string pincode, int districtCode, int subdistrictCode)
        {
            var result = await _locationService.GetVillagesAsync(pincode, districtCode, subdistrictCode);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        //[HttpGet("karnataka/hoblis/{district}/{taluka}")]
        //public async Task<List<HobliItem>> GetHoblis(int district, int taluka)
        //{
        //    var filter = Builders<KarnatakaLocation>.Filter.Eq(x => x.DistrictCode, district) & Builders<KarnatakaLocation>.Filter.Eq(x => x.TalukaCode, taluka);
        //    return await _karnatakaLocationService.Distinct(x => new { x.HobliCode, x.HobliName }, filter).ToListAsync();
        //}
    }
}
