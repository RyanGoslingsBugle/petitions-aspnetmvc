using System;
using System.Data.Entity;

namespace Coursework.Models
{
    public class Member : User
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string Avatar { get; set; }
    }
}