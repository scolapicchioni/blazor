using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
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
        try {
            Comment? c = await service.CreateAsync(comment);
            return CreatedAtRoute("FindComment", new { id = c.Id }, c);
        } catch (CreateUnauthorizedException<Comment>) {
            return Forbid();
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Comment>> Update(int id, Comment comment) {
        if (id != comment.Id)
            return BadRequest();
        try {
            Comment? c = await service.UpdateAsync(comment);
            if (c is null) return NotFound();
            return c;
        } catch (EditUnauthorizedException<Comment>) {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Comment>> Remove(int id) {
        try {
            Comment? cm = await service.RemoveAsync(id);
            if (cm is null) return NotFound();
            return cm;
        } catch (DeleteUnauthorizedException<Comment>) {
            return Forbid();
        }
    }
}
