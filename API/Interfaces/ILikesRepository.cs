using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data.Helpers;
using API.DTOs;
using API.Entity;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int LikedUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams);
    }
}