﻿namespace PhotoSharingApplication.Frontend.Core.Entities;

public class Photo {
    public int Id { get; set; }
    public string Title { get; set; } = String.Empty;
    public byte[]? PhotoFile { get; set; }
    public string? ImageMimeType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UserName { get; set; }
}
