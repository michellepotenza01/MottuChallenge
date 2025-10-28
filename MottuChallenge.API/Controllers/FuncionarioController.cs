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
    [Tags("Funcionarios")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class FuncionarioController : BaseController
    {
        private readonly FuncionarioService _funcionarioService;

        public FuncionarioController(FuncionarioService funcionarioService)
        {
            _funcionarioService = funcionarioService;
        }

        /// <summary>
        /// Listar todos os funcionários
        /// </summary>
        /// <remarks>
        /// Retorna todos os funcionários cadastrados no sistema.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <returns>Lista de funcionários</returns>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar funcionários",
            Description = "Retorna todos os funcionários cadastrados",
            OperationId = "GetFuncionarios"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Funcionario>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Funcionario>>> GetFuncionarios()
        {
            var response = await _funcionarioService.GetFuncionariosAsync();
            return HandleServiceResponse(response);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar funcionários paginados", Description = "Retorna funcionários com paginação - Versão 2", OperationId = "GetFuncionariosPagedV2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<Funcionario>))]
        public async Task<ActionResult<PagedResponse<Funcionario>>> GetFuncionariosPagedV2([FromQuery] PaginationParams paginationParams)
        {
            var response = await _funcionarioService.GetFuncionariosPagedAsync(paginationParams);
            if (!response.Success || response.Data == null)
                return HandleServiceResponse(response);

            return HandlePagedResponse(response.Data.Data, response.Data.Page, response.Data.PageSize, response.Data.TotalCount, response.Data.Message);
        }

        /// <summary>
        /// Obter funcionário específico
        /// </summary>
        /// <remarks>
        /// Retorna os detalhes de um funcionário específico pelo nome de usuário.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="usuarioFuncionario">Nome de usuário do funcionário</param>
        /// <returns>Detalhes do funcionário</returns>
        [HttpGet("{usuarioFuncionario}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obter funcionário",
            Description = "Retorna detalhes de um funcionário específico",
            OperationId = "GetFuncionario"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Funcionario))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Funcionario>> GetFuncionario(
            [FromRoute, SwaggerParameter("Nome de usuário do funcionário", Required = true)] string usuarioFuncionario)
        {
            var response = await _funcionarioService.GetFuncionarioByIdAsync(usuarioFuncionario);
            return HandleServiceResponse(response);
        }

        /// <summary>
        /// Criar novo funcionário
        /// </summary>
        /// <remarks>
        /// Cria um novo funcionário no sistema.
        /// 
        /// **Roles:** Admin
        /// </remarks>
        /// <param name="funcionarioDto">Dados do novo funcionário</param>
        /// <returns>Funcionário criado</returns>
        [HttpPost]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Criar funcionário",
            Description = "Cadastra um novo funcionário no sistema",
            OperationId = "CreateFuncionario"
        )]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Funcionario))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Funcionario>> CreateFuncionario(
            [FromBody, SwaggerRequestBody("Dados do funcionário", Required = true)] FuncionarioDto funcionarioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados do funcionário inválidos"));

            var response = await _funcionarioService.CreateFuncionarioAsync(funcionarioDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return HandleCreatedResponse(
                nameof(GetFuncionario),
                new { usuarioFuncionario = response.Data!.UsuarioFuncionario, version = RequestedApiVersion },
                response.Data,
                "Funcionário criado com sucesso"
            );
        }

        /// <summary>
        /// Atualizar funcionário
        /// </summary>
        /// <remarks>
        /// Atualiza os dados de um funcionário existente.
        /// 
        /// **Roles:** Admin, Funcionario (apenas próprio cadastro)
        /// </remarks>
        /// <param name="usuarioFuncionario">Nome de usuário do funcionário</param>
        /// <param name="funcionarioDto">Novos dados do funcionário</param>
        /// <returns>Funcionário atualizado</returns>
        [HttpPut("{usuarioFuncionario}")]
        [Authorize(Roles = "Admin,Funcionario")]
        [SwaggerOperation(
            Summary = "Atualizar funcionário",
            Description = "Atualiza dados de um funcionário existente",
            OperationId = "UpdateFuncionario"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Funcionario))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Funcionario>> UpdateFuncionario(
            [FromRoute, SwaggerParameter("Nome de usuário do funcionário", Required = true)] string usuarioFuncionario,
            [FromBody, SwaggerRequestBody("Dados atualizados do funcionário", Required = true)] FuncionarioDto funcionarioDto)
        {
            if (User.IsInRole("Funcionario") && !IsCurrentUser(usuarioFuncionario))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de atualização inválidos"));

            var response = await _funcionarioService.UpdateFuncionarioAsync(usuarioFuncionario, funcionarioDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                Data = response.Data,
                Message = "Funcionário atualizado com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        /// <summary>
        /// Excluir funcionário
        /// </summary>
        /// <remarks>
        /// Remove um funcionário do sistema.
        /// 
        /// **Roles:** Admin
        /// </remarks>
        /// <param name="usuarioFuncionario">Nome de usuário do funcionário</param>
        /// <returns>Confirmação de exclusão</returns>
        [HttpDelete("{usuarioFuncionario}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Excluir funcionário",
            Description = "Remove um funcionário do sistema",
            OperationId = "DeleteFuncionario"
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteFuncionario(
            [FromRoute, SwaggerParameter("Nome de usuário do funcionário", Required = true)] string usuarioFuncionario)
        {
            var response = await _funcionarioService.DeleteFuncionarioAsync(usuarioFuncionario);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        /// <summary>
        /// Listar funcionários por pátio (V2)
        /// </summary>
        /// <remarks>
        /// **VERSÃO 2** - Retorna todos os funcionários de um pátio específico com informações adicionais.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <returns>Lista de funcionários do pátio</returns>
        [HttpGet("patio/{nomePatio}")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar funcionários por pátio (V2)",
            Description = "Retorna funcionários de um pátio específico - Versão 2",
            OperationId = "GetFuncionariosPorPatioV2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Funcionario>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Funcionario>>> GetFuncionariosPorPatioV2(
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio)
        {
            var response = await _funcionarioService.GetFuncionariosPorPatioAsync(nomePatio);

            if (!response.Success || response.Data == null || !response.Data.Any())
                return NoContent();

            return Ok(new
            {
                response.Data,
                Message = $"Funcionários do pátio {nomePatio} recuperados com sucesso",
                Total = response.Data.Count,
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }
        
        [HttpGet("patio/{nomePatio}/paged")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Listar funcionários por pátio paginados", Description = "Retorna funcionários de um pátio com paginação - Versão 2", OperationId = "GetFuncionariosPorPatioPagedV2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<Funcionario>))]
        public async Task<ActionResult<PagedResponse<Funcionario>>> GetFuncionariosPorPatioPagedV2(
            [FromRoute] string nomePatio, 
            [FromQuery] PaginationParams paginationParams)
        {
            var response = await _funcionarioService.GetFuncionariosPorPatioPagedAsync(nomePatio, paginationParams);
            if (!response.Success || response.Data == null)
                return HandleServiceResponse(response);

            return HandlePagedResponse(response.Data.Data, response.Data.Page, response.Data.PageSize, response.Data.TotalCount, response.Data.Message);
        }

        /// <summary>
        /// Verificar se funcionário pertence ao pátio
        /// </summary>
        /// <remarks>
        /// Verifica se um funcionário específico pertence a um determinado pátio.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="usuarioFuncionario">Nome de usuário do funcionário</param>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <returns>Status da verificação</returns>
        [HttpGet("{usuarioFuncionario}/pertence-patio/{nomePatio}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Verificar pátio do funcionário",
            Description = "Verifica se funcionário pertence ao pátio",
            OperationId = "VerificarFuncionarioNoPatio"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> VerificarFuncionarioNoPatio(
            [FromRoute, SwaggerParameter("Nome de usuário do funcionário", Required = true)] string usuarioFuncionario,
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio)
        {
            var response = await _funcionarioService.VerificarFuncionarioNoPatioAsync(usuarioFuncionario, nomePatio);
            
            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                Data = new
                {
                    UsuarioFuncionario = usuarioFuncionario,
                    NomePatio = nomePatio,
                    Pertence = response.Data,
                    Timestamp = DateTime.Now
                },
                Message = response.Data ? 
                    "Funcionário pertence ao pátio" : 
                    "Funcionário não pertence ao pátio",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }
    }
}