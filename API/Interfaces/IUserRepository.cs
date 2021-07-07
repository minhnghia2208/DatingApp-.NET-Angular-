using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data.Helpers;
using API.DTOs;
using API.Entity;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUserAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);
        Task<MemberDTO> GetMemberAsync(string username);
        Task<MemberDTO> GetMemberByIdAsync(int id);
        Task<string> GetUserGender(string username);
    }
}