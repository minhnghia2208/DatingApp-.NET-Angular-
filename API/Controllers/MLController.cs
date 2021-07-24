using API.DTOs;
using API.Entity;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class MLController : BaseAPIController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMLService _mLService;
        private CustomVisionTrainingClient trainingApi;
        private CustomVisionPredictionClient predictionApi;
        private static Guid projectId = Guid.Parse("c13464db-3ccd-4c23-a1e0-a1682afa0f0c");
        private static Iteration iteration;
        private static string publishedModelName = "faceClassModel";
        private static MemoryStream testImage;
        
        public MLController(IMapper mapper
            , IPhotoService photoService
            , IUnitOfWork unitOfWork
            , IMLService mLService)
        {
            _unitOfWork = unitOfWork;
            _mLService = mLService;
            _photoService = photoService;
            _mapper = mapper;
            
            trainingApi =  _mLService.AuthenticateTraining();
            predictionApi = _mLService.AuthenticatePrediction();
        }
        
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("Create")]
        public Guid Create(){
            Console.WriteLine("Creating new project:");
            Project project = trainingApi.CreateProject("My New Project");

            Tag beautyTag = trainingApi.CreateTag(project.Id, "Beautiful");
            Tag averageTag = trainingApi.CreateTag(project.Id, "Average");
            
            List<string> Beautiful = Directory.GetFiles(Path.Combine("StaticFiles", "Images", "Beautiful")).ToList();
            List<string> Average = Directory.GetFiles(Path.Combine("StaticFiles", "Images", "Average")).ToList();

            foreach (var image in Beautiful)
            {
                using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(image)))
                {
                    trainingApi.CreateImagesFromData(project.Id, stream, new List<Guid>() { beautyTag.Id });
                }
            }

            // Or uploaded in a single batch 
            var imageFiles = Average.Select(img => new ImageFileCreateEntry(Path.GetFileName(img), System.IO.File.ReadAllBytes(img))).ToList();
            trainingApi.CreateImagesFromFiles(project.Id, new ImageFileCreateBatch(imageFiles, new List<Guid>() { averageTag.Id }));

            iteration = trainingApi.TrainProject(project.Id);
            return project.Id;     
        }

        [Authorize]
        [HttpPost("add-photo")]
        [ActionName(nameof(AddPhoto))]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {

            var username = User.GetUsername();
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            var dbPath = await _photoService.AddPhotoAsync(file, username);

            var photo = new Photo
            {
                Url = dbPath
            };
            
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;     
            }
            // Analyze attractiveness
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), photo.Url);
            var result = _mLService.TestIteration(predictionApi, fullPath);
            user.Attractiveness = result.Predictions[0].TagName;
            _unitOfWork.UserRepository.Update(user);

            user.Photos.Add(photo);

            if (await _unitOfWork.Complete())
            {
                return Ok("Image Uploaded");
            }
            return BadRequest("Problem addding photo");
        }
        [HttpGet("Train")]
        public string training(){
            
            _mLService.CreateProject(trainingApi);
            
            return "success";
        }

        [HttpGet("Predict")]
        public void predict(string fullPath){
            testImage = new MemoryStream(System.IO.File.ReadAllBytes(fullPath));
            
            Console.WriteLine("Making a prediction:");

            var result = predictionApi.ClassifyImage(projectId, publishedModelName, testImage);
        }

        [HttpGet("Delete")]
        public string Delete(){
            Console.WriteLine("Unpublishing iteration.");
            trainingApi.UnpublishIteration(projectId, iteration.Id);

            Console.WriteLine("Deleting project.");
            trainingApi.DeleteProject(projectId);
            return("Project Deleted");
            // _mLService.DeleteImages(trainingApi);
            // return "success";
        }

        [HttpGet("testing")]
        public void testing(){
            var iters = trainingApi.GetIterations(projectId);
            
            if (iters.Count > 0){
                foreach (var iter in iters){
                    if (iter.PublishName != null){
                        Console.WriteLine("TESTY TESTY TESTY TESTY");
                        Console.WriteLine("TESTY TESTY TESTY TESTY");
                        Console.WriteLine("TESTY TESTY TESTY TESTY");
                        Console.WriteLine(iter.PublishName);
                        Console.WriteLine("TESTY TESTY TESTY TESTY");
                        Console.WriteLine("TESTY TESTY TESTY TESTY");
                        Console.WriteLine("TESTY TESTY TESTY TESTY");
                        trainingApi.UnpublishIteration(projectId, iter.Id);
                    }
                }
            }
        }
    }
}