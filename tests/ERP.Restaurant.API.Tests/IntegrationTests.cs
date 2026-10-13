using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ERP.Restaurant.Infrastructure.Data;
using ERP.Restaurant.API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Restaurant.API.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task MesasController_Get_ReturnsList()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Mesas")
            .Options;

        using var context = new AppDbContext(options);
        context.Mesas.Add(new ERP.Restaurant.Core.Entities.Mesa { Id = 1, Numero = "1", Estado = "LIBRE" });
        context.Mesas.Add(new ERP.Restaurant.Core.Entities.Mesa { Id = 2, Numero = "2", Estado = "OCUPADA" });
        await context.SaveChangesAsync();

        var controller = new MesasController(context);
        var result = await controller.Get();

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeAssignableTo<System.Collections.IEnumerable>();
    }
}
