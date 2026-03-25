using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class MembersController(IMemberRepository memberRepository) : BaseApiController
{
   [HttpGet]
   public async Task<ActionResult<IReadOnlyList<Member>>> Get()
   {
      return Ok(await memberRepository.GetMembersAsync());

   }

   [Authorize]
   [HttpGet("{id}")]
   public async Task<ActionResult<Member>> Get(string id)
   {
      var member = await memberRepository.GetMemberByIdAsync(id);

      if (member == null) return NotFound();
      
      return member;
   }

   [HttpGet("{id}/photos")]
   public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
   {
      return Ok(await memberRepository.GetPhotosForMemberAsync(id));
   }
}