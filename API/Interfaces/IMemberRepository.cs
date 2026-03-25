using API.Entities;

namespace API.Interfaces;

public interface IMemberRepository
{
   void Update (Member member);
   Task<Member?> GetMemberByIdAsync(string id);
   Task<IReadOnlyList<Member>> GetMembersAsync();
   Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId);
   Task<bool> SaveAllAsync();
}