using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MaintainerController : ControllerBase
    {
        private readonly MaintainerService _maintainerService;
        private readonly UserRepository _userService;

        public MaintainerController(MaintainerService maintainerService, UserRepository userRepository)
        {
            _maintainerService = maintainerService;
            _userService = userRepository;
        }

        [HttpGet]
        public async Task<List<Maintainer>> Get() =>
            await _maintainerService.GetAsync();

        [HttpGet("searchEntity")]
        public async Task<ActionResult<ApiResponse<SearchItem>>> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm?.Trim()) || searchTerm.Trim().Length < 2)
                return BadRequest(new ApiResponse<SearchItem> { Success = false, Data = new List<SearchItem>(), Message = "Search term must be at least 2 characters" });

            var maintainers = await _maintainerService.SearchMaintainersAsync(searchTerm.Trim());

            var response = new ApiResponse<SearchItem>();
            if (maintainers == null || maintainers.Count == 0)
            {
                response.Success = false;
                response.Message = "No data found";
                response.Data = new List<SearchItem>();
                return Ok(response);
            }

            response.Success = true;
            response.Message = "Search successful";
            response.Data = maintainers.Select(m => new SearchItem { Id = m.MaintainerId, Name = m.MaintainerName }).ToList();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Maintainer>> Get(string id)
        {
            var existingMaintainer = await _maintainerService.GetAsync(id);

            if (existingMaintainer is null)
            {
                return NotFound();
            }

            return existingMaintainer;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateMaintainerDto newMaintainerDto)
        {
            var newMaintainer = new Maintainer
            {
                MaintainerId = newMaintainerDto.MaintainerId,
                MaintainerName = newMaintainerDto.MaintainerName,
                ContactNumber = newMaintainerDto.ContactNumber,
                AlternateContactNumber = newMaintainerDto.AlternateContactNumber,
                Address = newMaintainerDto.Address,
                Education = newMaintainerDto.Education,
                TrainingCertificateUrl = newMaintainerDto.TrainingCertificateUrl,
                IdentityProofDocument = newMaintainerDto.IdentityProofDocument,
                IdentityProofNumber = newMaintainerDto.IdentityProofNumber,
                FarmOwnerId = newMaintainerDto.FarmOwnerId,
                //FarmsMaintained = newMaintainerDto.FarmsMaintained,
                //Role = newMaintainerDto.Role,
                SystemStatus = "Active"
            };

            var existingMaintainer = await _maintainerService.GetAsync(newMaintainer.MaintainerId);
            if(existingMaintainer != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A Maintainer with MaintainerId '{newMaintainer.MaintainerId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            var existingMaintainerWithMobile = await _maintainerService.GetAsyncByMobile(newMaintainer.ContactNumber);
            var existingUserWithMobile = await _userService.GetByMobileAsync(newMaintainer.ContactNumber);
            if (existingMaintainerWithMobile != null || existingUserWithMobile != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A Maintainer with the same contact number already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            await _maintainerService.CreateAsync(newMaintainer);

            //create user for the owner
            await _userService.CreateUserWithPasswordAsync(newMaintainer.MaintainerName, newMaintainer.ContactNumber
                , string.Empty, new List<string> { UserRoles.FARMHELP.ToString() });

            return CreatedAtAction(nameof(Get), new { id = newMaintainer.Id }, newMaintainer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateMaintainerDto updatedMaintainerDto)
        {
            var existingMaintainer = await _maintainerService.GetAsync(id);
            if (existingMaintainer == null) return NotFound();

            existingMaintainer.MaintainerName = updatedMaintainerDto.MaintainerName;
            existingMaintainer.Address = updatedMaintainerDto.Address;
            existingMaintainer.ContactNumber = updatedMaintainerDto.ContactNumber;
            existingMaintainer.AlternateContactNumber = updatedMaintainerDto.AlternateContactNumber;
            existingMaintainer.IdentityProofDocument = updatedMaintainerDto.IdentityProofDocument;
            existingMaintainer.IdentityProofNumber = updatedMaintainerDto.IdentityProofNumber;
            //existingMaintainer.FarmsMaintained = updatedMaintainerDto.FarmsMaintained;
            //existingMaintainer.Role = updatedMaintainerDto.Role;
            existingMaintainer.SystemStatus = updatedMaintainerDto.SystemStatus;

            //TODO : Handle this as update and no replace
            await _maintainerService.UpdateAsync(id, existingMaintainer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingMaintainer = await _maintainerService.GetAsync(id);

            if (existingMaintainer is null)
            {
                return NotFound();
            }

            await _maintainerService.RemoveAsync(id);

            return NoContent();
        }
    }
}