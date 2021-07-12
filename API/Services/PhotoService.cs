using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using API.Interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace API.services
{
    public class PhotoService : IPhotoService
    {
        
        public async Task<string> AddPhotoAsync(IFormFile file, string username)

        {
            var folderName = Path.Combine("StaticFiles", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            // var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (file.Length > 0)
            {
                var fileName = DateTime.Now.ToString("Mddyyyyhhmmsstt")+ "-" + username + "-" + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')  ;
                System.Console.WriteLine(fileName);
                
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(folderName, fileName);
                using var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream());
                image.Mutate(x => x.Resize(500, 500));
                await image.SaveAsync(fullPath);
                // using (var stream = new FileStream(fullPath, FileMode.Create))
                // {
                //     await file.CopyToAsync(stream);
                // }
                return dbPath;
            }
            else
            {
                return "";
            }
        }

        public void DeletePhoto(string imagePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), imagePath);

            File.Delete(fullPath);
            
        }

    }
}