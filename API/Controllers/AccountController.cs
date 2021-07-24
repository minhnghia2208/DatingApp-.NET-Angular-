using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Distributed;
using API.Extensions;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly ITokenService _tokenService;
        private readonly IDistributedCache _cache;

        public IMapper _mapper { get; }
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager
            , SignInManager<AppUser> signInManager
            , ITokenService tokenService
            , IDistributedCache cache
            , IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _cache = cache;
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
                Access_Token = await _tokenService.CreateToken1(user),
                Refresh_Token = await _tokenService.Create_RToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                nLike = user.nLike,
                LikeRead = user.LikeRead,
            };
        }

        [Authorize(Policy = "RefreshToken")]
        [HttpPost("refresh1")]
        public async Task<ActionResult<UserDTO>> refresh1(TokenDTO token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Access_token);
           
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value !=
                jwtToken.Claims.First(claim => claim.Type == "nameid" )?.Value)
                return Unauthorized("User is unauthorized");

            var username = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "unique_name").Value;
            if (username == default) return Unauthorized();

            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => 
                    x.UserName == username);

            return new UserDTO
            {
                UserName = username.ToLower(),
                Access_Token = await _tokenService.CreateToken(user),
                Refresh_Token = await _tokenService.Create_RToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                nLike = user.nLike,
                LikeRead = user.LikeRead,
            };
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<UserDTO>> refresh(TokenDTO token)
        {
            var ahandler = new JwtSecurityTokenHandler();
            var aToken = ahandler.ReadJwtToken(token.Access_token);
            var userId = aToken.Claims.FirstOrDefault(claim => claim.Type == "nameid").Value;

            var Refresh_token =  await _cache.GetRecordAsync<string>(userId);
            if (Refresh_token is null)
                return Unauthorized("Invalid Refresh Token");
                
            var rhandler = new JwtSecurityTokenHandler();
            var rToken = rhandler.ReadJwtToken(Refresh_token);

            if (rToken.Claims.First(claim => claim.Type == "refreshtoken") == null)
                return Unauthorized("Invalid Token");
           
            if (rToken.Claims.First(claim => claim.Type == "nameid" )?.Value !=
                aToken.Claims.First(claim => claim.Type == "nameid" )?.Value)
                return Unauthorized("User is unauthorized");

            var username = aToken.Claims.FirstOrDefault(claim => claim.Type == "unique_name").Value;
            if (username == default) return Unauthorized();

            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => 
                    x.UserName == username);

            return new UserDTO
            {
                UserName = username.ToLower(),
                Access_Token = await _tokenService.CreateToken(user),
                Refresh_Token = await _tokenService.Create_RToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                nLike = user.nLike,
                LikeRead = user.LikeRead,
            };
        }

    }
}