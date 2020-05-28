using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;

namespace PhotoSharingApplication.WebServices.REST.Photos.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase {
        private readonly IPhotosService service;
        private readonly IAuthorizationService authorizationService;

        public PhotosController(IPhotosService service, IAuthorizationService authorizationService) {
            this.service = service;
            this.authorizationService = authorizationService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
            photo.UserName = User.Identity.Name;
            Photo p = await service.UploadAsync(photo);
            return CreatedAtRoute("Find", p, new { id = p.Id});
        }

        [HttpGet("{id:int}", Name = "Find")]
        public async Task<ActionResult<Photo>> Find(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() => await service.GetPhotosAsync();

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Photo>> Remove(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();
            
            var authorizationResult = await authorizationService.AuthorizeAsync(User, ph, Policies.EditDeletePhoto);

            if (authorizationResult.Succeeded) {
                return await service.RemoveAsync(id);
            } else {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
            if (id != photo.Id)
                return BadRequest();
            Photo ph = await service.FindAsync(id);
            var authorizationResult = await authorizationService.AuthorizeAsync(User, ph, Policies.EditDeletePhoto);

            if (authorizationResult.Succeeded) {
                return await service.UpdateAsync(photo);
            } else {
                return Forbid();
            }
        }
    }
}
