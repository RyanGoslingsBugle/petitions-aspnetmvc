using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Coursework.Models
{
    public class Member
    {
        [Required]
        public int ID { get; set; }
        [Required]
        [Display(Name = "User Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string City { get; set; }
        public string Country { get; set; }
        public string Avatar { get; set; }
        [Required]
        public Role Role { get; set; }

        public ICollection<Cause> Causes { get; set; }
    }

    // Enums in model from CodeProject, Shahriar Hussain, 23/05/14, https://www.codeproject.com/Articles/776908/Dealing-with-Enum-in-MVC

    public enum Role
    {
        Admin,
        Member
    }
}