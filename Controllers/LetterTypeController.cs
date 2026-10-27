using Letter_Request_Management_System.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Letter_Request_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LetterTypeController : ControllerBase
    {
        private readonly LetterTypeBusiness _letterTypeBusiness;

        public LetterTypeController(
            LetterTypeBusiness letterTypeBusiness)
        {
            _letterTypeBusiness = letterTypeBusiness;
        }

        [HttpGet]
        public IActionResult GetAllLetterTypes()
        {
            var letterTypes =
                _letterTypeBusiness.GetAllLetterTypes();

            return Ok(letterTypes);
        }
    }
}