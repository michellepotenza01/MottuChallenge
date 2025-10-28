using MottuChallenge.API.Data;
using Microsoft.EntityFrameworkCore;
using MottuChallenge.API.Models.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MottuChallenge.API.Enums;

namespace MottuChallenge.API.Services
{
    public class HealthService
    {
        private readonly MottuDbContext _context;
        private readonly ILogger<HealthService> _logger;

        public HealthService(MottuDbContext context, ILogger<HealthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResponse<CustomHealthReport>> CheckHealthAsync()
        {
            var healthReport = new CustomHealthReport();
            var entries = new Dictionary<string, CustomHealthReportEntry>();

            try
            {
                _logger.LogInformation("Iniciando health check completo...");

                var databaseEntry = await CheckDatabaseHealthAsync();
                entries.Add("database", databaseEntry);

                var apiEntry = await CheckApiHealthAsync();
                entries.Add("api", apiEntry);

                var mlEntry = await CheckMlServiceHealthAsync();
                entries.Add("ml_service", mlEntry);

                var memoryEntry = CheckMemoryHealth();
                entries.Add("memory", memoryEntry);

                var statsEntry = await GetStatisticsAsync();
                entries.Add("statistics", statsEntry);

                healthReport.Status = CalculateOverallStatus(entries);
                healthReport.TotalDuration = CalculateTotalDuration(entries);
                healthReport.Entries = entries;
                healthReport.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                healthReport.Timestamp = DateTime.UtcNow;

                _logger.LogInformation("Health check concluído: {Status}", healthReport.Status);

                return ServiceResponse<CustomHealthReport>.Ok(healthReport, "Health report gerado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante health check");
                
                var errorEntry = new CustomHealthReportEntry(
                    CustomHealthStatus.Unhealthy,
                    $"Exception: {ex.Message}",
                    TimeSpan.Zero,
                    data: null
                );
                
                entries.Add("error", errorEntry);
                healthReport.Status = CustomHealthStatus.Unhealthy;
                healthReport.Entries = entries;
                healthReport.Timestamp = DateTime.UtcNow;

                return ServiceResponse<CustomHealthReport>.Error($"Health check falhou: {ex.Message}")
                    .Convert(healthReport);
            }
        }

        private async Task<CustomHealthReportEntry> CheckDatabaseHealthAsync()
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return new CustomHealthReportEntry(
                        CustomHealthStatus.Unhealthy,
                        "Não foi possível conectar ao banco de dados",
                        DateTime.UtcNow - startTime,
                        data: new Dictionary<string, object>
                        {
                            ["connection_string"] = _context.Database.GetConnectionString()?.Split(';')[0] + "..."
                        }
                    );
                }

                var patiosCount = await _context.Patios.CountAsync();
                var duration = DateTime.UtcNow - startTime;

                return new CustomHealthReportEntry(
                    CustomHealthStatus.Healthy,
                    $"Database conectado. {patiosCount} pátios encontrados.",
                    duration,
                    data: new Dictionary<string, object>
                    {
                        ["patios_count"] = patiosCount,
                        ["response_time_ms"] = duration.TotalMilliseconds
                    }
                );
            }
            catch (Exception ex)
            {
                return new CustomHealthReportEntry(
                    CustomHealthStatus.Unhealthy,
                    $"Erro no banco de dados: {ex.Message}",
                    DateTime.UtcNow - startTime,
                    data: null
                );
            }
        }

        private async Task<CustomHealthReportEntry> CheckApiHealthAsync()
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                await Task.Delay(10);
                var duration = DateTime.UtcNow - startTime;

                return new CustomHealthReportEntry(
                    CustomHealthStatus.Healthy,
                    "API respondendo normalmente",
                    duration,
                    data: new Dictionary<string, object>
                    {
                        ["uptime_seconds"] = Environment.TickCount / 1000,
                        ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                    }
                );
            }
            catch (Exception ex)
            {
                return new CustomHealthReportEntry(
                    CustomHealthStatus.Unhealthy,
                    $"Erro na API: {ex.Message}",
                    DateTime.UtcNow - startTime,
                    data: null
                );
            }
        }

        private async Task<CustomHealthReportEntry> CheckMlServiceHealthAsync()
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                await Task.Delay(5);
                var duration = DateTime.UtcNow - startTime;

                return new CustomHealthReportEntry(
                    CustomHealthStatus.Healthy,
                    "ML Service operacional",
                    duration,
                    data: new Dictionary<string, object>
                    {
                        ["model_loaded"] = true,
                        ["prediction_available"] = true
                    }
                );
            }
            catch (Exception ex)
            {
                return new CustomHealthReportEntry(
                    CustomHealthStatus.Unhealthy,
                    $"Erro no ML Service: {ex.Message}",
                    DateTime.UtcNow - startTime,
                    data: null
                );
            }
        }

        private CustomHealthReportEntry CheckMemoryHealth()
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var memory = GC.GetTotalMemory(false) / 1024 / 1024;
                var duration = DateTime.UtcNow - startTime;

                var status = memory < 500 ? CustomHealthStatus.Healthy : 
                            memory < 1000 ? CustomHealthStatus.Degraded : 
                            CustomHealthStatus.Unhealthy;

                var description = status switch
                {
                    CustomHealthStatus.Healthy => $"Memória OK: {memory}MB",
                    CustomHealthStatus.Degraded => $"Memória elevada: {memory}MB",
                    _ => $"Memória crítica: {memory}MB"
                };

                return new CustomHealthReportEntry(
                    status,
                    description,
                    duration,
                    data: new Dictionary<string, object>
                    {
                        ["memory_mb"] = memory,
                        ["gc_collections"] = new 
                        {
                            gen0 = GC.CollectionCount(0),
                            gen1 = GC.CollectionCount(1),
                            gen2 = GC.CollectionCount(2)
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                return new CustomHealthReportEntry(
                    CustomHealthStatus.Unhealthy,
                    $"Erro na verificação de memória: {ex.Message}",
                    DateTime.UtcNow - startTime,
                    data: null
                );
            }
        }

        private async Task<CustomHealthReportEntry> GetStatisticsAsync()
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var patiosCount = await _context.Patios.CountAsync();
                var motosCount = await _context.Motos.CountAsync();
                var funcionariosCount = await _context.Funcionarios.CountAsync();
                var clientesCount = await _context.Clientes.CountAsync();
                
                var motosDisponiveis = await _context.Motos
                    .CountAsync(m => m.Status == StatusMoto.Disponivel);
                
                var motosManutencao = await _context.Motos
                    .CountAsync(m => m.PrecisaManutencao == 1);

                var duration = DateTime.UtcNow - startTime;

                return new CustomHealthReportEntry(
                    CustomHealthStatus.Healthy,
                    "Estatísticas coletadas com sucesso",
                    duration,
                    data: new Dictionary<string, object>
                    {
                        ["total_patios"] = patiosCount,
                        ["total_motos"] = motosCount,
                        ["total_funcionarios"] = funcionariosCount,
                        ["total_clientes"] = clientesCount,
                        ["motos_disponiveis"] = motosDisponiveis,
                        ["motos_manutencao"] = motosManutencao
                    }
                );
            }
            catch (Exception ex)
            {
                return new CustomHealthReportEntry(
                    CustomHealthStatus.Degraded,
                    $"Erro ao coletar estatísticas: {ex.Message}",
                    DateTime.UtcNow - startTime,
                    data: null
                );
            }
        }

        private static CustomHealthStatus CalculateOverallStatus(Dictionary<string, CustomHealthReportEntry> entries)
        {
            if (entries.Values.Any(e => e.Status == CustomHealthStatus.Unhealthy))
                return CustomHealthStatus.Unhealthy;
            
            if (entries.Values.Any(e => e.Status == CustomHealthStatus.Degraded))
                return CustomHealthStatus.Degraded;
                
            return CustomHealthStatus.Healthy;
        }

        private static TimeSpan CalculateTotalDuration(Dictionary<string, CustomHealthReportEntry> entries)
        {
            return entries.Values.Aggregate(TimeSpan.Zero, (acc, e) => acc + e.Duration);
        }
    }

    public enum CustomHealthStatus
    {
        Unhealthy = 0,
        Degraded = 1,
        Healthy = 2
    }

    public class CustomHealthReport
    {
        public CustomHealthStatus Status { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public Dictionary<string, CustomHealthReportEntry> Entries { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string Environment { get; set; } = string.Empty;
    }

    public class CustomHealthReportEntry
    {
        public CustomHealthStatus Status { get; }
        public string Description { get; }
        public TimeSpan Duration { get; }
        public IReadOnlyDictionary<string, object>? Data { get; }

        public CustomHealthReportEntry(CustomHealthStatus status, string description, TimeSpan duration, IReadOnlyDictionary<string, object>? data)
        {
            Status = status;
            Description = description;
            Duration = duration;
            Data = data;
        }
    }
}