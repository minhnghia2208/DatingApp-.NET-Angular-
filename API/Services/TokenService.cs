using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entity;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _Akey;
        private readonly SymmetricSecurityKey _Rkey;

        private readonly UserManager<AppUser> _userManager;
        private readonly IDistributedCache _cache;

        public TokenService(IConfiguration config
            , UserManager<AppUser> userManager
            , IDistributedCache cache)
        {
            _userManager = userManager;
            _cache = cache;
            _Akey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["AccessKey"]));
            _Rkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["RefreshKey"]));

        }

        public async Task<string> CreateToken1(AppUser user)
        {
            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role =>
                new Claim(ClaimTypes.Role, role)
            ));
            var creds = new SigningCredentials(_Akey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(-10),
                SigningCredentials = creds,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<string> CreateToken(AppUser user)
        {
            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role =>
                new Claim(ClaimTypes.Role, role)
            ));
            var creds = new SigningCredentials(_Akey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(30),
                SigningCredentials = creds,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        public async Task<string> Create_RToken(AppUser user)
        {
            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(type: "refreshtoken", value: "true"),
            };
            var creds = new SigningCredentials(_Rkey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Add Token into Redis
            await _cache.SetRecordAsync(user.Id.ToString(), tokenHandler.WriteToken(token));
            
            return tokenHandler.WriteToken(token);
        }
    }
}