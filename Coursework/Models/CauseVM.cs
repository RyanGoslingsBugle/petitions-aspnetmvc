using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Coursework.Models
{
    public class CauseVM
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required]
        public string Pledge { get; set; }
        [Required]
        public string Target { get; set; }
        public string ImageURL { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public virtual Member Member { get; set; }

        public virtual ICollection<Member> Signers { get; set; }

        [AttachmentNotRequired]
        public HttpPostedFileBase Image { get; set; }

        public CauseVM(Cause cause)
        {
            ID = cause.ID;
            Title = cause.Title;
            Description = cause.Description;
            Pledge = cause.Pledge;
            Target = cause.Target;
            ImageURL = cause.ImageURL;
            CreatedAt = cause.CreatedAt;
            Member = cause.Member;
            Signers = cause.Signers;
        }

        public CauseVM(Member member)
        {
            this.Member = member;
        }

        public CauseVM() {
            this.Signers = new HashSet<Member>();
        }
    }
}