namespace PhotoSharingApplication.Shared.Entities;
public class Photo {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UserName { get; set; }
    public string? ImageUrl { get; set; }
    public PhotoImage? PhotoImage { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
