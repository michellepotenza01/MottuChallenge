using Microsoft.EntityFrameworkCore;
using MottuChallenge.API.Data;
using MottuChallenge.API.Repositories;
using MottuChallenge.API.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5147");
builder.Environment.EnvironmentName = "Development";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
builder.Services.AddDbContext<MottuDbContext>(options =>
    options.UseOracle(connectionString, oracleOptions =>
    {
        oracleOptions.MigrationsAssembly("MottuApi");
    }));

builder.Services.AddScoped<PatioRepository>();
builder.Services.AddScoped<FuncionarioRepository>();
builder.Services.AddScoped<MotoRepository>();
builder.Services.AddScoped<ClienteRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PatioService>();
builder.Services.AddScoped<FuncionarioService>();
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<MotoPredictionService>();

builder.Services.AddScoped<IDataSeederService, DataSeederService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<MottuDbContext>(
        name: "database",
        tags: new[] { "database", "oracle" })
    .AddCheck("memory", 
        () => 
        {
            var memory = GC.GetTotalMemory(false) / 1024 / 1024;
            return memory < 500 
                ? HealthCheckResult.Healthy($"Memória OK: {memory}MB")
                : HealthCheckResult.Degraded($"Memória elevada: {memory}MB");
        },
        tags: new[] { "memory" })
    .AddCheck("api", 
        () => HealthCheckResult.Healthy("API respondendo"),
        tags: new[] { "api" });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("x-api-version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = "minha_chave_super_secreta_para_desenvolvimento_32_chars!";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MottuAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MottuClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("FuncionarioOrAdmin", policy => policy.RequireRole("Funcionario", "Admin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mottu API v1",
        Version = "v1",
        Description = "**API Estável** - Versão principal com todos os endpoints básicos\n\n" +
                     "**Autenticação:** GETs são públicos, POST/PUT/DELETE requerem JWT\n" +
                     "**Status:** Production Ready",
        Contact = new OpenApiContact
        {
            Name = "Equipe Mottu",
            Email = "suporte@mottu.com",
            Url = new Uri("https://mottu.com.br")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Mottu API v2",
        Version = "v2", 
        Description = "**Nova Versão** - Endpoints aprimorados com ML.NET e recursos avançados\n\n" +
                     "**Novidades:**\n" +
                     "• Predição de manutenção com Machine Learning\n" +
                     "• Estatísticas detalhadas\n" +
                     "• Health checks avançados\n" +
                     "• Performance melhorada",
        Contact = new OpenApiContact
        {
            Name = "Equipe Mottu",
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando o esquema Bearer.\n\n" +
                     "Digite: **Bearer** {seu_token} \n\n" +
                     "Exemplo: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        var version = api.ActionDescriptor.EndpointMetadata
            .OfType<MapToApiVersionAttribute>()
            .FirstOrDefault()?.Versions.FirstOrDefault()?.ToString() ?? "v1";
        
        return new[] { $"{controllerName} ({version})" };
    });

    c.OrderActionsBy(api => 
    {
        var httpMethodOrder = new Dictionary<string, int>
        {
            ["GET"] = 1,
            ["POST"] = 2, 
            ["PUT"] = 3,
            ["DELETE"] = 4
        };
        
        var method = api.HttpMethod ?? "";
        return $"{httpMethodOrder.GetValueOrDefault(method, 5)}-{api.RelativePath}";
    });

    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseResponseCompression();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
        await seeder.SeedDataAsync();
        Console.WriteLine("Data seeding executado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no data seeding: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottu API v1 (Estável)");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Mottu API v2 (Nova)");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Mottu API Documentation";
        c.DisplayOperationId();
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
    });
}

app.UseExceptionHandler("/error");

app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mottu API - Sistema de Gerenciamento de Pátio de Motos</title>
    <style>
        :root {
            --mottu-green: #00D46A;
            --mottu-dark: #1A1A1A;
            --mottu-black: #000000;
            --mottu-white: #FFFFFF;
            --mottu-gray: #F5F5F5;
        }
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background: linear-gradient(135deg, var(--mottu-black) 0%, var(--mottu-dark) 100%);
            color: var(--mottu-white);
            min-height: 100vh;
            line-height: 1.6;
        }
        
        .container { 
            max-width: 1200px; 
            margin: 0 auto; 
            padding: 40px 20px;
        }
        
        .header {
            text-align: center;
            margin-bottom: 50px;
            padding: 40px 20px;
            background: rgba(0, 212, 106, 0.1);
            border-radius: 20px;
            border: 1px solid var(--mottu-green);
            position: relative;
            overflow: hidden;
        }
        
        .header::before {
            content: '';
            position: absolute;
            top: -50%;
            left: -50%;
            width: 200%;
            height: 200%;
            background: radial-gradient(circle, rgba(0,212,106,0.1) 0%, rgba(0,0,0,0) 70%);
            animation: float 6s ease-in-out infinite;
        }
        
        @keyframes float {
            0%, 100% { transform: translateY(0px) rotate(0deg); }
            50% { transform: translateY(-20px) rotate(180deg); }
        }
        
        .logo {
            font-size: 4em;
            font-weight: bold;
            color: var(--mottu-green);
            margin-bottom: 20px;
            text-shadow: 0 0 30px rgba(0, 212, 106, 0.5);
        }
        
        .tagline {
            font-size: 1.4em;
            color: var(--mottu-white);
            opacity: 0.9;
            margin-bottom: 30px;
        }
        
        .grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
            gap: 30px;
            margin-bottom: 50px;
        }
        
        .card { 
            background: rgba(255, 255, 255, 0.05);
            padding: 30px;
            border-radius: 15px;
            border-left: 5px solid var(--mottu-green);
            backdrop-filter: blur(10px);
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }
        
        .card::before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, transparent, rgba(0, 212, 106, 0.1), transparent);
            transition: left 0.5s ease;
        }
        
        .card:hover::before {
            left: 100%;
        }
        
        .card:hover {
            transform: translateY(-5px);
            box-shadow: 0 15px 40px rgba(0, 212, 106, 0.2);
        }
        
        .card h2 {
            color: var(--mottu-green);
            margin-bottom: 20px;
            font-size: 1.5em;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .card h2 i {
            font-size: 1.2em;
        }
        
        .feature-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 15px;
            margin: 20px 0;
        }
        
        .feature { 
            background: rgba(0, 212, 106, 0.1);
            padding: 15px;
            border-radius: 8px;
            border: 1px solid rgba(0, 212, 106, 0.3);
            text-align: center;
            transition: all 0.3s ease;
        }
        
        .feature:hover {
            background: rgba(0, 212, 106, 0.2);
            transform: scale(1.05);
        }
        
        .btn { 
            display: inline-flex;
            align-items: center;
            gap: 10px;
            padding: 15px 30px; 
            background: var(--mottu-green); 
            color: var(--mottu-black); 
            text-decoration: none; 
            border-radius: 8px; 
            margin: 5px;
            transition: all 0.3s ease;
            font-weight: 600;
            border: 2px solid transparent;
        }
        
        .btn:hover { 
            background: transparent;
            color: var(--mottu-green);
            border-color: var(--mottu-green);
            transform: translateY(-2px);
            box-shadow: 0 10px 25px rgba(0, 212, 106, 0.3);
        }
        
        .btn-outline {
            background: transparent;
            color: var(--mottu-green);
            border: 2px solid var(--mottu-green);
        }
        
        .btn-outline:hover {
            background: var(--mottu-green);
            color: var(--mottu-black);
        }
        
        .links { 
            text-align: center; 
            margin-top: 30px;
        }
        
        .credential-box {
            background: rgba(255, 255, 255, 0.05);
            padding: 20px;
            border-radius: 10px;
            margin: 15px 0;
            border-left: 4px solid var(--mottu-green);
        }
        
        .credential-box strong {
            color: var(--mottu-green);
        }
        
        .status-badge {
            display: inline-block;
            padding: 5px 15px;
            background: var(--mottu-green);
            color: var(--mottu-black);
            border-radius: 20px;
            font-size: 0.8em;
            font-weight: bold;
            margin-left: 10px;
        }
        
        .tech-stack {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            margin: 20px 0;
        }
        
        .tech-item {
            background: rgba(0, 212, 106, 0.1);
            padding: 8px 15px;
            border-radius: 20px;
            border: 1px solid rgba(0, 212, 106, 0.3);
            font-size: 0.9em;
        }
        
        .footer {
            text-align: center;
            margin-top: 50px;
            padding: 30px;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
            color: rgba(255, 255, 255, 0.7);
        }
        
        @media (max-width: 768px) {
            .grid {
                grid-template-columns: 1fr;
            }
            
            .logo {
                font-size: 2.5em;
            }
            
            .feature-grid {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <div class="logo">MOTTU</div>
            <div class="tagline">Sistema Avançado de Gerenciamento de Pátio de Motos</div>
            <p>API RESTful desenvolvida em .NET com arquitetura moderna e recursos avançados</p>
        </div>
        
        <div class="grid">
            <div class="card">
                <h2>Documentação Interativa</h2>
                <p>Explore todos os endpoints da API com documentação completa e exemplos práticos.</p>
                <div class="tech-stack">
                    <span class="tech-item">Swagger UI</span>
                    <span class="tech-item">OpenAPI 3.0</span>
                    <span class="tech-item">Exemplos</span>
                </div>
                <div class="links">
                    <a href="/swagger" class="btn">
                        <i></i> Acessar Swagger
                    </a>
                </div>
            </div>

            <div class="card">
                <h2> Recursos da API</h2>
                <div class="feature-grid">
                    <div class="feature">
                        <strong>v1 (Estável)</strong>
                        <div>CRUD Completo</div>
                        <div>JWT Authentication</div>
                    </div>
                    <div class="feature">
                        <strong>v2 (Avançada)</strong>
                        <div>ML.NET</div>
                        <div>Estatísticas</div>
                    </div>
                    <div class="feature">
                        <strong>Monitoramento</strong>
                        <div>Health Checks</div>
                        <div>Performance</div>
                    </div>
                    <div class="feature">
                        <strong>REST</strong>
                        <div>HATEOAS</div>
                        <div>Paginação</div>
                    </div>
                </div>
            </div>

            <div class="card">
                <h2> Autenticação & Segurança</h2>
                <p>Sistema de autenticação JWT com diferentes níveis de acesso:</p>
                
                <div class="credential-box">
                    <strong> Administrador</strong><br>
                    Usuário: <code>admin_principal</code><br>
                    Senha: <code>Admin123!</code><br>
                    <span class="status-badge">Acesso Total</span>
                </div>
                
                <div class="credential-box">
                    <strong> Funcionário</strong><br>
                    Usuário: <code>funcionario_teste</code><br>
                    Senha: <code>Func123!</code><br>
                    <span class="status-badge">Acesso Limitado</span>
                </div>
                
                <div class="links">
                    <a href="/swagger#/Auth" class="btn btn-outline">
                        <i></i> Testar Autenticação
                    </a>
                </div>
            </div>

            <div class="card">
                <h2> Domínio da Aplicação</h2>
                <p><strong>Sistema de Gerenciamento de Pátio de Motos:</strong></p>
                <ul style="margin: 15px 0; padding-left: 20px;">
                    <li>Cadastro de Pátios e Vagas</li>
                    <li>Gestão de Funcionários</li>
                    <li>Controle de Frota de Motos</li>
                    <li>Cadastro de Clientes</li>
                    <li>Controle de Status (Disponível/Alugada/Manutenção)</li>
                    <li>Predição de Manutenção com ML.NET</li>
                </ul>
                <div class="tech-stack">
                    <span class="tech-item">.NET 8</span>
                    <span class="tech-item">Oracle</span>
                    <span class="tech-item">Entity Framework</span>
                    <span class="tech-item">ML.NET</span>
                </div>
            </div>

            <div class="card">
                <h2> Monitoramento</h2>
                <p>Acompanhe a saúde e performance da aplicação em tempo real.</p>
                <div class="feature-grid">
                    <div class="feature">
                        <strong> Database</strong>
                        <div>Oracle Connection</div>
                    </div>
                    <div class="feature">
                        <strong> Memória</strong>
                        <div>Uso de Recursos</div>
                    </div>
                    <div class="feature">
                        <strong> API</strong>
                        <div>Status da Aplicação</div>
                    </div>
                </div>
                <div class="links">
                    <a href="/health" class="btn btn-outline">
                        <i></i> Health Check
                    </a>
                </div>
            </div>

            <div class="card">
                <h2> Links Rápidos</h2>
                <p>Acesse rapidamente os principais recursos da API:</p>
                <div class="links">
                    <a href="/api" class="btn">
                        <i></i> API Info
                    </a>
                    <a href="/health" class="btn">
                        <i></i> Health Status
                    </a>
                    <a href="/swagger" class="btn">
                        <i></i> Documentação
                    </a>
                </div>
                <div style="margin-top: 20px;">
                    <p><strong>Versões disponíveis:</strong></p>
                    <div style="display: flex; gap: 10px; margin-top: 10px;">
                        <span class="tech-item">v1 (Estável)</span>
                        <span class="tech-item">v2 (Avançada)</span>
                    </div>
                </div>
            </div>
        </div>

        <div class="footer">
            <p><strong>Mottu API</strong> - Sistema de Gerenciamento de Pátio de Motos</p>
            <p>Desenvolvido com .NET 8 | Arquitetura RESTful | Boas Práticas API</p>
            <p style="margin-top: 10px; font-size: 0.9em; opacity: 0.7;">
                Health Checks |  Versionamento |  JWT Security |  ML.NET |  HATEOAS |  Swagger
            </p>
        </div>
    </div>
</body>
</html>
""", "text/html")).WithTags("Home").WithName("HomePage");

app.MapGet("/index.html", () => Results.Redirect("/")).WithTags("Home");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            environment = app.Environment.EnvironmentName,
            timestamp = DateTime.UtcNow,
            uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = $"{e.Value.Duration.TotalMilliseconds}ms",
                data = e.Value.Data
            }),
            totalDuration = $"{report.TotalDuration.TotalMilliseconds}ms"
        }, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await context.Response.WriteAsync(result);
    }
}).WithMetadata(new EndpointNameMetadata("health-check"));

app.MapGet("/api/info", () => 
{
    var assembly = Assembly.GetExecutingAssembly();
    return new
    {
        application = "Mottu API - Sistema de Gerenciamento de Pátio de Motos",
        version = assembly.GetName().Version?.ToString() ?? "1.0.0",
        environment = app.Environment.EnvironmentName,
        timestamp = DateTime.UtcNow,
        features = new[]
        {
            "JWT Authentication",
            "ML.NET Integration", 
            "Health Checks",
            "API Versioning",
            "HATEOAS Links",
            "Pagination",
            "Swagger Documentation",
            "Oracle Database",
            "Response Compression"
        },
        links = new[]
        {
            new { rel = "documentation", href = "/swagger", method = "GET" },
            new { rel = "health", href = "/health", method = "GET" },
            new { rel = "api-info", href = "/api", method = "GET" },
            new { rel = "home", href = "/", method = "GET" }
        }
    };
}).WithTags("API Info").WithName("GetApiInfo");

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api", () => new
{
    message = "Bem-vindo à Mottu API - Sistema de Gerenciamento de Pátio de Motos",
    description = "API RESTful desenvolvida em .NET 8 com arquitetura moderna e boas práticas",
    documentation = "/swagger",
    health = "/health",
    home = "/",
    versions = new[] { 
        new { 
            version = "v1", 
            status = "stable",
            description = "API Estavel - Endpoints básicos de CRUD",
            path = "/api/v1"
        },
        new { 
            version = "v2", 
            status = "advanced",
            description = "Nova Versao - Recursos avancados com ML.NET",
            path = "/api/v2"
        }
    },
    features = new[] {
        "JWT Authentication",
        "ML.NET Integration", 
        "Health Checks",
        "API Versioning",
        "HATEOAS Links",
        "Pagination",
        "Swagger Documentation",
        "Oracle Database",
        "Response Compression"
    },
    timestamp = DateTime.UtcNow,
    _links = new[]
    {
        new { rel = "self", href = "/api", method = "GET" },
        new { rel = "documentation", href = "/swagger", method = "GET" },
        new { rel = "health", href = "/health", method = "GET" },
        new { rel = "home", href = "/", method = "GET" },
        new { rel = "api-info", href = "/api/info", method = "GET" }
    }
}).WithTags("API Info");

Console.WriteLine("==============================================");
Console.WriteLine(" MOTTU API INICIADA COM SUCESSO!");
Console.WriteLine("==============================================");
Console.WriteLine($" URL Principal: http://localhost:5147");
Console.WriteLine($" Pagina Inicial: http://localhost:5147/");
Console.WriteLine($" Swagger Docs: http://localhost:5147/swagger");
Console.WriteLine($" Health Check: http://localhost:5147/health");
Console.WriteLine($" API Info: http://localhost:5147/api");
Console.WriteLine("==============================================");
Console.WriteLine("CREDENCIAIS PARA TESTE:");
Console.WriteLine("  Admin: usuario=admin_principal, senha=Admin123!");
Console.WriteLine("   Funcionário: usuario=funcionario_teste, senha=Func123!");
Console.WriteLine("==============================================");
Console.WriteLine("VERSOES DISPONIVEIS:");
Console.WriteLine("   • v1 - API Estavel (Producao)");
Console.WriteLine("   • v2 - Nova Versao (Recursos Avancados)");
Console.WriteLine("==============================================");
Console.WriteLine(" DICAS RAPIDAS:");
Console.WriteLine("   • Use /api/v1/... para versao estavel");
Console.WriteLine("   • Use /api/v2/... para novos recursos");
Console.WriteLine("   • Obtenha token JWT em: POST /api/v1/auth/login");
Console.WriteLine("   • Health Check mostra status em tempo real");
Console.WriteLine("   • Swagger tem exemplos de todos os endpoints");
Console.WriteLine("==============================================");

await app.RunAsync();

public partial class Program { }
