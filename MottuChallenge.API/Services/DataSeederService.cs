using MottuChallenge.API.Models;
using MottuChallenge.API.Repositories;


namespace MottuChallenge.API.Services
{
    public interface IDataSeederService
    {
        Task SeedDataAsync();
    }

    public class DataSeederService : IDataSeederService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataSeederService> _logger;

        public DataSeederService(IServiceProvider serviceProvider, ILogger<DataSeederService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var patioRepository = scope.ServiceProvider.GetRequiredService<PatioRepository>();
            var funcionarioRepository = scope.ServiceProvider.GetRequiredService<FuncionarioRepository>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            try
            {
                _logger.LogInformation("Iniciando data seeding...");

                var patioCentral = await patioRepository.GetByIdAsync("Patio-Central");
                if (patioCentral == null)
                {
                    patioCentral = new Patio
                    {
                        NomePatio = "Patio-Central",
                        Localizacao = "Av. Paulista, 1000 - São Paulo/SP",
                        VagasTotais = 50,
                        VagasOcupadas = 0
                    };
                    await patioRepository.AddAsync(patioCentral);
                    _logger.LogInformation("Pátio Central criado com sucesso");
                }

                var adminPrincipal = await funcionarioRepository.GetByIdAsync("admin_principal");
                if (adminPrincipal == null)
                {
                    adminPrincipal = new Funcionario
                    {
                        UsuarioFuncionario = "admin_principal",
                        Nome = "Administrador Principal",
                        SenhaHash = authService.HashPassword("Admin123!"),
                        NomePatio = "Patio-Central",
                        Role = "Admin"
                    };
                    await funcionarioRepository.AddAsync(adminPrincipal);
                    _logger.LogInformation("Admin principal criado com sucesso - Usuário: admin_principal, Senha: Admin123!");
                }

                var funcionarioTeste = await funcionarioRepository.GetByIdAsync("funcionario_teste");
                if (funcionarioTeste == null)
                {
                    funcionarioTeste = new Funcionario
                    {
                        UsuarioFuncionario = "funcionario_teste",
                        Nome = "Funcionário de Teste",
                        SenhaHash = authService.HashPassword("Func123!"),
                        NomePatio = "Patio-Central",
                        Role = "Funcionario"
                    };
                    await funcionarioRepository.AddAsync(funcionarioTeste);
                    _logger.LogInformation("Funcionário de teste criado com sucesso - Usuário: funcionario_teste, Senha: Func123!");
                }

                _logger.LogInformation("Data seeding concluído com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante data seeding");
                throw;
            }
        }
    }

     public static class DataSeederExtensions
    {
        public static IHost SeedData(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
            seeder.SeedDataAsync().GetAwaiter().GetResult();
            return host;
        }
    }
}