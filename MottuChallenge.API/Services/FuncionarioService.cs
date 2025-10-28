using MottuChallenge.API.Models;
using MottuChallenge.API.DTOs;
using MottuChallenge.API.Repositories;
using MottuChallenge.API.Models.Common;

namespace MottuChallenge.API.Services
{
    public class FuncionarioService
    {
        private readonly FuncionarioRepository _funcionarioRepository;
        private readonly PatioRepository _patioRepository;
        private readonly MotoRepository _motoRepository;
        private readonly AuthService _authService;

        public FuncionarioService(FuncionarioRepository funcionarioRepository, PatioRepository patioRepository, MotoRepository motoRepository, AuthService authService)
        {
            _funcionarioRepository = funcionarioRepository;
            _patioRepository = patioRepository;
            _motoRepository = motoRepository;
            _authService = authService;
        }

        public async Task<ServiceResponse<PagedResponse<Funcionario>>> GetFuncionariosPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var result = await _funcionarioRepository.GetAllPagedAsync(paginationParams.PageNumber, paginationParams.PageSize);
                var pagedResponse = new PagedResponse<Funcionario>(result.Funcionarios, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Funcionario>>.Ok(pagedResponse, "Funcionários recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Funcionario>>.Error($"Erro ao buscar funcionários: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<Funcionario>>> GetFuncionariosPorPatioPagedAsync(string nomePatio, PaginationParams paginationParams)
        {
            try
            {
                var result = await _funcionarioRepository.GetByPatioPagedAsync(nomePatio, paginationParams.PageNumber, paginationParams.PageSize);
                var pagedResponse = new PagedResponse<Funcionario>(result.Funcionarios, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Funcionario>>.Ok(pagedResponse, $"Funcionários do pátio {nomePatio} recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Funcionario>>.Error($"Erro ao buscar funcionários do pátio: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Funcionario>>> GetFuncionariosAsync()
        {
            try
            {
                var funcionarios = await _funcionarioRepository.GetAllAsync();
                return ServiceResponse<List<Funcionario>>.Ok(funcionarios, "Funcionários recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Funcionario>>.Error($"Erro ao buscar funcionários: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Funcionario>> GetFuncionarioByIdAsync(string usuarioFuncionario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioFuncionario))
                    return ServiceResponse<Funcionario>.Error("Usuário do funcionário é obrigatório");

                var funcionario = await _funcionarioRepository.GetByIdAsync(usuarioFuncionario);
                return funcionario is null 
                    ? ServiceResponse<Funcionario>.NotFound("Funcionário")
                    : ServiceResponse<Funcionario>.Ok(funcionario, "Funcionário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Funcionario>.Error($"Erro ao buscar funcionário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Funcionario>> CreateFuncionarioAsync(FuncionarioDto funcionarioDto)
        {
            try
            {
                if (await _funcionarioRepository.ExistsAsync(funcionarioDto.UsuarioFuncionario))
                    return ServiceResponse<Funcionario>.Error("Usuário já cadastrado");

                var patio = await _patioRepository.GetByIdAsync(funcionarioDto.NomePatio);
                if (patio is null)
                    return ServiceResponse<Funcionario>.NotFound("Pátio");

                var funcionario = new Funcionario
                {
                    UsuarioFuncionario = funcionarioDto.UsuarioFuncionario.Trim(),
                    Nome = funcionarioDto.Nome.Trim(),
                    SenhaHash = _authService.HashPassword(funcionarioDto.Senha),
                    NomePatio = funcionarioDto.NomePatio.Trim(),
                    Role = (funcionarioDto.Role ?? "Funcionario").Trim()
                };

                await _funcionarioRepository.AddAsync(funcionario);
                return ServiceResponse<Funcionario>.Ok(funcionario, "Funcionário criado com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Funcionario>.Error($"Erro ao criar funcionário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Funcionario>> UpdateFuncionarioAsync(string usuarioFuncionario, FuncionarioDto funcionarioDto)
        {
            try
            {
                var funcionarioExistente = await _funcionarioRepository.GetByIdAsync(usuarioFuncionario);
                if (funcionarioExistente is null)
                    return ServiceResponse<Funcionario>.NotFound("Funcionário");

                var patio = await _patioRepository.GetByIdAsync(funcionarioDto.NomePatio);
                if (patio is null)
                    return ServiceResponse<Funcionario>.NotFound("Pátio");

                funcionarioExistente.Nome = funcionarioDto.Nome.Trim();
                
                if (!string.IsNullOrEmpty(funcionarioDto.Senha))
                    funcionarioExistente.SenhaHash = _authService.HashPassword(funcionarioDto.Senha);
                
                funcionarioExistente.NomePatio = funcionarioDto.NomePatio.Trim();
                funcionarioExistente.Role = (funcionarioDto.Role ?? funcionarioExistente.Role).Trim();

                await _funcionarioRepository.UpdateAsync(funcionarioExistente);
                return ServiceResponse<Funcionario>.Ok(funcionarioExistente, "Funcionário atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Funcionario>.Error($"Erro ao atualizar funcionário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteFuncionarioAsync(string usuarioFuncionario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioFuncionario))
                    return ServiceResponse<bool>.Error("Usuário do funcionário é obrigatório");

                var funcionario = await _funcionarioRepository.GetByIdAsync(usuarioFuncionario);
                if (funcionario is null)
                    return ServiceResponse<bool>.NotFound("Funcionário");

                var motosDoFuncionario = await _motoRepository.GetByFuncionarioAsync(usuarioFuncionario);
                if (motosDoFuncionario.Any())
                    return ServiceResponse<bool>.Error("Não é possível excluir funcionário com motos associadas");

                await _funcionarioRepository.DeleteAsync(funcionario);
                return ServiceResponse<bool>.Ok(true, "Funcionário excluído com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Error($"Erro ao excluir funcionário: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Funcionario>>> GetFuncionariosPorPatioAsync(string nomePatio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomePatio))
                    return ServiceResponse<List<Funcionario>>.Error("Nome do pátio é obrigatório");

                var funcionarios = await _funcionarioRepository.GetByPatioAsync(nomePatio);
                return ServiceResponse<List<Funcionario>>.Ok(funcionarios, "Funcionários do pátio recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Funcionario>>.Error($"Erro ao buscar funcionários do pátio: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> VerificarFuncionarioNoPatioAsync(string usuarioFuncionario, string nomePatio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioFuncionario) || string.IsNullOrWhiteSpace(nomePatio))
                    return ServiceResponse<bool>.Error("Usuário e pátio são obrigatórios");

                var pertence = await _funcionarioRepository.PertenceAoPatioAsync(usuarioFuncionario, nomePatio);
                return ServiceResponse<bool>.Ok(pertence, "Verificação de pátio realizada");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Error($"Erro ao verificar pátio do funcionário: {ex.Message}");
            }
        }
    }
}