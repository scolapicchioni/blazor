using System;
using System.Collections.Generic;

namespace PhotoSharingApplication.Shared.Core.Entities {
    public class Photo {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public PhotoImage PhotoImage { get; set; }
    }
}
