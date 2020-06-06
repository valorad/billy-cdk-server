using System;
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
            services.AddSingleton<ICDKService, CDKService>();

            // add GraphQL
            // services.AddSingleton<RootGraph>();
            services.AddSingleton<Query>();
            services.AddSingleton<ISchema>(provider => Schema.For(@"
                type Player {
                    _id: ID!
                    dbname: String!
                    isPremium: Boolean!
                    games: [String]!
                }

                input PlayerView {
                    dbname: String!
                    isPremium: Boolean!
                }

                type Query {
                    player(dbname: String!): Player
                }
            ", _ =>
            {
                _.Types.Include<Query>();
                _.ServiceProvider = provider;
            }));

            services
                .AddGraphQL((services, options) =>
                {
                    options.EnableMetrics = true;
                    options.ExposeExceptions = true;
                    var logger = services.GetRequiredService<ILogger<Startup>>();
                    options.UnhandledExceptionDelegate = ctx => logger.LogError("{Error} occured", ctx.OriginalException.Message);
                })
            .AddSystemTextJson(deserializerSettings => { }, serializerSettings => { })
            .AddDataLoader();            

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
