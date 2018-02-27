using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Coursework.Models
{
    public class MemberVM
    {
        [Required]
        public int ID { get; set; }
        [Required]
        [Display(Name = "User Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ImagePath { get; set; }

        public ICollection<Cause> Causes { get; set; }

        public HttpPostedFileBase Image { get; set; }

        public MemberVM (Member member)
        {
            Name = member.Name;
            Email = member.Email;
            City = member.City;
            Country = member.Country;
            ImagePath = member.ImagePath;
            Causes = member.Causes;
        }

        public MemberVM()
        {
        }
    }
}