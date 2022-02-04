using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Server.Controllers;

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

    [HttpPost]
    public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
        try {
            Photo? p = await service.UploadAsync(photo);
            return CreatedAtRoute("Find", new { id = photo.Id }, p);
        } catch (CreateUnauthorizedException<Photo>) {
            return Forbid();
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
        if (id != photo.Id)
            return BadRequest();
        try {
            Photo? p = await service.UpdateAsync(photo);
            if (p is null) return NotFound();
            return p;
        } catch (EditUnauthorizedException<Photo>) {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Photo>> Remove(int id) {
        try {
            Photo? ph = await service.RemoveAsync(id);
            if (ph is null) return NotFound();
            return ph;
        } catch (DeleteUnauthorizedException<Photo>) {
            return Forbid();
        }
    }
}
