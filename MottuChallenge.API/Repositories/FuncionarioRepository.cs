using MottuChallenge.API.Data;
using MottuChallenge.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MottuChallenge.API.Repositories
{
    public class FuncionarioRepository
    {
        private readonly MottuDbContext _context;

        public FuncionarioRepository(MottuDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Funcionario> Funcionarios, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Funcionarios
                .Include(f => f.Patio)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var funcionarios = await query
                .OrderBy(f => f.UsuarioFuncionario)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (funcionarios, totalCount);
        }
         public async Task<(List<Funcionario> Funcionarios, int TotalCount)> GetByPatioPagedAsync(string nomePatio, int pageNumber, int pageSize)
        {
            var query = _context.Funcionarios
                .Where(f => f.NomePatio == nomePatio)
                .Include(f => f.Patio)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var funcionarios = await query
                .OrderBy(f => f.UsuarioFuncionario)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (funcionarios, totalCount);
        }
        public async Task<List<Funcionario>> GetAllAsync()
        {
            return await _context.Funcionarios
                .Include(f => f.Patio)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Funcionario?> GetByIdAsync(string usuarioFuncionario)
        {
            return await _context.Funcionarios
                .Include(f => f.Patio)
                .FirstOrDefaultAsync(f => f.UsuarioFuncionario == usuarioFuncionario);
        }

        public async Task AddAsync(Funcionario funcionario)
        {
            await _context.Funcionarios.AddAsync(funcionario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Funcionario funcionario)
        {
            _context.Funcionarios.Update(funcionario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Funcionario funcionario)
        {
            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();
        }

       public async Task<bool> ExistsAsync(string usuarioFuncionario)
{
    try
    {
        var result = await _context.Funcionarios
            .Where(f => f.UsuarioFuncionario == usuarioFuncionario)
            .Select(f => 1)
            .FirstOrDefaultAsync();
        
        return result == 1;
    }
    catch
    {
        return false;
    }
}

        public async Task<Funcionario?> GetByUsernameAsync(string usuario)
        {
            return await _context.Funcionarios
                .Include(f => f.Patio)
                .FirstOrDefaultAsync(f => f.UsuarioFuncionario == usuario);
        }

        public async Task<List<Funcionario>> GetByPatioAsync(string nomePatio)
        {
            return await _context.Funcionarios
                .Where(f => f.NomePatio == nomePatio)
                .Include(f => f.Patio)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> PertenceAoPatioAsync(string usuarioFuncionario, string nomePatio)
        {
            return await _context.Funcionarios
                .AnyAsync(f => f.UsuarioFuncionario == usuarioFuncionario && f.NomePatio == nomePatio);
        }
    }
}