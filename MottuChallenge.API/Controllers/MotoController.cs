using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MottuChallenge.API.Models;
using MottuChallenge.API.DTOs;
using MottuChallenge.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MottuChallenge.API.Enums;
using MottuChallenge.API.DTOs.ML;
using MottuChallenge.API.Models.Common;

namespace MottuChallenge.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Motos")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MotoController : BaseController
    {
        private readonly MotoService _motoService;
        private readonly MotoPredictionService _predictionService;

        public MotoController(MotoService motoService, MotoPredictionService predictionService)
        {
            _motoService = motoService;
            _predictionService = predictionService;
        }

        /// <summary>
        /// Listar todas as motos
        /// </summary>
        /// <remarks>
        /// Retorna todas as motos cadastradas no sistema com opção de filtro por status e setor.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="status">Filtrar por status da moto (opcional)</param>
        /// <param name="setor">Filtrar por setor de conservação (opcional)</param>
        /// <returns>Lista de motos</returns>
        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Listar motos",
            Description = "Retorna todas as motos cadastradas",
            OperationId = "GetMotos"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Moto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Moto>>> GetMotos(
            [FromQuery, SwaggerParameter("Status da moto")] StatusMoto? status = null,
            [FromQuery, SwaggerParameter("Setor de conservação")] SetorMoto? setor = null)
        {
            var response = await _motoService.GetMotosAsync(status, setor);
            return HandleServiceResponse(response);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [SwaggerOperation(Summary = "Listar motos paginadas", Description = "Retorna motos com paginação - Versão 2", OperationId = "GetMotosPagedV2")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<Moto>))]
        public async Task<ActionResult<PagedResponse<Moto>>> GetMotosPagedV2(
            [FromQuery] PaginationParams paginationParams, 
            [FromQuery] StatusMoto? status = null, 
            [FromQuery] SetorMoto? setor = null)
        {
            var response = await _motoService.GetMotosPagedAsync(paginationParams, status, setor);
            if (!response.Success || response.Data == null)
                return HandleServiceResponse(response);

            return HandlePagedResponse(response.Data.Data, response.Data.Page, response.Data.PageSize, response.Data.TotalCount, response.Data.Message);
        }

        /// <summary>
        /// Obter moto específica
        /// </summary>
        /// <remarks>
        /// Retorna os detalhes de uma moto específica pela placa.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="placa">Placa da moto no formato XXX-0000</param>
        /// <returns>Detalhes da moto</returns>
        [HttpGet("{placa}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obter moto",
            Description = "Retorna detalhes de uma moto específica",
            OperationId = "GetMoto"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Moto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Moto>> GetMoto(
            [FromRoute, SwaggerParameter("Placa da moto", Required = true)] string placa)
        {
            if (string.IsNullOrEmpty(placa) || !System.Text.RegularExpressions.Regex.IsMatch(placa, @"^[A-Z]{3}-\d{4}$"))
                return BadRequest(CreateErrorResponse("Formato de placa inválido. Use: XXX-0000"));

            var response = await _motoService.GetMotoAsync(placa);
            return HandleServiceResponse(response);
        }

        /// <summary>
        /// Criar nova moto
        /// </summary>
        /// <remarks>
        /// Cria uma nova moto no sistema.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="motoDto">Dados da nova moto</param>
        /// <returns>Moto criada</returns>
        [HttpPost]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Criar moto",
            Description = "Cadastra uma nova moto no sistema",
            OperationId = "CreateMoto"
        )]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Moto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Moto>> CreateMoto(
            [FromBody, SwaggerRequestBody("Dados da moto", Required = true)] MotoDto motoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados da moto inválidos"));

            var response = await _motoService.CreateMotoAsync(motoDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return HandleCreatedResponse(
                nameof(GetMoto),
                new { placa = response.Data!.Placa, version = RequestedApiVersion },
                response.Data,
                "Moto criada com sucesso"
            );
        }

        /// <summary>
        /// Atualizar moto
        /// </summary>
        /// <remarks>
        /// Atualiza os dados de uma moto existente.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="placa">Placa da moto</param>
        /// <param name="motoDto">Novos dados da moto</param>
        /// <returns>Moto atualizada</returns>
        [HttpPut("{placa}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Atualizar moto",
            Description = "Atualiza dados de uma moto existente",
            OperationId = "UpdateMoto"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Moto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Moto>> UpdateMoto(
            [FromRoute, SwaggerParameter("Placa da moto", Required = true)] string placa,
            [FromBody, SwaggerRequestBody("Dados atualizados da moto", Required = true)] MotoDto motoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de atualização inválidos"));

            if (placa != motoDto.Placa)
                return BadRequest(CreateErrorResponse("A placa da URL não corresponde à placa do corpo da requisição"));

            var response = await _motoService.UpdateMotoAsync(placa, motoDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                Data = response.Data,
                Message = "Moto atualizada com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        /// <summary>
        /// Excluir moto
        /// </summary>
        /// <remarks>
        /// Remove uma moto do sistema.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="placa">Placa da moto</param>
        /// <returns>Confirmação de exclusão</returns>
        [HttpDelete("{placa}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Excluir moto",
            Description = "Remove uma moto do sistema",
            OperationId = "DeleteMoto"
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteMoto(
            [FromRoute, SwaggerParameter("Placa da moto", Required = true)] string placa)
        {
            if (string.IsNullOrEmpty(placa) || !System.Text.RegularExpressions.Regex.IsMatch(placa, @"^[A-Z]{3}-\d{4}$"))
                return BadRequest(CreateErrorResponse("Formato de placa inválido. Use: XXX-0000"));

            var response = await _motoService.DeleteMotoAsync(placa);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        /// <summary>
        /// Prever necessidade de manutenção (ML.NET)
        /// </summary>
        /// <remarks>
        /// Utiliza machine learning para prever se uma moto precisa de manutenção com base em seus dados.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <param name="placa">Placa da moto</param>
        /// <returns>Resultado da predição de manutenção</returns>
        [HttpGet("{placa}/prever-manutencao")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Prever manutenção",
            Description = "Utiliza ML.NET para prever necessidade de manutenção",
            OperationId = "PreverManutencao"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> PreverManutencao(
            [FromRoute, SwaggerParameter("Placa da moto", Required = true)] string placa)
        {
            if (string.IsNullOrEmpty(placa))
                return BadRequest(CreateErrorResponse("Placa é obrigatória"));

            var predictionResponse = await _motoService.PreverManutencaoAsync(placa);
            var motoResponse = await _motoService.GetMotoAsync(placa);

            if (!predictionResponse.Success || !motoResponse.Success)
                return NotFound(CreateErrorResponse(predictionResponse.Message));

            var moto = motoResponse.Data!;
            var prediction = predictionResponse.Data!;
            var fatores = _predictionService.ObterFatoresInfluentes(moto, prediction);
            var recomendacao = _predictionService.ObterRecomendacaoManutencao(prediction);

            var resultado = new MotoManutencaoResponseDto
            {
                Placa = placa,
                PrecisaManutencao = prediction.PrecisaManutencao,
                Probabilidade = prediction.Probability,
                Score = prediction.Score,
                Recomendacao = recomendacao,
                NivelUrgencia = prediction.NivelUrgencia,
                Fatores = fatores,
                Timestamp = DateTime.Now
            };

            return Ok(new
            {
                Data = resultado,
                Message = "Predição de manutenção realizada com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion,
                ModeloML = "Regressão Logística com normalização de features"
            });
        }

        /// <summary>
        /// Listar motos por pátio (V2)
        /// </summary>
        /// <remarks>
        /// **VERSÃO 2** - Retorna todas as motos de um pátio específico com estatísticas.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <returns>Lista de motos do pátio</returns>
        [HttpGet("patio/{nomePatio}")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar motos por pátio (V2)",
            Description = "Retorna motos de um pátio específico com estatísticas - Versão 2",
            OperationId = "GetMotosPorPatioV2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> GetMotosPorPatioV2(
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio)
        {
            var response = await _motoService.GetMotosPorPatioAsync(nomePatio);
            
            if (!response.Success || response.Data == null || !response.Data.Any())
                return NoContent();

            var motos = response.Data;
            var estatisticas = new
            {
                TotalMotos = motos.Count,
                MotosDisponiveis = motos.Count(m => m.Status == StatusMoto.Disponivel),
                MotosAlugadas = motos.Count(m => m.Status == StatusMoto.Alugada),
                MotosManutencao = motos.Count(m => m.Status == StatusMoto.Manutencao),
                MotosPrecisandoManutencao = motos.Count(m => m.PrecisaManutencao == 1),
                PercentualDisponiveis = motos.Count > 0 ? 
                    Math.Round((double)motos.Count(m => m.Status == StatusMoto.Disponivel) / motos.Count * 100, 2) : 0
            };

            return Ok(new
            {
                Data = new
                {
                    Motos = motos,
                    Estatisticas = estatisticas
                },
                Message = $"Motos do pátio {nomePatio} recuperadas com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }

        /// <summary>
        /// Listar motos que precisam de manutenção
        /// </summary>
        /// <remarks>
        /// Retorna todas as motos que foram identificadas como precisando de manutenção.
        /// 
        /// **Roles:** Funcionario, Admin
        /// </remarks>
        /// <returns>Lista de motos para manutenção</returns>
        [HttpGet("precisando-manutencao")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Listar motos para manutenção",
            Description = "Retorna motos que precisam de manutenção",
            OperationId = "GetMotosPrecisandoManutencao"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Moto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Moto>>> GetMotosPrecisandoManutencao()
        {
            var response = await _motoService.GetMotosPrecisandoManutencaoAsync();
            return HandleServiceResponse(response);
        }
    }
}