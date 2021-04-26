using System;

namespace PhotoSharingApplication.Shared.Core.Entities {
    public class Comment {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public string UserName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SubmittedOn { get; set; }
        public Photo Photo { get; set; }
    }
}