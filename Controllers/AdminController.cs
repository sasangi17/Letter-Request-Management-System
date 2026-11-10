using Letter_Request_Management_System.Business;
using Letter_Request_Management_System.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Letter_Request_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminBusiness _adminBusiness;
        private readonly AdminSignatureBusiness _adminSignatureBusiness;

        public AdminController(
            AdminBusiness adminBusiness,
            AdminSignatureBusiness adminSignatureBusiness)
        {
            _adminBusiness = adminBusiness;
            _adminSignatureBusiness = adminSignatureBusiness;
        }

        // GET: /api/Admin/requests
        // Retrieve all employee letter requests.
        [HttpGet("requests")]
        public IActionResult GetAllRequests()
        {
            var requests = _adminBusiness.GetAllRequests();

            return Ok(requests);
        }

        // POST: /api/Admin/requestsapprove?id=3
        // Approve an employee letter request.
        [HttpPost("requestsapprove")]
        public IActionResult ApproveRequest([FromQuery] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid letter request ID is required."
                });
            }

            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int adminUserId))
            {
                return Unauthorized(new
                {
                    message = "The authenticated admin user ID is missing."
                });
            }

            try
            {
                int result = _adminBusiness.ApproveRequest(id,adminUserId);

                if (result == 0)
                {
                    return NotFound(new
                    {
                        message = "Letter request not found or is no longer pending."
                    });
                }

                if (result == 2)
                {
                    return Conflict(new
                    {
                        message = "This request has already been processed."
                    });
                }

                if (result == 3)
                {
                    return BadRequest(new
                    {
                        message = "Upload your admin signature before approving a request."
                    });
                }

                if (result == 4)
                {
                    return BadRequest(new
                    {
                        message = "The saved signature image could not be found. Upload it again."
                    });
                }

                if (result == 5)
                {
                    return BadRequest(new
                    {
                        message = "The administrator's full name could not be found."
                    });
                }

                if (result != 1)
                {
                    return StatusCode(500, new
                    {
                        message = "The request could not be approved."
                    });
                }

                return Ok(new
                {
                    message = "Letter approved and signed PDF generated successfully.",
                    letterRequestId = id,
                    status = "Approved"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return StatusCode(500, new
                {
                    message = "An error occurred while approving the request and generating the PDF."
                });
            }
        }


        [HttpPost("signature")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSignature(IFormFile? signature)
        {
            try
            {
                var adminUserIdValue = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (!int.TryParse(adminUserIdValue, out int adminUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid admin user."
                    });
                }

                if (signature == null || signature.Length == 0)
                {
                    return BadRequest(new
                    {
                        message = "Please select a signature image."
                    });
                }

                if (signature.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        message = "Signature image must not exceed 2 MB."
                    });
                }

                string extension = Path.GetExtension(signature.FileName)
                    .ToLowerInvariant();

                if (extension != ".png" &&
                    extension != ".jpg" &&
                    extension != ".jpeg")
                {
                    return BadRequest(new
                    {
                        message = "Only PNG and JPG signature images are allowed."
                    });
                }

                using var memoryStream = new MemoryStream();

                await signature.CopyToAsync(memoryStream);

                byte[] signatureBytes = memoryStream.ToArray();

                bool isPng =
                    signatureBytes.Length >= 8 &&
                    signatureBytes[0] == 0x89 &&
                    signatureBytes[1] == 0x50 &&
                    signatureBytes[2] == 0x4E &&
                    signatureBytes[3] == 0x47 &&
                    signatureBytes[4] == 0x0D &&
                    signatureBytes[5] == 0x0A &&
                    signatureBytes[6] == 0x1A &&
                    signatureBytes[7] == 0x0A;

                bool isJpeg =
                    signatureBytes.Length >= 3 &&
                    signatureBytes[0] == 0xFF &&
                    signatureBytes[1] == 0xD8 &&
                    signatureBytes[2] == 0xFF;

                if (!isPng && !isJpeg)
                {
                    return BadRequest(new
                    {
                        message = "The uploaded file is not a valid PNG or JPG image."
                    });
                }

                string contentType = isPng ? "image/png" : "image/jpeg";

                bool saved = _adminSignatureBusiness.SaveSignature(
                    adminUserId,
                    signatureBytes,
                    contentType,
                    Path.GetFileName(signature.FileName));

                if (!saved)
                {
                    return StatusCode(500, new
                    {
                        message = "Failed to save the admin signature."
                    });
                }

                return Ok(new
                {
                    message = "Admin signature uploaded successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("SIGNATURE UPLOAD ERROR:");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "An error occurred while saving the signature.",
                    error = ex.Message
                });
            }
        }

        // GET: /api/Admin/signature
        // Retrieve the logged-in admin's saved signature.
        [HttpGet("signature")]
        public IActionResult GetSignature()
        {
            string? userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int adminUserId))
            {
                return Unauthorized(new
                {
                    message = "Invalid admin user information."
                });
            }

            var signature = _adminSignatureBusiness.GetSignature(adminUserId);

            if (signature == null ||
                signature.SignatureImage == null ||
                signature.SignatureImage.Length == 0)
            {
                return NotFound(new
                {
                    message = "No signature has been uploaded."
                });
            }

            return File(
                signature.SignatureImage,
                signature.ContentType
            );
        }

        // POST: /api/Admin/requestsreject?id=17
        // Reject an employee letter request.
        [HttpPost("requestsreject")]
        public IActionResult RejectRequest(
            [FromQuery] int id,
            [FromBody] RejectRequestDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid letter request ID is required."
                });
            }

            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                return BadRequest(new
                {
                    message = "A rejection reason is required."
                });
            }

            if (dto.RejectionReason.Trim().Length > 1000)
            {
                return BadRequest(new
                {
                    message = "The rejection reason must not exceed 1000 characters."
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

            try
            {
                int result = _adminBusiness.RejectRequest(
                    id,
                    adminUserId,
                    dto.RejectionReason);

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
                        message = "This request has already been processed."
                    });
                }

                if (result == 6)
                {
                    return BadRequest(new
                    {
                        message = "A rejection reason is required."
                    });
                }

                if (result == 7)
                {
                    return BadRequest(new
                    {
                        message = "The rejection reason must not exceed 1000 characters."
                    });
                }

                if (result != 1)
                {
                    return StatusCode(500, new
                    {
                        message = "The request could not be rejected."
                    });
                }

                return Ok(new
                {
                    message = "Letter request rejected successfully.",
                    letterRequestId = id,
                    status = "Rejected",
                    rejectionReason = dto.RejectionReason.Trim()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return StatusCode(500, new
                {
                    message = "An error occurred while rejecting the letter request."
                });
            }
        }
    }
}
