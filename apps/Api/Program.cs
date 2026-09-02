using AgentCore.Approval;
using AgentCore.LLM;
using AgentCore.Policy;
using AgentCore.Tools;
using AgentRuntime;
using AgentRuntime.Approval;
using AgentRuntime.Decisions;
using AgentRuntime.Execution;
using AgentRuntime.Tools;
using Policy;
using Tools;

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

// LLM provider
// Register the YTL Ilmu implementation here once it exists.
// builder.Services.AddHttpClient<ILLMProvider, IlmuProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();