using System.Security.Claims;
using Letter_Request_Management_System.Business;
using Letter_Request_Management_System.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Letter_Request_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee")]
    public class LetterRequestController : ControllerBase
    {
        private readonly LetterRequestBusiness _business;
        

        public LetterRequestController(
            LetterRequestBusiness business)
        {
            _business = business;
            
        }

        private int GetLoggedInUserId()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userId, out int id) ? id : 0;
        }

        // Create a new letter request.
        [HttpPost("createrequest")]
        public IActionResult CreateRequest(
            [FromBody] LetterRequestDto dto)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.LetterTypeName))
            {
                return BadRequest("A valid letter type name is required.");
            }

            try
            {
                int requestId = _business.CreateRequest(userId,dto.LetterTypeName);

                if (requestId == 0)
                {
                    return BadRequest("Could not create the letter request.");
                }

                return Ok(new
                {
                    message = "Letter request created successfully.",
                    letterRequestId = requestId,
                    letterTypeName = dto.LetterTypeName,
                    status = "Pending"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Get logged-in employee's requests.
        [HttpGet("myrequest")]
        public IActionResult GetMyRequests()
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            return Ok(_business.GetMyRequests(userId));
        }

        // POST: /api/LetterRequest/cancel?id=req_id
        // Cancel the logged-in employee's pending request.
        [HttpPost("cancel")]
        public IActionResult CancelRequest([FromQuery] int id)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid letter request ID is required."
                });
            }

            try
            {
                int result = _business.CancelRequest(id, userId);

                if (result == 0)
                {
                    return NotFound(new
                    {
                        message = "The request was not found, does not belong to you, or is no longer pending."
                    });
                }

                if (result != 1)
                {
                    return StatusCode(500, new
                    {
                        message = "The letter request could not be cancelled."
                    });
                }

                return Ok(new
                {
                    message = "Letter request cancelled successfully.",
                    letterRequestId = id,
                    status = "Cancelled"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return StatusCode(500, new
                {
                    message = "An error occurred while cancelling the letter request."
                });
            }
        }
    }
}
