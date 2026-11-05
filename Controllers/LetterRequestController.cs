using System.Security.Claims;
using Letter_Request_Management_System.Business;
using Letter_Request_Management_System.Models.Dto;
using Letter_Request_Management_System.Services;
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
        private readonly SignedLetterPdfService _signedLetterPdfService;

        public LetterRequestController(
            LetterRequestBusiness business,
            SignedLetterPdfService signedLetterPdfService)
        {
            _business = business;
            _signedLetterPdfService = signedLetterPdfService;
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

        // Preview a pending letter.
        [HttpGet("preview")]
        public IActionResult PreviewLetter(int id)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Invalid letter request ID.");

            var pendingLetter = _business.GetPendingLetterForPreview(id, userId);

            if (pendingLetter == null)
            {
                return NotFound("The pending letter was not found or is not available for preview.");
            }

            return Ok(new
            {
                letterRequestId = pendingLetter.LetterRequestId,
                letterTypeName = pendingLetter.LetterTypeName,
                letterContent = pendingLetter.LetterContent,
                status = pendingLetter.Status,
                watermark = "NOT APPROVED"
            });
        }

        // Download an approved letter with the saved signature snapshot.
        [HttpGet("letterdownload")]
        public IActionResult DownloadLetter(int id)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Invalid letter request ID.");

            var letter = _business.GetApprovedLetterForDownload(id, userId);

            if (letter == null)
            {
                return NotFound("The letter was not found, is not approved, or has no saved signature.");
            }

            try
            {
                byte[] pdfBytes =
                    _signedLetterPdfService.GenerateSignedPdfBytes(
                        letter.LetterContent,
                        letter.ApproverName,
                        letter.ApprovedAt,
                        letter.SignatureImage);

                string fileName = BuildPdfFileName(
                    letter.EmployeeName,
                    letter.LetterTypeName,
                    id);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return StatusCode(500,
                    "An error occurred while generating the approved letter PDF.");
            }
        }

        // Builds a name like "Ariyan's Service Letter.pdf"
        private static string BuildPdfFileName(
            string employeeName,
            string letterTypeName,
            int fallbackId)
        {
            if (string.IsNullOrWhiteSpace(employeeName) ||
                string.IsNullOrWhiteSpace(letterTypeName))
            {
                return $"Approved-Letter-{fallbackId}.pdf";
            }

            // Capitalise each word of the letter type: "service letter" -> "Service Letter"
            string typeName = System.Globalization.CultureInfo.CurrentCulture
                .TextInfo.ToTitleCase(letterTypeName.Trim().ToLowerInvariant());

            string name = $"{employeeName.Trim()}'s {typeName}";

            // Remove characters not allowed in file names
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), string.Empty);

            return $"{name}.pdf";
        }

        // PUT: /api/LetterRequest/cancel?id=17
        // Cancel the logged-in employee's pending request.
        [HttpPut("cancel")]
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
