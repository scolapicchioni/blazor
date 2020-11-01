using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.WebServices.REST.Photos.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase {
        private readonly IPhotosService service;

        public PhotosController(IPhotosService service) {
            this.service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
            try {
                Photo p = await service.UploadAsync(photo);
                return CreatedAtRoute("Find", p, new { id = p.Id});
            } catch (UnauthorizedCreateAttemptException<Photo>) {
                return Forbid();
            }
        }

        [HttpGet("withimage/{id:int}", Name = "FindWithImage")]
        public async Task<ActionResult<Photo>> FindWithImage(int id) {
            Photo ph = await service.FindWithImageAsync(id);
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet("{id:int}", Name = "Find")]
        public async Task<ActionResult<Photo>> Find(int id) {
            Photo ph = await service.FindAsync(id);
            ph.ImageUrl = Url.Link(nameof(GetImage), new { id = ph.Id });
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet("image/{id:int}", Name = "GetImage")]
        public async Task<IActionResult> GetImage(int id) {
            PhotoImage ph = await service.GetImageAsync(id);
            if (ph == null) return NotFound();
            return File(ph.PhotoFile, ph.ImageMimeType);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() {
            List<Photo> photos = (await service.GetPhotosAsync()).ToList();
            photos.ForEach(p => p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id }));
            return photos;
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Photo>> Remove(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();

            try { 
                return await service.RemoveAsync(id);
            } catch(UnauthorizedDeleteAttemptException<Photo>) {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
            if (id != photo.Id)
                return BadRequest();
            Photo ph = await service.FindAsync(id);

            try { 
                return await service.UpdateAsync(photo);
            } catch(UnauthorizedEditAttemptException<Photo>) {
                return Forbid();
            }
        }
    }
}
