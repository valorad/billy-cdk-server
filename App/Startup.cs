using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Controllers.Graphs;
using App.Database;
using App.Models;
using App.Services;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using App.Lib;
using GraphQL;

namespace App
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      // add secrets
      services.Configure<DBConfig>(
        Configuration.GetSection("mongo")
      );

      services.AddSingleton<IDBConfig>(sp =>
        sp.GetRequiredService<IOptions<DBConfig>>().Value
      );

      // configure db
      services.AddTransient<IDBContext, DBAccess>();
      services.AddTransient<IDBCollection, DBCollection>();

      // add services
      services.AddSingleton<IPlayerService, PlayerService>();
      services.AddSingleton<IGameService, GameService>();
      services.AddSingleton<ICDKeyService, CDKeyService>();

      // add GraphQL
      services.AddSingleton<PlayerGraph>();
      services.AddSingleton<GameGraph>();
      services.AddSingleton<CDKeyGraph>();
      services.AddSingleton<Query>();
      services.AddSingleton<Mutation>();
      services.AddSingleton<JsonGraphType>();
      services.AddSingleton<ISchema>(
        (provider) =>
        {
          var schema = Schema.For(Graph.LoadDefinitions(), _ =>
            {
              _.Types.Include<Query>();
              _.Types.Include<Mutation>();
              _.ServiceProvider = provider;
            }
          );
          schema.RegisterValueConverter(new JsonGraphTypeConverter());
          return schema;
        }


      );

      services.AddCors(options =>
      {
        options.AddPolicy("policy0", builder =>
        {
          builder.AllowAnyHeader()
                    .WithMethods("GET", "POST")
                    .WithOrigins("*")
                    .SetIsOriginAllowedToAllowWildcardSubdomains();;
        });
      });

      services
        .AddGraphQL((options, provider) =>
        {
            options.EnableMetrics = false;
            options.ExposeExceptions = true;
            var logger = provider.GetRequiredService<ILogger<Startup>>();
            options.UnhandledExceptionDelegate = ctx => logger.LogError("{Error} occured", ctx.OriginalException.Message);
        })
        .AddSystemTextJson(deserializerSettings => { }, serializerSettings => { })
        .AddDataLoader();

      services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

      string basePath = Configuration.GetSection("basePath").Value;

      if (basePath is null)
      {
        basePath = "/";
      }

      app.UsePathBase(basePath);

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseCors("policy0");

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });

      // use graphQL
      app.UseGraphQL<ISchema>("/gql");

    }
  }
}
