using Letter_Request_Management_System.Business;
using Letter_Request_Management_System.Models.Dto;
using Letter_Request_Management_System.Services;
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
        private readonly AdminSignatureFileService _signatureFileService;

        public AdminController(
            AdminBusiness adminBusiness,
            AdminSignatureBusiness adminSignatureBusiness,
            AdminSignatureFileService signatureFileService)
        {
            _adminBusiness = adminBusiness;
            _adminSignatureBusiness = adminSignatureBusiness;
            _signatureFileService = signatureFileService;
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

        // PUT: /api/Admin/requestsapprove?id=3

        [HttpPut("requestsapprove")]
        public IActionResult ApproveRequest([FromQuery] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid letter request ID is required."
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
                int result = _adminBusiness.ApproveRequest(
                    id,
                    adminUserId);

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
                // Log ex on the backend for debugging.
                Console.WriteLine(ex);

                return StatusCode(500, new
                {
                    message = "An error occurred while approving the request and generating the PDF."
                });
            }
        }




        // POST: /api/Admin/signature
        [HttpPost("signature")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSignature(IFormFile signature)
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

            if (signature == null || signature.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select a signature image."
                });
            }

            try
            {
                // Save the image in the protected folder.
                string signatureReference =
                    await _signatureFileService.SaveSignatureAsync(signature);

                // Save its reference in SQL Server.
                bool saved =
                    _adminSignatureBusiness.SaveSignatureReference(
                        adminUserId,
                        signatureReference);

                if (!saved)
                {
                    return StatusCode(500, new
                    {
                        message = "The signature reference could not be saved."
                    });
                }

                return Ok(new
                {
                    message = "Admin signature uploaded and database reference saved successfully.",
                    signatureReference
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while saving the signature. Check the backend terminal for details."
                });
            }
        }



    }
}
