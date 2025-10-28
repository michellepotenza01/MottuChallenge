using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MottuChallenge.API.Models.Auth;
using MottuChallenge.API.Models.Common;
using MottuChallenge.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Autenticação")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthController : BaseController
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realizar login no sistema
        /// </summary>
        /// <remarks>
        /// Autentica funcionários no sistema e retorna token JWT para acesso aos endpoints protegidos.
        /// 
        /// **Roles:** Nenhuma (público)
        /// 
        /// Exemplo de requisição:
        /// ```json
        /// {
        ///     "usuario": "funcionario123",
        ///     "senha": "senhaSegura123"
        /// }
        /// ```
        /// </remarks>
        /// <param name="loginRequest">Credenciais de acesso</param>
        /// <returns>Token JWT e informações de autenticação</returns>
        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Login de funcionarios",
            Description = "Autentica funcionarios e retorna token JWT",
            OperationId = "LoginFuncionario"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody, SwaggerRequestBody("Credenciais de login", Required = true)] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de login inválidos"));

            var response = await _authService.LoginAsync(loginRequest);

            if (!response.Success)
                return Unauthorized(CreateErrorResponse(response.Message));

            return Ok(new
            {
                response.Data,
                Message = "Login realizado com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        /// <summary>
        ///  Validar token JWT atual
        /// </summary>
        /// <remarks>
        /// Verifica se o token JWT atual é válido e retorna informações do usuário autenticado.
        /// 
        /// **Roles:** Qualquer usuário autenticado
        /// </remarks>
        /// <returns>Status da validação e informações do usuário</returns>
        [HttpGet("validate")]
        [MapToApiVersion("1.0")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Validar token JWT",
            Description = "Verifica a validade do token JWT atual",
            OperationId = "ValidateToken"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult ValidateToken()
        {
            var userInfo = new
            {
                Usuario = User.Identity?.Name,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                User.Identity?.AuthenticationType
            };

            return Ok(new
            {
                Message = "Token válido",
                Data = userInfo,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        /// <summary>
        ///  Obter informações do usuário atual (V2)
        /// </summary>
        /// <remarks>
        /// **VERSÃO 2** - Retorna informações detalhadas do usuário autenticado.
        /// 
        /// **Roles:** Qualquer usuário autenticado
        /// </remarks>
        /// <returns>Informações detalhadas do usuário</returns>
        [HttpGet("me")]
        [MapToApiVersion("2.0")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obter usuário atual (V2)",
            Description = "Retorna informações detalhadas do usuário autenticado - Versão 2",
            OperationId = "GetCurrentUserV2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetCurrentUserV2()
        {
            var userInfo = new
            {
                Usuario = User.Identity?.Name,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }),
                SessionId = Guid.NewGuid().ToString(),
                IssuedAt = DateTime.Now
            };

            return Ok(new
            {
                Data = userInfo,
                Message = "Informações do usuário recuperadas com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }
    }
}