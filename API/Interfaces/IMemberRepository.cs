using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMemberRepository
{
    void Update(Member member);
    Task<Member?> GetMemberByIdAsync(string id);
    Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams);
    Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId);
    Task<bool> SaveAllAsync();
    Task<Member?> GetMemberForUpdate(string id);
}
