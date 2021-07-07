using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly ITokenService _tokenService;
        public IMapper _mapper { get; }
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, 
            ITokenService tokenService, IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO register)
        {
            if (await CheckUnique(register.Username))
                return BadRequest("User already exists");
            var user = _mapper.Map<AppUser>(register);
            user.UserName = register.Username.ToLower();
            var result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return BadRequest(result.Errors);
            return new UserDTO
            {
                UserName = register.Username.ToLower(),
                Access_Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };
        }
        private async Task<bool> CheckUnique(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> login(LoginDTO login)
        {
            // var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == login.UserName);
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == login.UserName.ToLower());
            if (user == default)
                return Unauthorized("UserName is Invalid");
            var result = await _signInManager
                .CheckPasswordSignInAsync(user, login.Password, false);
            if (!result.Succeeded) return Unauthorized();
            return new UserDTO
            {
                UserName = login.UserName.ToLower(),
                Access_Token = await _tokenService.CreateToken(user),
                Refresh_Token = _tokenService.Create_RToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        // Will add analyizing JWT to get user information instead of asking for username
        // Will revoke the old token 
        [HttpPost("refresh")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> refresh(LoginDTO login)
        {
            return await this.login(login);
        }

    }
}