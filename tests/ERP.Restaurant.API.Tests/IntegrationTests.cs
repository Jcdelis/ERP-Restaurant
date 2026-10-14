using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ERP.Restaurant.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Restaurant.API.Tests;

public class IntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public IntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MesasEndpoint_Host_ReturnsOk()
    {
        using var factory = _fixture.Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real DB with InMemory
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb_Mesas_Host"));
            });
        });

        using var client = factory.CreateClient();

        // Add Authorization header with valid JWT token
        var token = _fixture.GenerateTestToken("ADMIN", "test@example.com");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var resp = await client.GetAsync("/api/Mesas");
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProductosEndpoint_Host_ReturnsOk()
    {
        using var factory = _fixture.Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb_Productos_Host"));
            });
        });

        using var client = factory.CreateClient();
        var resp = await client.GetAsync("/api/Productos");
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
