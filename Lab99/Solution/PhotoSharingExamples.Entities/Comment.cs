using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;

namespace PhotoSharingExamples.Shared.Entities
{
    public class Comment : BaseEntity
    {
        public int PhotoId { get; set; }
        public string UserName { get; set; }
        //[Required]
        //[StringLength(250)]
        public string Subject { get; set; }
        //[DataType(DataType.MultilineText)]
        public string Body { get; set; }
        public DateTime SubmittedOn { get; set; }
        public Photo Photo { get; set; }
        //[NotMapped]
        //public bool UserMayDelete { get; private set; }

        public Comment()
        {
        }

        public Comment(int photoId, string userName, string subject, string body, DateTime submittedOn)
        {
            Guard.Against.NullOrEmpty(subject, nameof(subject));
            Guard.Against.Null(photoId, nameof(photoId));
            Guard.Against.OutOfSQLDateRange(submittedOn, nameof(submittedOn));
            PhotoId = photoId;
            UserName = userName;
            Subject = subject;
            Body = body;
            SubmittedOn = submittedOn;
        }
        public Comment(int id, int photoId, string userName, string subject, string body, DateTime submittedOn) : this(photoId,userName,subject,body,submittedOn)
        {
            Id = id;
        }
    }
}
