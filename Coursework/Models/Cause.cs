using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Coursework.Models
{
    public class Cause
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Pledge { get; set; }
        [Required]
        public string Target { get; set; }
        public string ImageURL { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public Member Member { get; set; }
    }

    public class CauseDBContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Cause> Causes { get; set; }
    }
}