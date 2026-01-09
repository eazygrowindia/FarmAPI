using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json.Serialization;

namespace FarmAPI.Controllers
{
    [Authorize]
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

        [HttpGet("GetByPincode/{pincode}")]
        public async Task<ActionResult<PincodeResponse>> GetByPincode(string pincode)
        {
            var result = await _locationService.GetByPincodeAsync(pincode);
            if (result.Districts.Count == 0)
                return NotFound($"No locations found for pincode {pincode}");

            return Ok(result);
        }

        [HttpGet("GetStateByPincode/{pincode}")]
        public async Task<ActionResult<PincodeResponse>> GetStateByPincode(string pincode)
        {
            var result = await _locationService.GetStateByPincodeAsync(pincode);
            if (result == null)
                return NotFound($"No State found for pincode {pincode}");

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

        [HttpGet("GetAllStates")]
        public async Task<ActionResult<PincodeResponse>> GetAllStates()
        {
            var result = await _locationService.GetStatesAsync();
            if (result == null || result.Count <= 0)
                return NotFound($"No States found");

            return Ok(result);
        }

        [HttpGet("GetDistrictsByState/{state}")]
        public async Task<ActionResult<List<DistrictInfo>>> GetDistrictsByState(string state)
        {
            var result = await _locationService.GetDistrictsByStateAsync(state);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("GetSubdistrictsByDistrict/{districtCode}")]
        public async Task<ActionResult<List<SubdistrictResponse>>> GetSubdistrictsByDistrict(int districtCode)
        {
            var result = await _locationService.GetSubdistrictsByDistrictAsync(districtCode);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("GetVillagesBySubDistrict/{subDistrictName}")]
        public async Task<ActionResult<List<VillageInfo>>> GetVillagesBySubDistrict(string subDistrictName)
        {
            var result = await _locationService.GetVillagesBySubDistrictAsync(subDistrictName);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        #region Start Karnataka Specific APIs

        [HttpGet("GetKarnatakaDistricts")]
        public async Task<ActionResult<List<DistrictInfo>>> GetKarnatakaDistricts()
        {
            var result = await _karnatakaLocationService.GetDistrictsAsync();
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("GetKarnatakaTalukasByDistrict/{districtCode}")]
        public async Task<ActionResult<List<SubdistrictInfo>>> GetKarnatakaTalukasByDistrict(int districtCode)
        {
            var result = await _karnatakaLocationService.GetTalukasByDistrictAsync(districtCode);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("GeKarnatakatHoblisByDistrictAndTaluka")]
        public async Task<ActionResult<List<HobliResponse>>> GetHoblisByDistrictAndTaluka([FromQuery] int districtCode, [FromQuery] int talukaCode)
        {
            var result = await _karnatakaLocationService.GetHoblisByDistrictAndTalukaAsync(districtCode, talukaCode);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("GetKarnatakaVillagesByDistrictAndTalukaAndHobli")]
        public async Task<ActionResult<List<VillageInfo>>> GetVillagesByDistrictAndTalukaAndHobli([FromQuery] int districtCode, [FromQuery] int talukaCode,[FromQuery] int hobliCode)
        {
            var result = await _karnatakaLocationService.GetVillagesByDistrictAndTalukaAndHobliAsync(districtCode, talukaCode, hobliCode);
            if (result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        #endregion End Karnataka Specific APIs
    }
}
