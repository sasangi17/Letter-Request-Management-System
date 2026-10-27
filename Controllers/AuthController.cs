using Letter_Request_Management_System.Services;
using Letter_Request_Management_System.Business;
using Letter_Request_Management_System.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Letter_Request_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthBusiness _authBusiness;
        private readonly JwtService _jwtService;

        public AuthController(
            AuthBusiness authBusiness,
            JwtService jwtService)
        {
            _authBusiness = authBusiness;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null ||
                string.IsNullOrWhiteSpace(loginDto.Email) ||
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest(new
                {
                    message = "Email and password are required."
                });
            }

            // Verify the user's credentials.
            var user = _authBusiness.Login(
                loginDto.Email.Trim(),
                loginDto.Password);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var roles = user.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (roles.Count == 0 ||
                roles.Any(role =>
                    !role.Equals("Admin",
                        StringComparison.OrdinalIgnoreCase) &&
                    !role.Equals("Employee",
                        StringComparison.OrdinalIgnoreCase)))
            {
                return StatusCode(500, new
                {
                    message = "Invalid role configuration for this account."
                });
            }

            // If no role was selected and multiple roles are available,
            // return the role-selection response without issuing a token.
            if (string.IsNullOrWhiteSpace(loginDto.SelectedRole))
            {
                if (roles.Count > 1)
                {
                    return Ok(new
                    {
                        requiresRoleSelection = true,
                        roles
                    });
                }
            }

            string selectedRole;

            if (roles.Count == 1)
            {
                selectedRole = roles[0];

                // If a role was supplied, it must match the user's role.
                if (!string.IsNullOrWhiteSpace(loginDto.SelectedRole) &&
                    !selectedRole.Equals(
                        loginDto.SelectedRole.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = "The selected role is not assigned to this account."
                    });
                }
            }
            else
            {
                selectedRole = loginDto.SelectedRole!.Trim();

                bool hasSelectedRole = roles.Any(role =>
                    role.Equals(
                        selectedRole,
                        StringComparison.OrdinalIgnoreCase));

                if (!hasSelectedRole)
                {
                    return BadRequest(new
                    {
                        message = "The selected role is not assigned to this account."
                    });
                }

                // Use the canonical role name from the database.
                selectedRole = roles.First(role =>
                    role.Equals(
                        selectedRole,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Generate a token only after credentials and role are verified.
            string token = _jwtService.GenerateToken(
                user, selectedRole);

            var result = new UserDto
            {
                Token = token,
                UserId = user.UserId,
                EmpNo = user.EmpNo,
                Email = user.Email,
                FullName = user.FullName,
                Department = user.Department,
                Position = user.Position,
                Roles = roles,
                ExpiresAt = _jwtService.GetExpiration()
            };

            return Ok(new
            {
                requiresRoleSelection = false,
                result.Token,
                result.UserId,
                result.EmpNo,
                result.Email,
                result.FullName,
                result.Department,
                result.Position,
                role = selectedRole,
                result.Roles,
                result.ExpiresAt
            });
        }
    }
}
