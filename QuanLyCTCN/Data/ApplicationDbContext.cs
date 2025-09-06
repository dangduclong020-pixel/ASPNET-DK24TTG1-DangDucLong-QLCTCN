using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<ChiTieu> ChiTieus { get; set; }
        public DbSet<ThuNhap> ThuNhaps { get; set; }
        public DbSet<NganSach> NganSachs { get; set; }
        public DbSet<MucTieu> MucTieus { get; set; }
        public DbSet<NhacNho> NhacNhos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình mối quan hệ
            modelBuilder.Entity<ChiTieu>()
                .HasOne(c => c.NguoiDung)
                .WithMany(n => n.ChiTieus)
                .HasForeignKey(c => c.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTieu>()
                .HasOne(c => c.DanhMuc)
                .WithMany(d => d.ChiTieus)
                .HasForeignKey(c => c.DanhMucId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThuNhap>()
                .HasOne(t => t.NguoiDung)
                .WithMany(n => n.ThuNhaps)
                .HasForeignKey(t => t.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThuNhap>()
                .HasOne(t => t.DanhMuc)
                .WithMany(d => d.ThuNhaps)
                .HasForeignKey(t => t.DanhMucId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NganSach>()
                .HasOne(n => n.NguoiDung)
                .WithMany(nd => nd.NganSaches)
                .HasForeignKey(n => n.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NganSach>()
                .HasOne(n => n.DanhMuc)
                .WithMany(d => d.NganSaches)
                .HasForeignKey(n => n.DanhMucId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MucTieu>()
                .HasOne(m => m.NguoiDung)
                .WithMany(n => n.MucTieus)
                .HasForeignKey(m => m.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhacNho>()
                .HasOne(n => n.NguoiDung)
                .WithMany(nd => nd.NhacNhos)
                .HasForeignKey(n => n.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kiểm tra ràng buộc loại danh mục
            modelBuilder.Entity<DanhMuc>()
                .ToTable(tb => tb.HasCheckConstraint("CK_DanhMuc_Loai", "[loai] = N'ChiTieu' OR [loai] = N'ThuNhap'"));
        }
    }
}
