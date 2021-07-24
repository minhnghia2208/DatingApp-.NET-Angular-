using System.Threading.Tasks;
using API.Entity;

namespace API.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken1(AppUser user); 
        Task<string> CreateToken(AppUser user); 
        Task<string> Create_RToken(AppUser user);
    }
}