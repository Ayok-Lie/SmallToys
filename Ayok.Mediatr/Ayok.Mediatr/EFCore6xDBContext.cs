using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Ayok.Mediatr
{
    public partial class EFCore6xDBContext : BaseDbContext
    {
        //public EFCore6xDBContext()
        //{
        //}
        //public EFCore6xDBContext(DbContextOptions<EFCore6xDBContext> options)
        //    : base(options)
        //{
        //}
        public EFCore6xDBContext(DbContextOptions<EFCore6xDBContext> options, IMediator mediator) : base(options, mediator)
        {
        }
        public virtual DbSet<GoodsInfo> GoodsInfo { get; set; } = null!;
        public virtual DbSet<RoleInfo> RoleInfo { get; set; } = null!;
        public virtual DbSet<UserInfo> UserInfo { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoodsInfo>(entity =>
            {
                entity.Property(e => e.rowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });
            modelBuilder.Entity<RoleInfo>(entity =>
            {
                entity.Property(e => e.id).ValueGeneratedNever();
            });
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
