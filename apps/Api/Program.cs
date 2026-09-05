using AgentCore.Approval;
using AgentCore.LLM;
using LLM.Ilmu;
using AgentCore.Policy;
using AgentCore.Tools;
using AgentRuntime;
using AgentRuntime.Approval;
using AgentRuntime.Decisions;
using AgentRuntime.Execution;
using AgentRuntime.Tools;
using Api.Channels;
using Api.Channels.Telegram;
using Policy;
using Tools;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agent runtime
builder.Services.AddScoped<AgentDecisionParser>();
builder.Services.AddScoped<AgentDecisionValidator>();
builder.Services.AddScoped<ActionExecutionService>();
builder.Services.AddScoped<ToolExecutor>();

// Approval
builder.Services.AddSingleton<IApprovalService, InMemoryApprovalService>();

// Policy
builder.Services.AddScoped<IPolicyEngine, ActionPolicy>();

// Tools
builder.Services.AddSingleton<ToolRegistry>(sp =>
{
    var registry = new ToolRegistry();

    registry.Register(
        new LookupOrderTool());

    return registry;
});

builder.Services.AddSingleton<IToolRegistry>(
    sp => sp.GetRequiredService<ToolRegistry>());

// Orchestrator
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Channels
builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection("Telegram"));

builder.Services.AddHttpClient<TelegramChannel>(client =>
{
    client.BaseAddress =
        new Uri("https://api.telegram.org/");
});

builder.Services.AddScoped<ChannelMessageProcessor>();

// LLM provider (ILMU)
builder.Services.Configure<IlmuOptions>(
    builder.Configuration.GetSection("Ilmu"));

builder.Services.AddHttpClient<ILLMProvider, IlmuProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ilmu API diagnostic
//var ilmuKey = Environment.GetEnvironmentVariable("ILMU__APIKEY");

//Console.WriteLine(
//    $"ILMU key loaded: {!string.IsNullOrWhiteSpace(ilmuKey)}");

//Console.WriteLine(
//    $"ILMU key prefix: {ilmuKey?[..Math.Min(7, ilmuKey.Length)]}");

//var model = Environment.GetEnvironmentVariable("ILMU__MODEL");

//Console.WriteLine(
//    $"ILMU model loaded: {!string.IsNullOrWhiteSpace(model)}");

//Console.WriteLine(
//    $"ILMU model: {model}");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();