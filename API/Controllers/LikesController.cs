using System.Threading.Tasks;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.Extensions;
using API.Entity;
using System.Collections.Generic;
using API.DTOs;
using API.Data.Helpers;
using System;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseAPIController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMLService _mLService;
        private CustomVisionTrainingClient trainingApi;
        private CustomVisionPredictionClient predictionApi;

        public LikesController(IUnitOfWork unitOfWork
            , IMLService mLService)
        {
            _mLService = mLService;
            _unitOfWork = unitOfWork;

            trainingApi =  _mLService.AuthenticateTraining();
            predictionApi = _mLService.AuthenticatePrediction();
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");
            var userLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest("You already like this user");
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            // Train LikedUser Photo Section
            // Deleted all images in workspace
            if (sourceUser.nLike == 0) _mLService.DeleteImages(trainingApi);
            sourceUser.nLike++;
            var mainPhoto = _unitOfWork.LikesRepository.GetMainPhoto(username);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), mainPhoto.Url);
            _mLService.UploadMainImage(trainingApi, fullPath);


            if (sourceUser.nLike >= 5){
                var iterId = _mLService.TrainProject(trainingApi);
                _mLService.PublishIteration(trainingApi, iterId);
                sourceUser.nLike = 0;
            }

            _unitOfWork.UserRepository.Update(sourceUser);
            // End of TrainImage
            sourceUser.LikedUsers.Add(userLike);

            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("Failed to like user");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }
    }
}