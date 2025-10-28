using MottuChallenge.API.Data;
using MottuChallenge.API.Models;
using Microsoft.EntityFrameworkCore;
using MottuChallenge.API.Enums;

namespace MottuChallenge.API.Repositories
{
    public class MotoRepository
    {
        private readonly MottuDbContext _context;

        public MotoRepository(MottuDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Moto> Motos, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Motos
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var motos = await query
                .OrderBy(m => m.Placa)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (motos, totalCount);
        }

        public async Task<(List<Moto> Motos, int TotalCount)> GetByStatusPagedAsync(StatusMoto status, int pageNumber, int pageSize)
        {
            var query = _context.Motos
                .Where(m => m.Status == status)
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var motos = await query
                .OrderBy(m => m.Placa)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (motos, totalCount);
        }

        public async Task<(List<Moto> Motos, int TotalCount)> GetByPatioPagedAsync(string nomePatio, int pageNumber, int pageSize)
        {
            var query = _context.Motos
                .Where(m => m.NomePatio == nomePatio)
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var motos = await query
                .OrderBy(m => m.Placa)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (motos, totalCount);
        }

        public async Task<List<Moto>> GetAllAsync()
        {
            return await _context.Motos
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Moto>> GetByFuncionarioAsync(string usuarioFuncionario)
        {
            return await _context.Motos
                .Where(m => m.UsuarioFuncionario == usuarioFuncionario)
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Moto?> GetByIdAsync(string placa)
        {
            return await _context.Motos
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .FirstOrDefaultAsync(m => m.Placa == placa);
        }

        public async Task AddAsync(Moto moto)
        {
            await _context.Motos.AddAsync(moto);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Moto moto)
        {
            _context.Motos.Update(moto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Moto moto)
        {
            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();
        }

       public async Task<bool> ExistsAsync(string placa)
{
    try
    {
        
        var result = await _context.Motos
            .Where(m => m.Placa == placa)
            .Select(m => 1)
            .FirstOrDefaultAsync();
        
        return result == 1;
    }
    catch
    {
        return false;
    }
}

        public async Task<List<Moto>> GetByPatioAsync(string nomePatio)
        {
            return await _context.Motos
                .Where(m => m.NomePatio == nomePatio)
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Moto>> GetByStatusAsync(StatusMoto status)
        {
            return await _context.Motos
                .Where(m => m.Status == status)
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Moto>> GetMotosPrecisandoManutencaoAsync()
        {
            return await _context.Motos
                .Where(m => m.PrecisaManutencao == 1)
                .Include(m => m.Patio)
                .Include(m => m.Funcionario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AtualizarStatusManutencaoAsync(string placa, bool precisaManutencao, float probabilidade)
        {
            var moto = await GetByIdAsync(placa);
            if (moto is null) return false;

            moto.PrecisaManutencao = precisaManutencao ? 1 : 0;
            moto.ProbabilidadeManutencao = probabilidade;
            moto.DataAtualizacao = DateTime.Now;
            
            await UpdateAsync(moto);
            return true;
        }
    }
}