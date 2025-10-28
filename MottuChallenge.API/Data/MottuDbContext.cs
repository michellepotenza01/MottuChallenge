using Microsoft.EntityFrameworkCore;
using MottuChallenge.API.Models;

namespace MottuChallenge.API.Data
{
    public class MottuDbContext : DbContext
    {
        public MottuDbContext(DbContextOptions<MottuDbContext> options) : base(options) { }

        public DbSet<Moto> Motos { get; set; }
        public DbSet<Patio> Patios { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

             foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(bool))
                    {
                        property.SetColumnType("NUMBER(1)");
                    }
                }
            }

             modelBuilder.Entity<Patio>(entity =>
            {
                entity.HasKey(p => p.NomePatio);
                entity.Property(p => p.NomePatio).HasMaxLength(50);
                entity.Property(p => p.Localizacao).HasMaxLength(200);
            });

            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.HasKey(f => f.UsuarioFuncionario);
                entity.Property(f => f.UsuarioFuncionario).HasMaxLength(50);
                entity.Property(f => f.Nome).HasMaxLength(100);
                entity.Property(f => f.SenhaHash).HasMaxLength(256);
                entity.Property(f => f.NomePatio).HasMaxLength(50);
                entity.Property(f => f.Role).HasMaxLength(20);
                
                entity.HasOne(f => f.Patio)
                    .WithMany(p => p.Funcionarios)
                    .HasForeignKey(f => f.NomePatio);
            });

            modelBuilder.Entity<Moto>(entity =>
            {
                entity.HasKey(m => m.Placa);
                entity.Property(m => m.Placa).HasMaxLength(8);
                entity.Property(m => m.NomePatio).HasMaxLength(50);
                entity.Property(m => m.UsuarioFuncionario).HasMaxLength(50);
                
                entity.HasOne(m => m.Patio)
                    .WithMany(p => p.Motos)
                    .HasForeignKey(m => m.NomePatio);

                entity.HasOne(m => m.Funcionario)
                    .WithMany(f => f.Motos)
                    .HasForeignKey(m => m.UsuarioFuncionario);
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(c => c.UsuarioCliente);
                entity.Property(c => c.UsuarioCliente).HasMaxLength(50);
                entity.Property(c => c.Nome).HasMaxLength(100);
                entity.Property(c => c.SenhaHash).HasMaxLength(256);
                entity.Property(c => c.MotoPlaca).HasMaxLength(8);
                
                entity.HasOne(c => c.Moto)
                    .WithMany()
                    .HasForeignKey(c => c.MotoPlaca);
            });
        }
    }
}