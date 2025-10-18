using HRMSystem.Data;
using Microsoft.AspNetCore.Mvc;

namespace HRMSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RoleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var roles = _context.Roles
                .Select(r => new { id = r.Id, name = r.RoleName })
                .ToList();
            return Ok(roles);
        }
    }

}
