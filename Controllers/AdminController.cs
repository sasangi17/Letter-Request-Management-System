
using System.Security.Claims;
using Letter_Request_Management_System.Business;
using Letter_Request_Management_System.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Letter_Request_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminBusiness _adminBusiness;

        public AdminController(AdminBusiness adminBusiness)
        {
            _adminBusiness = adminBusiness;
        }

        // GET: /api/Admin/requests
        [HttpGet("requests")]
        public IActionResult GetAllRequests()
        {
            var requests = _adminBusiness.GetAllRequests();
            return Ok(requests);
        }
        
        /*
        // GET: /api/Admin/requests/3
        [HttpGet("requests/{id:int}")]
        public IActionResult GetRequestById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid letter request ID is required."
                });
            }

            var request = _adminBusiness.GetRequestById(id);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "Letter request not found."
                });
            }

            return Ok(request);
        }*/

        // PUT: /api/Admin/requests/3/approve
        [HttpPut("requestsapprove")]
        public IActionResult ApproveRequest(
            int id,
            [FromBody] ApproveLetterRequestDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid letter request ID is required."
                });
            }

            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.SignatureReference))
            {
                return BadRequest(new
                {
                    message = "A signature reference is required."
                });
            }

            string? userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int adminUserId))
            {
                return Unauthorized(new
                {
                    message = "The authenticated admin user ID is missing."
                });
            }

            int result = _adminBusiness.ApproveRequest(
                id,
                adminUserId,
                dto.SignatureReference);

            if (result == 0)
            {
                return NotFound(new
                {
                    message = "Letter request not found."
                });
            }

            if (result == 2)
            {
                return Conflict(new
                {
                    message = "This request is not pending and cannot be approved again."
                });
            }

            return Ok(new
            {
                message = "Letter request approved successfully.",
                letterRequestId = id,
                status = "Approved"
            });
        }
    }
}
