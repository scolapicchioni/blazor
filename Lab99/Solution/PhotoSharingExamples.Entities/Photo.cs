using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;

namespace PhotoSharingExamples.Shared.Entities
{
    public class Photo : BaseEntity
    {

        //[Required]
        public string Title { get; set; }
        //[DisplayName("Picture")]
        public byte[] PhotoFile { get; set; }
        public string ImageMimeType { get; set; }
        //[DataType(DataType.MultilineText)]
        public string Description { get; set; }
        //[DataType(DataType.DateTime), DisplayFormat(DataFormatString ="{0:dd/MM/yy}", ApplyFormatInEditMode =true)]
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }

        private readonly List<Comment> _comments = new List<Comment>();
        public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

        public Photo()
        {

        }

        public Photo(string title, byte[] photoFile, string imageMimeType, string description, DateTime createdDate, string userName, List<Comment> comments)
        {
            Guard.Against.NullOrEmpty(title, nameof(title));
            //Guard.Against.Null(comments, nameof(comments));
            Title = title;
            PhotoFile = photoFile;
            ImageMimeType = imageMimeType;
            Description = description;
            CreatedDate = createdDate;
            UserName = userName;
            if (comments != null) _comments.AddRange(comments);
        }

        public Photo(int id, string title, byte[] photoFile, string imageMimeType, string description, DateTime createdDate, string userName, List<Comment> comments) : this(title,photoFile,imageMimeType,description,createdDate,userName,comments)
        {
            Id = id;
        }

    }
}
