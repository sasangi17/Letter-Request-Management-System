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
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(userId, out int id) ? id : 0;
        }
        //Create a new letter request
        [HttpPost("createrequest")]
        public IActionResult CreateRequest(
            [FromBody] LetterRequestDto dto)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                return Unauthorized();
            }

            int requestId = _business.CreateRequest(
                userId, dto.LetterTypeId);

            if (requestId == 0)
            {
                return BadRequest(
                    "Could not create the request. Check that the selected letter type exists.");
            }

            var request = _business.GetRequestById(
                requestId, userId);

            if (request == null)
            {
                return StatusCode(
                    500, "The request was created but could not be retrieved.");
            }

            return CreatedAtAction(
                nameof(GetRequestById),
                new { id = requestId },
                request);
        }

        [HttpGet("myrequest")]
        public IActionResult GetMyRequests()
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                return Unauthorized();
            }

            return Ok(_business.GetMyRequests(userId));
        }

        [HttpGet("requestbyid")]
        public IActionResult GetRequestById(int id)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                return Unauthorized();
            }

            var request = _business.GetRequestById(id, userId);

            if (request == null)
            {
                return NotFound(
                    "The letter request was not found.");
            }

            return Ok(request);
        }
    }
}
