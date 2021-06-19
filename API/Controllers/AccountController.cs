using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO register)
        {
            if (await CheckUnique(register.Username))
                return BadRequest("User already exists");
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = register.Username.ToLower(),
                PasswordHarsh = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
                PasswordSalt = hmac.Key,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDTO{
                UserName = register.Username.ToLower(),
                Access_Token = _tokenService.CreateToken(user),
            };
        }
        private async Task<bool> CheckUnique(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> login(LoginDTO login)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == login.UserName);
            if (user == default)
                return Unauthorized("UserName Invalid");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
            for (int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHarsh[i])
                    return Unauthorized("Password Invalid");
            }
            return new UserDTO{
                UserName = login.UserName.ToLower(),
                Access_Token = _tokenService.CreateToken(user),
                Refresh_Token = _tokenService.CreateToken(user)
            };
        }
    }
}