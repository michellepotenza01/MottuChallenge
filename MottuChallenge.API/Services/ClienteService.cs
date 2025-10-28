using MottuChallenge.API.Models;
using MottuChallenge.API.DTOs;
using MottuChallenge.API.Repositories;
using MottuChallenge.API.Models.Common;

namespace MottuChallenge.API.Services
{
    public class ClienteService
    {
        private readonly ClienteRepository _clienteRepository;
        private readonly MotoRepository _motoRepository;
        private readonly AuthService _authService;

        public ClienteService(ClienteRepository clienteRepository, MotoRepository motoRepository, AuthService authService)
        {
            _clienteRepository = clienteRepository;
            _motoRepository = motoRepository;
            _authService = authService;
        }

        public async Task<ServiceResponse<PagedResponse<Cliente>>> GetClientesPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var result = await _clienteRepository.GetAllPagedAsync(paginationParams.PageNumber, paginationParams.PageSize);
                var pagedResponse = new PagedResponse<Cliente>(result.Clientes, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Cliente>>.Ok(pagedResponse, "Clientes recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Cliente>>.Error($"Erro ao buscar clientes: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Cliente>>> GetClientesAsync()
        {
            try
            {
                var clientes = await _clienteRepository.GetAllAsync();
                return ServiceResponse<List<Cliente>>.Ok(clientes, "Clientes recuperados com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Cliente>>.Error($"Erro ao buscar clientes: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Cliente>> GetClienteByIdAsync(string usuarioCliente)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioCliente))
                    return ServiceResponse<Cliente>.Error("Usuário do cliente é obrigatório");

                var cliente = await _clienteRepository.GetByIdAsync(usuarioCliente);
                return cliente is null 
                    ? ServiceResponse<Cliente>.NotFound("Cliente")
                    : ServiceResponse<Cliente>.Ok(cliente, "Cliente encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Cliente>.Error($"Erro ao buscar cliente: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Cliente>> CreateClienteAsync(ClienteDto clienteDto)
        {
            try
            {
                if (await _clienteRepository.ExistsAsync(clienteDto.UsuarioCliente))
                    return ServiceResponse<Cliente>.Error("Usuário já cadastrado");

                if (!string.IsNullOrEmpty(clienteDto.MotoPlaca))
                {
                    var moto = await _motoRepository.GetByIdAsync(clienteDto.MotoPlaca);
                    if (moto is null)
                        return ServiceResponse<Cliente>.NotFound("Moto");

                    var motoAssociada = await _clienteRepository.GetByMotoPlacaAsync(clienteDto.MotoPlaca);
                    if (motoAssociada is not null)
                        return ServiceResponse<Cliente>.Error("Moto já está associada a outro cliente");
                }

                var cliente = new Cliente
                {
                    UsuarioCliente = clienteDto.UsuarioCliente.Trim(),
                    Nome = clienteDto.Nome.Trim(),
                    SenhaHash = _authService.HashPassword(clienteDto.Senha),
                    MotoPlaca = clienteDto.MotoPlaca?.Trim(),
                    DataCriacao = DateTime.Now,
                    DataAtualizacao = DateTime.Now
                };

                await _clienteRepository.AddAsync(cliente);
                return ServiceResponse<Cliente>.Ok(cliente, "Cliente criado com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Cliente>.Error($"Erro ao criar cliente: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Cliente>> UpdateClienteAsync(string usuarioCliente, ClienteDto clienteDto)
        {
            try
            {
                var clienteExistente = await _clienteRepository.GetByIdAsync(usuarioCliente);
                if (clienteExistente is null)
                    return ServiceResponse<Cliente>.NotFound("Cliente");

                if (!string.IsNullOrEmpty(clienteDto.MotoPlaca))
                {
                    var moto = await _motoRepository.GetByIdAsync(clienteDto.MotoPlaca);
                    if (moto is null)
                        return ServiceResponse<Cliente>.NotFound("Moto");

                    var motoAssociada = await _clienteRepository.GetByMotoPlacaAsync(clienteDto.MotoPlaca);
                    if (motoAssociada is not null && motoAssociada.UsuarioCliente != usuarioCliente)
                        return ServiceResponse<Cliente>.Error("Moto já está associada a outro cliente");
                }

                clienteExistente.Nome = clienteDto.Nome.Trim();
                
                if (!string.IsNullOrEmpty(clienteDto.Senha))
                    clienteExistente.SenhaHash = _authService.HashPassword(clienteDto.Senha);
                
                clienteExistente.MotoPlaca = clienteDto.MotoPlaca?.Trim();
                clienteExistente.DataAtualizacao = DateTime.Now;

                await _clienteRepository.UpdateAsync(clienteExistente);
                return ServiceResponse<Cliente>.Ok(clienteExistente, "Cliente atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Cliente>.Error($"Erro ao atualizar cliente: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteClienteAsync(string usuarioCliente)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioCliente))
                    return ServiceResponse<bool>.Error("Usuário do cliente é obrigatório");

                var cliente = await _clienteRepository.GetByIdAsync(usuarioCliente);
                if (cliente is null)
                    return ServiceResponse<bool>.NotFound("Cliente");

                await _clienteRepository.DeleteAsync(cliente);
                return ServiceResponse<bool>.Ok(true, "Cliente excluído com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Error($"Erro ao excluir cliente: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Cliente>> GetClientePorMotoAsync(string motoPlaca)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motoPlaca))
                    return ServiceResponse<Cliente>.Error("Placa da moto é obrigatória");

                var cliente = await _clienteRepository.GetByMotoPlacaAsync(motoPlaca);
                return cliente is null 
                    ? ServiceResponse<Cliente>.NotFound("Cliente para esta moto")
                    : ServiceResponse<Cliente>.Ok(cliente, "Cliente encontrado pela moto");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Cliente>.Error($"Erro ao buscar cliente pela moto: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> AtualizarHistoricoManutencaoAsync(string usuarioCliente)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioCliente))
                    return ServiceResponse<bool>.Error("Usuário do cliente é obrigatório");

                var sucesso = await _clienteRepository.RegistrarManutencaoAsync(usuarioCliente);
                return sucesso 
                    ? ServiceResponse<bool>.Ok(true, "Histórico de manutenção atualizado")
                    : ServiceResponse<bool>.NotFound("Cliente");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Error($"Erro ao atualizar histórico: {ex.Message}");
            }
        }
    }
}