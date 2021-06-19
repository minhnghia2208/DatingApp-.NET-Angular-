using API.Entity;

namespace API.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user); 
    }
}