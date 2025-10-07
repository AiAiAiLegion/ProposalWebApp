using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.EntityFrameworkCore;
using ProposalWebApp.Data;
using ProposalWebApp.Facades;
using ProposalWebApp.Models;
using ProposalWebApp.Services;
using ProposalWebApp.Services.Lookup;
using ProposalWebApp.Services.Ui;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- ЛОГГИРОВАНИЕ ---
builder.Logging.AddConfiguration(               // подключаем appsettings.json
    builder.Configuration.GetSection("Logging")
);
builder.Logging.AddConsole();                   // вывод в консоль
builder.Logging.AddDebug();                     // вывод в Output (VS/Rider)
builder.Logging.AddEventSourceLogger();

// --- Blazorise ---
builder.Services
    .AddBlazorise(options => { options.Immediate = true; })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

// --- DbContext с логгингом EF ---
builder.Services.AddDbContextFactory<ProposalDbContext>(options =>
{
    var provider = builder.Configuration.GetSection("Database").GetValue<string>("Provider");
    var connectionString = builder.Configuration.GetSection("Database").GetValue<string>("ConnectionString");

    switch (provider?.ToLower())
    {
        case "postgresql":
            options.UseNpgsql(connectionString);
            SearchProvider.Configure
            (
                // proposals
                (pq, search) => pq.Where(p =>
                    EF.Functions.ToTsVector("simple",
                        (p.Author ?? "") + " " + (p.Department ?? "") + " " + (p.Number.ToString() ?? "") + " " + p.CreatedAt.ToString())
                    .Matches(EF.Functions.PlainToTsQuery("simple", search))),

                // materials
                (mq, search) => mq.Where(m =>
                    EF.Functions.ToTsVector("simple",
                        (m.Name ?? "") + " " + (m.Code ?? "") + " " + (m.Comment ?? ""))
                    .Matches(EF.Functions.PlainToTsQuery("simple", search)))
            );
            break;

        case "mssql":
            options.UseSqlServer(connectionString);
            SearchProvider.Configure
            (
                (pq, search) => pq.Where(p =>
                    EF.Functions.FreeText(
                        (p.Author ?? "") + " " + (p.Department ?? "") + " " + (p.Number.ToString() ?? "") + " " + p.CreatedAt.ToString(), search)),

                (mq, search) => mq.Where(m =>
                    EF.Functions.FreeText(
                        (m.Name ?? "") + " " + (m.Code ?? "") + " " + (m.Comment ?? ""), search))
            );
            break;

        case "mysql":
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            SearchProvider.Configure
            (
               (pq, search) => pq.Where(p =>
                   (p.Author ?? "").Contains(search) ||
                   (p.Department ?? "").Contains(search) ||
                   (p.Number.ToString() ?? "").Contains(search)),

               (mq, search) => mq.Where(m =>
                   (m.Name ?? "").Contains(search) ||
                   (m.Code ?? "").Contains(search) ||
                   (m.Comment ?? "").Contains(search))
            );
            break;

        default:
            throw new InvalidOperationException($"Неподдерживаемый провайдер БД: {provider}");
    }

    //  ключевые фичи EF Core
    options.EnableSensitiveDataLogging();   // показывать параметры SQL
    options.EnableDetailedErrors();         // подробные ошибки EF
    options.LogTo(Console.WriteLine, LogLevel.Information); // SQL-запросы в консоль
});

// --- Сервисы ---
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IProposalLookupService, ProposalLookupService>();
builder.Services.AddScoped<IMaterialLookupService, MaterialLookupService>();

// --- Фасады ---
builder.Services.AddScoped<IProposalsFacade, ProposalsFacade>();
builder.Services.AddScoped<IProposalDetailsFacade, ProposalDetailsFacade>();
builder.Services.AddScoped<IMaterialsFacade, MaterialsFacade>();

// --- Уведомления ---
builder.Services.AddScoped<IUiNotificationService, UiNotificationService>();

// --- Razor & Blazor ---
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// --- Автосоздание БД ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProposalDbContext>();
    db.Database.EnsureCreated();
}


// --- Лог о старте ---
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(" Приложение запущено. Среда: {Env}", app.Environment.EnvironmentName);

// --- Middleware ошибок ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Необработанное исключение");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Произошла ошибка. Попробуйте позже.");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

