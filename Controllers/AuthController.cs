using HRMSystem.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HRMSystem.Services;

namespace HRMSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        public AuthController(AppDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Invalid login request");
            }

            var employee = await _context.Employees
                .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Roles)
                .FirstOrDefaultAsync(e => e.Email == login.Email);
            if (employee == null)
            {
                return BadRequest("Invalid email or password");
            }

            var hasher = new PasswordHasher<Employee>();
            var result = hasher.VerifyHashedPassword(employee, employee.HashPassword, login.Password);

            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest("Invalid email or password");
            }
            switch (employee.Status)
            {
                case EmployeeStatus.Resigned:
                    return BadRequest("Your account has been resigned.");
                case EmployeeStatus.Retired:
                    return BadRequest("Your account is retired.");
                case EmployeeStatus.OnLeave:
                    return BadRequest("Your account is temporarily inactive (On Leave).");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Email),
            };

            foreach (var role in employee.EmployeeRoles.Select(er => er.Roles.RoleName))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok("Logged out successfully");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == dto.Email);

            if (employee == null) return Ok();

            var token = Guid.NewGuid().ToString();

            employee.ResetPasswordToken = token;
            employee.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:3000/auth/reset-password?token={token}";

            await _emailService.SendAsync(
                employee.Email,
                "Reset your password",
                $"Click here to reset your password:\n{resetLink}"
            );

            return Ok();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e =>
                e.ResetPasswordToken == dto.Token &&
                e.ResetPasswordExpiry > DateTime.UtcNow);

            if (employee == null)
                return BadRequest("Invalid or expired token");

            employee.HashPassword = new PasswordHasher<Employee>()
                .HashPassword(employee, dto.NewPassword);

            employee.ResetPasswordToken = null;
            employee.ResetPasswordExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Password reset successful");
        }

    }
}
