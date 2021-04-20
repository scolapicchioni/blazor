using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.WebServices.REST.Photos.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase {
        private readonly IPhotosService service;

        public PhotosController(IPhotosService service) {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() => await service.GetPhotosAsync();

        [HttpGet("{id:int}", Name = "Find")]
        public async Task<ActionResult<Photo>> Find(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();
            return ph;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
            try {
                photo.UserName = User.Identity.Name;
                Photo p = await service.UploadAsync(photo);
                return CreatedAtRoute(nameof(Find), p, new { id = p.Id });
            } catch (UnauthorizedCreateAttemptException<Photo>) {
                return Forbid();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
            if (id != photo.Id)
                return BadRequest();
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();

            try {
                return await service.UpdateAsync(photo);
            } catch (UnauthorizedEditAttemptException<Photo>) {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Photo>> Remove(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();
            try {
                return await service.RemoveAsync(id);
            } catch (UnauthorizedDeleteAttemptException<Photo>) {
                return Forbid();
            }
        }
    }
}
