using BillyCDK.App.Graphs;
using BillyCDK.DataAccess.Models;
using BillyCDK.DataAccess.Utilities;
using GraphQL;
using GraphQL.Types;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Query>();
builder.Services.AddSingleton<Mutation>();

builder.Services.AddSingleton<ISchema>((provider) =>
{
    var schema = Schema.For(GraphUtils.LoadDefinitions(), _ =>
    {
        _.Types.Include<Query>();
        _.Types.Include<Mutation>();
        _.ServiceProvider = provider;
    }
    );
    return schema;
}
);

builder.Services.AddGraphQL(gqlBuilder =>
    gqlBuilder
        .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true)
        .AddSystemTextJson()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseWebSockets();

app.UseGraphQL<ISchema>("/graphql");


app.MapGet("/api", () =>
{
    return new InstanceMessage<object>(
        Okay: 1,
        NumAffected: 0,
        Message: "API Works!",
        Instances: null
    );
})
.WithName("Ready State")
.WithOpenApi();

app.Run();
