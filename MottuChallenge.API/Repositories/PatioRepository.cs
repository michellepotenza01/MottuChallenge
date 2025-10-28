using MottuChallenge.API.Data;
using MottuChallenge.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MottuChallenge.API.Repositories
{
    public class PatioRepository
    {
        private readonly MottuDbContext _context;

        public PatioRepository(MottuDbContext context)
        {
            _context = context;
        }
        public async Task<(List<Patio> Patios, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Patios
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var patios = await query
                .OrderBy(p => p.NomePatio)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (patios, totalCount);
        }

        public async Task<(List<Patio> Patios, int TotalCount)> GetPatiosComVagasDisponiveisPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Patios
                .Where(p => p.VagasTotais - p.VagasOcupadas > 0)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var patios = await query
                .OrderBy(p => p.NomePatio)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (patios, totalCount);
        }
        public async Task<List<Patio>> GetAllAsync()
        {
            return await _context.Patios
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Patio?> GetByIdAsync(string nomePatio)
        {
            return await _context.Patios
                .FirstOrDefaultAsync(p => p.NomePatio == nomePatio);
        }

        public async Task AddAsync(Patio patio)
        {
            await _context.Patios.AddAsync(patio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Patio patio)
        {
            _context.Patios.Update(patio);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Patio patio)
        {
            _context.Patios.Remove(patio);
            await _context.SaveChangesAsync();
        }

       public async Task<bool> ExistsAsync(string nomePatio)
{
    try
    {
        
        var result = await _context.Patios
            .Where(p => p.NomePatio == nomePatio)
            .Select(p => 1)
            .FirstOrDefaultAsync();
        
        return result == 1;
    }
    catch
    {
        return false;
    }
}

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Patios.CountAsync();
        }

        public async Task<List<Patio>> GetPatiosComVagasDisponiveisAsync()
        {
            return await _context.Patios
                .Where(p => p.VagasTotais - p.VagasOcupadas > 0)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AtualizarVagasAsync(string nomePatio, int alteracaoVagas)
        {
            var patio = await GetByIdAsync(nomePatio);
            if (patio is null) return false;

            var novasVagasOcupadas = patio.VagasOcupadas + alteracaoVagas;
            if (novasVagasOcupadas < 0 || novasVagasOcupadas > patio.VagasTotais)
                return false;

            patio.VagasOcupadas = novasVagasOcupadas;
            await UpdateAsync(patio);
            return true;
        }
    }
}