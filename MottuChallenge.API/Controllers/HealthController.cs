using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MottuChallenge.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Health Check")]
    [Produces("application/json")]
    public class HealthController : BaseController
    {
        private readonly HealthService _healthService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(HealthService healthService, ILogger<HealthController> logger)
        {
            _healthService = healthService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Verificar saúde completa da API")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetHealthComplete()
        {
            _logger.LogInformation("Health check completo solicitado");
            
            var response = await _healthService.CheckHealthAsync();
            
            if (!response.Success)
            {
                return StatusCode(503, new
                {
                    Status = "ServiceUnavailable",
                    Message = "Serviço indisponível",
                    Details = response.Data,
                    Timestamp = DateTime.Now,
                    Version = RequestedApiVersion
                });
            }

            var healthData = response.Data!;
            
            var statusCode = healthData.Status switch
            {
                CustomHealthStatus.Healthy => StatusCodes.Status200OK,
                CustomHealthStatus.Degraded => StatusCodes.Status200OK,
                CustomHealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status503ServiceUnavailable
            };

            return StatusCode(statusCode, new
            {
                Status = healthData.Status.ToString(),
                Message = "Health check realizado com sucesso",
                Data = healthData,
                Timestamp = healthData.Timestamp,
                Version = RequestedApiVersion
            });
        }

        [HttpGet("database")]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Verificar saúde do banco de dados")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetDatabaseHealth()
        {
            var response = await _healthService.CheckHealthAsync();
            
            if (!response.Success || response.Data?.Entries == null)
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Message = "Não foi possível verificar o banco de dados",
                    Timestamp = DateTime.Now
                });
            }

            if (!response.Data.Entries.TryGetValue("database", out var dbHealth))
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Message = "Informações do database não disponíveis",
                    Timestamp = DateTime.Now
                });
            }

            var statusCode = dbHealth.Status switch
            {
                CustomHealthStatus.Healthy => StatusCodes.Status200OK,
                CustomHealthStatus.Degraded => StatusCodes.Status200OK,
                CustomHealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status503ServiceUnavailable
            };

            return StatusCode(statusCode, new
            {
                Status = dbHealth.Status.ToString(),
                Message = dbHealth.Description,
                Data = new { dbHealth.Status, dbHealth.Description, dbHealth.Duration, dbHealth.Data },
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        [HttpGet("statistics")]
        [MapToApiVersion("2.0")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(Summary = "Estatísticas do sistema")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetStatisticsV2()
        {
            var response = await _healthService.CheckHealthAsync();
            
            if (!response.Success || response.Data?.Entries == null)
            {
                return Ok(new
                {
                    Message = "Não foi possível coletar estatísticas completas",
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                });
            }

            if (!response.Data.Entries.TryGetValue("statistics", out var statsEntry))
            {
                return Ok(new
                {
                    Message = "Estatísticas não disponíveis",
                    Timestamp = DateTime.Now,
                    Version = "2.0"
                });
            }

            var statisticsInfo = new
            {
                DatabaseStats = statsEntry.Data,
                SystemInfo = new
                {
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                    Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                    CurrentTime = DateTime.Now
                }
            };

            return Ok(new
            {
                Data = statisticsInfo,
                Message = "Estatísticas recuperadas com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }

        [HttpGet("ping")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Health check simples")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult PingV2()
        {
            var pingInfo = new
            {
                Status = "Healthy",
                Message = "API está respondendo normalmente",
                Timestamp = DateTime.Now,
                Version = "2.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                Metrics = new
                {
                    Uptime = Environment.TickCount / 1000,
                    MemoryUsageMB = GC.GetTotalMemory(false) / 1024 / 1024
                }
            };

            return Ok(new
            {
                Data = pingInfo,
                Message = "Ping realizado com sucesso",
                Timestamp = DateTime.Now
            });
        }

        [HttpGet("version")]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Obter informações da versão")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetVersion()
        {
            var assembly = GetType().Assembly;
            var versionInfo = new
            {
                ApiVersion = RequestedApiVersion,
                AssemblyVersion = assembly.GetName().Version?.ToString() ?? "1.0.0",
                Framework = "ASP.NET Core 8.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                Timestamp = DateTime.Now,
                Features = new[] 
                { 
                    "JWT Authentication", 
                    "ML.NET Integration", 
                    "Health Checks", 
                    "API Versioning",
                    "Swagger Documentation"
                }
            };

            return Ok(new
            {
                Data = versionInfo,
                Message = "Informações de versão recuperadas",
                Timestamp = DateTime.Now
            });
        }
    }
}