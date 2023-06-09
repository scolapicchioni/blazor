using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.REST.Photos.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PhotosController : ControllerBase {
    private readonly IPhotosService service;

    public PhotosController(IPhotosService service) => this.service = service;
    [HttpGet("{startIndex}/{amount}")]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(int startIndex, int amount, CancellationToken cancellationToken) => 
        (await service
        .GetPhotosAsync(startIndex, amount, cancellationToken))
        .Select(p => new Photo { 
            Id = p.Id, 
            CreatedDate = p.CreatedDate, 
            Description = p.Description, 
            PhotoImage = p.PhotoImage, 
            Title = p.Title, 
            UserName = p.UserName, 
            ImageUrl = p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id }) 
        }).ToList();

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetPhotosCount() => await service.GetPhotosCountAsync();

    [HttpGet("{id:int}", Name = "Find")]
    public async Task<ActionResult<Photo>> Find(int id) {
        Photo? ph = await service.FindAsync(id);
        if (ph is null) return NotFound();
        ph.ImageUrl = Url.Link(nameof(GetImage), new { id = ph.Id });
        return ph;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
        try {
            photo.UserName = User?.Identity?.Name;
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

    [HttpGet("withimage/{id:int}", Name = "FindWithImage")]
    public async Task<ActionResult<Photo>> FindWithImage(int id) {
        Photo? ph = await service.FindWithImageAsync(id);
        if (ph is null) return NotFound();
        return ph;
    }

    [HttpGet("image/{id:int}", Name = "GetImage")]
    public async Task<IActionResult> GetImage(int id) {
        PhotoImage? ph = await service.GetImageAsync(id);
        if (ph is null || ph.PhotoFile is null || ph.ImageMimeType is null) return NotFound();
        return File(ph.PhotoFile, ph.ImageMimeType);
    }
}
