using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Server.Controllers;

[Route("[controller]")]
[ApiController]
public class CommentsController : ControllerBase {
    private readonly ICommentsService service;

    public CommentsController(ICommentsService service) {
        this.service = service;
    }
    [HttpGet("/photos/{photoId:int}/comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsForPhoto(int photoId) => await service.GetCommentsForPhotoAsync(photoId);

    [HttpGet("{id:int}", Name = "FindComment")]
    public async Task<ActionResult<Comment>> Find(int id) {
        Comment? cm = await service.FindAsync(id);
        if (cm is null) return NotFound();
        return cm;
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> CreateAsync(Comment comment) {
        Comment? c = await service.CreateAsync(comment);
        return CreatedAtRoute("FindComment", new { id = c.Id }, c);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Comment>> Update(int id, Comment comment) {
        if (id != comment.Id)
            return BadRequest();
        return await service.UpdateAsync(comment);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Comment>> Remove(int id) {
        Comment? cm = await service.FindAsync(id);
        if (cm is null) return NotFound();
        return await service.RemoveAsync(id);
    }
}
