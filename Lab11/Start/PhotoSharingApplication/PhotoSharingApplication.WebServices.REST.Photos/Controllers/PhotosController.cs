using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.Rest.Photos.Controllers;

[Route("[controller]")]
[ApiController]
public class PhotosController : ControllerBase {
    private readonly IPhotosService service;

    public PhotosController(IPhotosService service) => this.service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() => await service.GetPhotosAsync();

    [HttpGet("{id:int}", Name = "Find")]
    public async Task<ActionResult<Photo>> Find(int id) {
        Photo? ph = await service.FindAsync(id);
        if (ph is null) return NotFound();
        return ph;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
        photo.UserName = User?.Identity?.Name;
        Photo? p = await service.UploadAsync(photo);
        return CreatedAtRoute("Find", new { id = p.Id }, p);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
        if (id != photo.Id)
            return BadRequest();
        return await service.UpdateAsync(photo);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Photo>> Remove(int id) {
        Photo ph = await service.FindAsync(id);
        if (ph == null) return NotFound();
        return await service.RemoveAsync(id);
    }
}
