using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web;

namespace Coursework.Models
{
    public class Cause
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Pledge { get; set; }
        public string Target { get; set; }
        public string ImageURL { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Member Member { get; set; }

        public virtual ICollection<Member> Signers { get; set; }

        public Cause(CauseVM causeVM)
        {
            Title = causeVM.Title;
            Description = causeVM.Description;
            Pledge = causeVM.Pledge;
            Target = causeVM.Target;
            ImageURL = causeVM.ImageURL;
            CreatedAt = causeVM.CreatedAt;
            Member = causeVM.Member;
            Signers = causeVM.Signers;
        }

        public Cause() {
            this.Signers = new HashSet<Member>();
        }
    }

    // Defining the DBContext inside one of the models is messy, but made the most sense in development
    // Would move it out into own class in production

    public class CauseDBContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Cause> Causes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cause>()
                .HasMany<Member>(s => s.Signers)
                .WithMany(c => c.Causes)
                .Map(cs =>
                {
                    cs.MapLeftKey("CauseID");
                    cs.MapRightKey("MemberID");
                    cs.ToTable("CauseMembers");
                });
        }
    }
}