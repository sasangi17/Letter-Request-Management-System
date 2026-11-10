using Letter_Request_Management_System.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Letter_Request_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee")]
    public class LetterPdfController : ControllerBase
    {
        private readonly LetterPdfBusiness _letterPdfBusiness;

        public LetterPdfController(LetterPdfBusiness letterPdfBusiness)
        {
            _letterPdfBusiness = letterPdfBusiness;
        }

        // GET: api/LetterPdf/preview/1
        [HttpGet("preview")]
        public IActionResult PreviewLetter(int id)
        {
            int userId = GetCurrentUserId();

            if (userId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user information."
                });
            }

            var letter = _letterPdfBusiness.GetPreviewLetter(
                id,
                userId);

            if (letter == null)
            {
                return NotFound(new
                {
                    message = "Pending letter request not found."
                });
            }

            return Ok(new
            {
                letterRequestId = id,
                letterContent = letter.LetterContent,
                status = letter.Status,
                message = "This is an unofficial letter preview. It is not approved."
            });
        }

        // GET: api/LetterPdf/download?id=1
        [HttpGet("download")]
        public IActionResult DownloadLetter(int id)
        {
            int userId = GetCurrentUserId();

            if (userId <= 0)
                return Unauthorized(new
                {
                    message = "Invalid user information."
                });

            var result = _letterPdfBusiness.GetApprovedPdfForDownload(id, userId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Approved letter PDF not found."
                });
            }

            return File(
                result.Value.PdfBytes,
                "application/pdf",
                result.Value.FileName);
        }


        private int GetCurrentUserId()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?? User.FindFirst("UserId")?.Value;

            return int.TryParse(userId, out int parsedUserId)
                ? parsedUserId
                : 0;
        }
    }
}