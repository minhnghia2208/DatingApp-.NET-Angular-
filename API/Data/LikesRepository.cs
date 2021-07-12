using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data.Helpers;
using API.DTOs;
using API.Entity;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context)
        {
        _context = context;
        }

        public Photo GetMainPhoto(string userName)
        {
            var user = _context.Users
                .Include(p => p.Photos)
                .FirstOrDefault(x => x.UserName == userName);
                
    
            foreach (var photo in user.Photos){
                if (photo.IsMain) return photo;
            }
            return null;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int LikedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, LikedUserId);
        }

        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();
            if (likesParams.Predicate == "liked"){
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.LikedUser);
            }
            if (likesParams.Predicate == "likedBy"){
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }
            var likedUsers = users.Select(user => new LikeDTO {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id
            });
            return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
        
    }
}