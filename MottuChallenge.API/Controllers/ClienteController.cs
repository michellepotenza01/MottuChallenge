using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MottuChallenge.API.Models;
using MottuChallenge.API.DTOs;
using MottuChallenge.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MottuChallenge.API.Models.Common;

namespace MottuChallenge.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Clientes")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ClienteController : BaseController
    {
        private readonly ClienteService _clienteService;

        public ClienteController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        /// <summary>
        /// Listar todos os clientes
        /// </summary>
        /// <remarks>
        /// Retorna todos os clientes cadastrados no sistema.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <returns>Lista de clientes</returns>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar clientes",
            Description = "Retorna todos os clientes cadastrados",
            OperationId = "GetClientes"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Cliente>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Cliente>>> GetClientes()
        {
            var response = await _clienteService.GetClientesAsync();
            return HandleServiceResponse(response);
        }

        /// <summary>
        /// Obter cliente específico
        /// </summary>
        /// <remarks>
        /// Retorna os detalhes de um cliente específico pelo nome de usuário.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="usuarioCliente">Nome de usuário do cliente</param>
        /// <returns>Detalhes do cliente</returns>
        [HttpGet("{usuarioCliente}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obter cliente",
            Description = "Retorna detalhes de um cliente específico",
            OperationId = "GetCliente"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cliente))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Cliente>> GetCliente(
            [FromRoute, SwaggerParameter("Nome de usuário do cliente", Required = true)] string usuarioCliente)
        {
            var response = await _clienteService.GetClienteByIdAsync(usuarioCliente);
            return HandleServiceResponse(response);
        }

        /// <summary>
        /// Criar novo cliente
        /// </summary>
        /// <remarks>
        /// Cria um novo cliente no sistema.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="clienteDto">Dados do novo cliente</param>
        /// <returns>Cliente criado</returns>
        [HttpPost]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Criar cliente",
            Description = "Cadastra um novo cliente no sistema",
            OperationId = "CreateCliente"
        )]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Cliente))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Cliente>> CreateCliente(
            [FromBody, SwaggerRequestBody("Dados do cliente", Required = true)] ClienteDto clienteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados do cliente inválidos"));

            var response = await _clienteService.CreateClienteAsync(clienteDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return HandleCreatedResponse(
                nameof(GetCliente),
                new { usuarioCliente = response.Data!.UsuarioCliente, version = RequestedApiVersion },
                response.Data,
                "Cliente criado com sucesso"
            );
        }

        /// <summary>
        /// Atualizar cliente
        /// </summary>
        /// <remarks>
        /// Atualiza os dados de um cliente existente.
        /// 
        /// **Roles:** Proprietário do recurso, Funcionario, Admin
        /// </remarks>
        /// <param name="usuarioCliente">Nome de usuário do cliente</param>
        /// <param name="clienteDto">Novos dados do cliente</param>
        /// <returns>Cliente atualizado</returns>
        [HttpPut("{usuarioCliente}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Atualizar cliente",
            Description = "Atualiza dados de um cliente existente",
            OperationId = "UpdateCliente"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cliente))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Cliente>> UpdateCliente(
            [FromRoute, SwaggerParameter("Nome de usuário do cliente", Required = true)] string usuarioCliente,
            [FromBody, SwaggerRequestBody("Dados atualizados do cliente", Required = true)] ClienteDto clienteDto)
        {
            if (User.IsInRole("Cliente") && !IsCurrentUser(usuarioCliente))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de atualização inválidos"));

            var response = await _clienteService.UpdateClienteAsync(usuarioCliente, clienteDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                response.Data,
                Message = "Cliente atualizado com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        /// <summary>
        /// Excluir cliente
        /// </summary>
        /// <remarks>
        /// Remove um cliente do sistema.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="usuarioCliente">Nome de usuário do cliente</param>
        /// <returns>Confirmação de exclusão</returns>
        [HttpDelete("{usuarioCliente}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Excluir cliente",
            Description = "Remove um cliente do sistema",
            OperationId = "DeleteCliente"
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteCliente(
            [FromRoute, SwaggerParameter("Nome de usuário do cliente", Required = true)] string usuarioCliente)
        {
            var response = await _clienteService.DeleteClienteAsync(usuarioCliente);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        /// <summary>
        /// Buscar cliente por moto (V2)
        /// </summary>
        /// <remarks>
        /// **VERSÃO 2** - Retorna o cliente associado a uma moto específica.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="motoPlaca">Placa da moto no formato XXX-0000</param>
        /// <returns>Cliente associado à moto</returns>
        [HttpGet("por-moto/{motoPlaca}")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Buscar cliente por moto (V2)",
            Description = "Retorna cliente associado a uma moto específica - Versão 2",
            OperationId = "GetClientePorMotoV2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cliente))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Cliente>> GetClientePorMotoV2(
            [FromRoute, SwaggerParameter("Placa da moto", Required = true)] string motoPlaca)
        {
            var response = await _clienteService.GetClientePorMotoAsync(motoPlaca);
            return HandleServiceResponse(response);
        }

        /// <summary>
        /// Atualizar histórico de manutenção do cliente
        /// </summary>
        /// <remarks>
        /// Registra uma nova manutenção no histórico do cliente.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="usuarioCliente">Nome de usuário do cliente</param>
        /// <returns>Confirmação da atualização</returns>
        [HttpPost("{usuarioCliente}/registrar-manutencao")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Registrar manutenção do cliente",
            Description = "Atualiza histórico de manutenções do cliente",
            OperationId = "RegistrarManutencaoCliente"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> RegistrarManutencaoCliente(
            [FromRoute, SwaggerParameter("Nome de usuário do cliente", Required = true)] string usuarioCliente)
        {
            var response = await _clienteService.AtualizarHistoricoManutencaoAsync(usuarioCliente);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                Message = "Histórico de manutenção atualizado com sucesso",
                UsuarioCliente = usuarioCliente,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        /// <summary>
        /// Estatísticas de clientes (V2)
        /// </summary>
        /// <remarks>
        /// **VERSÃO 2** - Retorna estatísticas detalhadas sobre os clientes do sistema.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <returns>Estatísticas dos clientes</returns>
        [HttpGet("estatisticas")]
        [MapToApiVersion("2.0")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Estatísticas de clientes (V2)",
            Description = "Retorna estatísticas detalhadas dos clientes - Versão 2",
            OperationId = "GetEstatisticasClientesV2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetEstatisticasClientesV2()
        {
            var clientesResponse = await _clienteService.GetClientesAsync();
            
            if (!clientesResponse.Success || clientesResponse.Data == null)
                return NoContent();

            var clientes = clientesResponse.Data;
            var clientesComMoto = clientes.Count(c => c.PossuiMoto());
            var clientesSemMoto = clientes.Count(c => !c.PossuiMoto());

            var estatisticas = new
            {
                TotalClientes = clientes.Count,
                ClientesComMoto = clientesComMoto,
                ClientesSemMoto = clientesSemMoto,
                PercentualComMoto = clientes.Count > 0 ? 
                    Math.Round((double)clientesComMoto / clientes.Count * 100, 2) : 0,
                MediaManutencoes = clientes.Count > 0 ? 
                    Math.Round(clientes.Average(c => c.QuantidadeManutencoes), 2) : 0,
                ClienteMaisAntigo = clientes.MinBy(c => c.DataCriacao)?.UsuarioCliente,
                UltimaAtualizacao = DateTime.Now
            };

            return Ok(new
            {
                Data = estatisticas,
                Message = "Estatísticas de clientes recuperadas com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }
    }
}