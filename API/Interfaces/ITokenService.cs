using System.Threading.Tasks;
using API.Entity;

namespace API.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user); 
        string Create_RToken(AppUser user);
    }
}