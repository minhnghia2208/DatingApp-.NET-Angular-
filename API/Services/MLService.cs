using System;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Interfaces;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace API.Services
{
    public class MLService : IMLService
    {
        private static Guid projectId = Guid.Parse("c13464db-3ccd-4c23-a1e0-a1682afa0f0c");
        private static string trainingEndpoint = "https://nghia.cognitiveservices.azure.com/";
        private static string trainingKey = "";
        private static string predictionEndpoint = "https://nghia-prediction.cognitiveservices.azure.com/";
        private static string predictionKey = "";
        private static string predictionResourceId = "";
        private static List<string> Beautiful;
        private static List<string> Average;
        private static Tag beautyTag;
        private static Tag averageTag;
        private static Iteration iteration;
        private static string publishedModelName = "faceClassModel";
        private static MemoryStream testImage;

        public void AddTags(CustomVisionTrainingClient trainingApi)
        {
            beautyTag = trainingApi.CreateTag(projectId, "Beautiful");
            averageTag = trainingApi.CreateTag(projectId, "Average");
        }

        public CustomVisionPredictionClient AuthenticatePrediction()
        {
            CustomVisionPredictionClient predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = predictionEndpoint
            };
            return predictionApi;
        }

        public CustomVisionTrainingClient AuthenticateTraining()
        {
            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(
                new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = trainingEndpoint
            };
            return trainingApi;
        }

        public void LoadImagesFromDisk()
        {
            Beautiful = Directory.GetFiles(Path.Combine("StaticFiles", "Images", "Beautiful")).ToList();
            Average = Directory.GetFiles(Path.Combine("StaticFiles", "Images", "Average")).ToList();
        }

        public void UploadMainImage(CustomVisionTrainingClient trainingApi, string fullPath){
            var stream = new MemoryStream(System.IO.File.ReadAllBytes(fullPath));

            var tags = trainingApi.GetTags(projectId);
            trainingApi.CreateImagesFromData(projectId, stream, new List<Guid>() { tags[0].Id });

        }

        public void PublishIteration(CustomVisionTrainingClient trainingApi, Guid iterId)
        {
            var iters = trainingApi.GetIterations(projectId);
            if (iters.Count > 0){
                foreach (var iter in iters){
                    if (iter.PublishName != null){
                        trainingApi.UnpublishIteration(projectId, iter.Id);
                    }
                }
            }
            trainingApi.PublishIteration(projectId, iterId, publishedModelName, predictionResourceId);
            Console.WriteLine("Done!\n");   
        }

        public Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImagePrediction TestIteration(
            CustomVisionPredictionClient predictionApi
            , string fullPath)
        {
            Console.WriteLine("Making a prediction:");
            testImage = new MemoryStream(System.IO.File.ReadAllBytes(fullPath));
            Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImagePrediction 
                result = predictionApi.ClassifyImage(projectId, publishedModelName, testImage);

            // Loop over each prediction and write out the results
            foreach (var c in result.Predictions)
            {
                Console.WriteLine($"\t{c.TagName}: {c.Probability:P1}");
            }
            return result;
        }

        public Guid TrainProject(CustomVisionTrainingClient trainingApi)
        {
            Console.WriteLine("\tTraining");
            
            iteration = trainingApi.TrainProject(projectId);
           
            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Console.WriteLine("Waiting 10 seconds for training to complete...");
                Thread.Sleep(10000);

                // Re-query the iteration to get it's updated status
                iteration = trainingApi.GetIteration(projectId, iteration.Id);
            }
            return iteration.Id;
        }

        public void UploadImages(CustomVisionTrainingClient trainingApi)
        {
            Console.WriteLine("\tUploading images");
            LoadImagesFromDisk();

            // Images can be uploaded one at a time
            foreach (var image in Beautiful)
            {
                using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(image)))
                {
                    trainingApi.CreateImagesFromData(projectId, stream, new List<Guid>() { beautyTag.Id });
                }
            }

            // Or uploaded in a single batch 
            var imageFiles = Average.Select(img => 
                new ImageFileCreateEntry(Path.GetFileName(img), System.IO.File.ReadAllBytes(img)))
                .ToList();
            trainingApi.CreateImagesFromFiles(projectId, 
                new ImageFileCreateBatch(imageFiles, new List<Guid>() { averageTag.Id }));
        }
        
        public void CreateProject(CustomVisionTrainingClient trainingApi){
            AddTags(trainingApi);
            UploadImages(trainingApi);
            TrainProject(trainingApi);
            // PublishIteration(trainingApi);
        }

        public void DeleteImages(CustomVisionTrainingClient trainingApi){
            var tags = trainingApi.GetTags(projectId);
            
            var imgs = trainingApi.GetImages(projectId);
            
            Guid[] imgsId = new Guid[30];
            var count = 0;
            foreach (var img in imgs) {
                if (img.Tags[0].TagName == "Beautiful"){
                    imgsId[count] = img.Id;
                    count++;
                }
            }
            
            trainingApi.DeleteImages(projectId, imgsId);
        }
        
    }
}