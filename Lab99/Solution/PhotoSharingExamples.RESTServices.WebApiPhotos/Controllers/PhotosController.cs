using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Shared.Authorization;
using PhotoSharingExamples.Shared.Entities;

namespace PhotoSharingExamples.RESTServices.WebApiPhotos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly ILogger<PhotosController> _logger;
        private readonly IPhotosService photosService;
        private readonly IAuthorizationService authorizationService;

        public PhotosController(ILogger<PhotosController> logger, IPhotosService photosService, IAuthorizationService authorizationService)
        {
            _logger = logger;
            this.photosService = photosService;
            this.authorizationService = authorizationService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
            _logger.LogInformation("Create invoked");

            //user.Identity.Name works because:
            // 1) on the Identity Server Project Config I added the JwtClaimTypes.Name in the UserClaims of the "photos" ApiResource
            // 2) in this startup I added options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name; in the AddJwtBearer("Bearer", options =>{ ... })
            return await photosService.CreateAsync(photo.Title, photo.PhotoFile, photo.ImageMimeType, photo.Description, User.Identity.Name);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Photo>> Find(int id)
        {
            _logger.LogInformation($"Find invoked with Id {id}");
            Photo ph = await photosService.FindAsync(id);
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet("title/{title}")]
        public async Task<ActionResult<Photo>> FindByTitle(string title)
        {
            Photo ph = await photosService.FindByTitle(title);
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet("all/{number:int}")]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(int number)
        {
            return await photosService.GetPhotosAsync(number);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosByIds([FromQuery(Name = "ids")]int[] ids)
        {
            return await photosService.GetPhotosByIdsAsync(ids.ToList());
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Photo>> Remove(int id)
        {
            Photo ph = await photosService.FindAsync(id);
            var authorizationResult = await authorizationService.AuthorizeAsync(User, ph, Policies.EditDeletePhoto);

            if (authorizationResult.Succeeded)
            {
                return await photosService.RemoveAsync(id);
            }
            else
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<Photo>> Update(int id, Photo photo)
        {
            Photo ph = await photosService.FindAsync(id);
            var authorizationResult = await authorizationService.AuthorizeAsync(User, ph, Policies.EditDeletePhoto);

            if (authorizationResult.Succeeded)
            {
                return await photosService.UpdateAsync(id, photo.Title, photo.PhotoFile, photo.ImageMimeType, photo.Description);
            }
            else
            {
                return Forbid();
            }
        }
    }
}
