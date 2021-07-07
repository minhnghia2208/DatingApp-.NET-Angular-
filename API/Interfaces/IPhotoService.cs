using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace API.Interfaces
{
    public interface IPhotoService
    {
        Task<string> AddPhotoAsync (IFormFile file, string username);
        void DeletePhoto (string imagePath);
    }
}