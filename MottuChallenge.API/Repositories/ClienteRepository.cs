using MottuChallenge.API.Data;
using MottuChallenge.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MottuChallenge.API.Repositories
{
    public class ClienteRepository
    {
        private readonly MottuDbContext _context;

        public ClienteRepository(MottuDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Cliente> Clientes, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Clientes
                .Include(c => c.Moto)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var clientes = await query
                .OrderBy(c => c.UsuarioCliente)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (clientes, totalCount);
        }

        public async Task<List<Cliente>> GetAllAsync()
        {
            return await _context.Clientes
                .Include(c => c.Moto)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Cliente?> GetByIdAsync(string usuarioCliente)
        {
            return await _context.Clientes
                .Include(c => c.Moto)
                .FirstOrDefaultAsync(c => c.UsuarioCliente == usuarioCliente);
        }

        public async Task AddAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Cliente cliente)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string usuarioCliente)
{
    try
    {
        var result = await _context.Clientes
            .Where(c => c.UsuarioCliente == usuarioCliente)
            .Select(c => 1)
            .FirstOrDefaultAsync();
        
        return result == 1;
    }
    catch
    {
        return false;
    }
}

        public async Task<Cliente?> GetByMotoPlacaAsync(string motoPlaca)
        {
            return await _context.Clientes
                .Include(c => c.Moto)
                .FirstOrDefaultAsync(c => c.MotoPlaca == motoPlaca);
        }

        public async Task<bool> RegistrarManutencaoAsync(string usuarioCliente)
        {
            var cliente = await GetByIdAsync(usuarioCliente);
            if (cliente is null) return false;

            cliente.RegistrarManutencao();
            await UpdateAsync(cliente);
            return true;
        }
    }
}