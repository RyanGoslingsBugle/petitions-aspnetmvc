using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coursework.Models
{
    public class Cause
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Pledge { get; set; }
        public string Target { get; set; }
        public string ImageURL { get; set; }
        public DateTime CreatedAt { get; set; }

        public int MemberID { get; set; }
        [ForeignKey("MemberID")]
        public Member Member { get; set; }
    }

    public class CauseDBContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Cause> Causes { get; set; }
    }
}