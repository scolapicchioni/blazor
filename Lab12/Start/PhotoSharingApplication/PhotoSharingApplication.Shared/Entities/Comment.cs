namespace PhotoSharingApplication.Shared.Entities;

public class Comment {
    public int Id { get; set; }
    public int PhotoId { get; set; }
    public string UserName { get; set; } = String.Empty;
    public string Subject { get; set; } = String.Empty;
    public string Body { get; set; } = String.Empty;
    public DateTime SubmittedOn { get; set; }
}
