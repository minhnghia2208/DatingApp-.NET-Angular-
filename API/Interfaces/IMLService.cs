using System;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;

namespace API.Interfaces
{
    public interface IMLService
    {
        CustomVisionTrainingClient AuthenticateTraining();
        CustomVisionPredictionClient AuthenticatePrediction();
        void AddTags(CustomVisionTrainingClient trainingApi);
        void UploadImages(CustomVisionTrainingClient trainingApi);
        void TrainProject(CustomVisionTrainingClient trainingApi);
        void PublishIteration(CustomVisionTrainingClient trainingApi);
        Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImagePrediction TestIteration(CustomVisionPredictionClient predictionApi, string fullPath);
        void LoadImagesFromDisk();
        void UploadMainImage(CustomVisionTrainingClient trainingApi, string fullPath);
        void CreateProject(CustomVisionTrainingClient trainingApi);
        void DeleteImages(CustomVisionTrainingClient trainingApi);
    }
}