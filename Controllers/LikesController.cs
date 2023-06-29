using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly ILikesRepository _likeRepo;
        private readonly IUserRepository _userRepo;
        public LikesController(IUserRepository userRepo, ILikesRepository likeRepo)
        {
            _userRepo = userRepo;
            _likeRepo = likeRepo;

        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepo.GetUserByUsernameAsync(username);
            var sourceUser = await _likeRepo.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You can not like yourself");

            var userLike = await _likeRepo.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("you already like this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepo.SaveAllAsync()) return Ok();

            return BadRequest("Something went wrong while liking this person");

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await _likeRepo.getUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}