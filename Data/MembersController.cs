using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

[Route("api/[controller]")]
[ApiController]
public class MembersController(AppDbContext context) : ControllerBase
{
   [HttpGet]
   public async Task<ActionResult<IReadOnlyList<AppUser>>> Get()
   {
      var member = await context.Users.ToListAsync();
      
      return member;
   }

   [HttpGet("{id}")]
   public async Task<ActionResult<AppUser>> Get(Guid id)
   {
      var member = await context.Users.FindAsync(id);

      if (member == null) return NotFound();
      
      return member;
   }
}