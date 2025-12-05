using Microsoft.EntityFrameworkCore;
using VendasService.Models;
namespace VendasService.Data
{
    public class VendasContext : DbContext
    {
        public VendasContext(DbContextOptions<VendasContext> options)
        : base(options)
        {
        }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClienteNome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ClienteEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ValorTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasConversion<string>();

                entity.HasMany(e => e.Itens)
                .WithOne(i => i.Pedido)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ItemPedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProdutoNome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            });
        }
    }
}