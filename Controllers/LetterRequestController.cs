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
        private readonly AdminSignatureFileService _signatureFileService;
        private readonly SignedLetterPdfService _signedLetterPdfService;
        

        public LetterRequestController(
            LetterRequestBusiness business,
            AdminSignatureFileService signatureFileService,
            SignedLetterPdfService signedLetterPdfService)
        {
            _business = business;
            _signatureFileService = signatureFileService;
            _signedLetterPdfService = signedLetterPdfService;

        }

        private int GetLoggedInUserId()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userId, out int id) ? id : 0;
        }

        // Create a new letter request
        [HttpPost("createrequest")]
        public IActionResult CreateRequest([FromBody] LetterRequestDto dto)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            if (dto == null || dto.LetterTypeId <= 0)
                return BadRequest("A valid letter type is required.");

            int requestId = _business.CreateRequest(userId, dto.LetterTypeId);

            if (requestId == 0)
            {
                return BadRequest(
                    "Could not create the request. Check that the selected letter type exists.");
            }

            return Ok(new
            {
                message = "Letter request created successfully.",
                letterRequestId = requestId,
                status = "Pending"
            });
        }

        // Get logged-in employee's requests
        [HttpGet("myrequest")]
        public IActionResult GetMyRequests()
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            return Ok(_business.GetMyRequests(userId));
        }

        // Preview a pending letter inside the application
        [HttpGet("preview")]
        public IActionResult PreviewLetter(int id)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Invalid letter request ID.");

            var pendingLetter = _business.GetPendingLetterForPreview(
                id,
                userId);

            if (pendingLetter == null)
            {
                return NotFound(
                    "The pending letter was not found or is not available for preview.");
            }

            // Return letter content as JSON instead of a downloadable PDF.
            return Ok(new
            {
                letterRequestId = pendingLetter.LetterRequestId,
                letterTypeName = pendingLetter.LetterTypeName,
                letterContent = pendingLetter.LetterContent,
                status = pendingLetter.Status,
                watermark = "NOT APPROVED"
            });
        }

        // Download an approved letter with the administrator's signature
        [HttpGet("letterdownload")]
        public IActionResult DownloadLetter(int id)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Invalid letter request ID.");

            var approvedLetter =
                _business.GetApprovedLetterForDownload(id, userId);

            if (approvedLetter == null)
            {
                return NotFound(
                    "The letter was not found, is not approved, or has no saved signature reference.");
            }

            var letter = approvedLetter.Value;

            string? signatureImagePath =
                _signatureFileService.GetSignatureFullPath(
                    letter.SignatureReference);

            if (string.IsNullOrWhiteSpace(signatureImagePath) ||
                !System.IO.File.Exists(signatureImagePath))
            {
                return NotFound(
                    "The administrator's signature image could not be found. Please ask the administrator to upload the signature again.");
            }

            try
            {
                byte[] pdfBytes =
                    _signedLetterPdfService.GenerateSignedPdfBytes(
                        letter.LetterContent,
                        letter.ApproverName,
                        letter.ApprovedAt,
                        signatureImagePath);

                string fileName = $"Approved-Letter-{id}.pdf";

                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return StatusCode(
                    500,
                    "An error occurred while generating the approved letter PDF. Check the backend terminal for details.");
            }
        }
    }
}
