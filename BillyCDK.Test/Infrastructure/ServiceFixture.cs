﻿using BillyCDK.DataAccess.Database;
using BillyCDK.DataAccess.Models;
using BillyCDK.DataAccess.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BillyCDK.Test.Infrastructure;

public class ServiceFixture
{
    public IHost TestHost { get; set; }

    public ServiceFixture()
    {
        var builder = Host.CreateDefaultBuilder();

        TestHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // always read user secrets, simpler for development
                config.AddUserSecrets<ServiceFixture>();
            })
            .ConfigureServices((context, services) =>
            {
                // configure db
                services.AddSingleton<IDBContext, DBContext>();
                services.AddSingleton<IDBCollection, DBCollection>();

                // add services
                services.AddSingleton<IPlayerService, PlayerService>();
                services.AddSingleton<IGameService, GameService>();
                services.AddSingleton<ICDKeyService, CDKeyService>();
            })
            .Build();
    }
}