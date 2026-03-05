using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class MembersController(AppDbContext context) : BaseApiController
{
   [HttpGet]
   public async Task<ActionResult<IReadOnlyList<AppUser>>> Get()
   {
      var member = await context.Users.ToListAsync();
      
      return member;
   }

   [Authorize]
   [HttpGet("{id}")]
   public async Task<ActionResult<AppUser>> Get(Guid id)
   {
      var member = await context.Users.FindAsync(id);

      if (member == null) return NotFound();
      
      return member;
   }
}