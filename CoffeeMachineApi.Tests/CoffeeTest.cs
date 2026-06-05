using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CoffeeMachineAPI.Services;
using System;
using System.Text.Json;

public class CoffeeTests
{
    private readonly IConfiguration _config;
    private readonly int _code200;
    private readonly int _code503;
    private readonly int _code418;
    private readonly string _message200;
    private readonly string _message418;
    private readonly int _month;
    private readonly int _day;
    private readonly int _limit;

    public CoffeeTests()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();

        _code200 = int.Parse(_config["StaticMaintenanceVar:ResponseCode200"]);
        _code503 = int.Parse(_config["StaticMaintenanceVar:ResponseCode503"]);
        _code418 = int.Parse(_config["StaticMaintenanceVar:ResponseCode418"]);
        _message200 = _config["StaticMaintenanceVar:ResponseMessage200"];
        _message418 = _config["StaticMaintenanceVar:ResponseMessage418"];
        _month = int.Parse(_config["StaticMaintenanceVar:TeapotMonth"]);
        _day = int.Parse(_config["StaticMaintenanceVar:TeapotDay"]);
        _limit = int.Parse(_config["StaticMaintenanceVar:OutOfCoffeeCounter"]);
    }

    private CoffeeService CreateService(Func<DateTimeOffset>? nowOverride = null)
    {
        return new CoffeeService(new RequestCounter(), _config, nowOverride);
    }

    [Fact]
    public async Task Should_Return_200_With_Config_Message()
    {
        var service = CreateService();

        var result = await service.BrewCoffeeAsync();

        Assert.Equal(_code200, result.StatusCode);
        Assert.NotNull(result.Response);

        var json = JsonSerializer.Serialize(result.Response);
        var doc = JsonDocument.Parse(json);

        var message = doc.RootElement.GetProperty("message").GetString();
        var prepared = doc.RootElement.GetProperty("prepared").GetString();

        Assert.Equal(_message200, message);
        Assert.True(DateTimeOffset.TryParse(prepared, out _));
    }

    [Fact]
    public async Task Should_Return_503_On_Configured_Limit()
    {
        var service = CreateService();

        for (int i = 0; i < _limit - 1; i++)
            await service.BrewCoffeeAsync();

        var result = await service.BrewCoffeeAsync();

        Assert.Equal(_code503, result.StatusCode);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task Should_Return_418_With_Config_Message()
    {
        var service = CreateService(
            () => new DateTimeOffset(2026, _month, _day, 10, 0, 0, TimeSpan.Zero)
        );

        var result = await service.BrewCoffeeAsync();

        Assert.Equal(_code418, result.StatusCode);
        Assert.NotNull(result.Response);

        var json = JsonSerializer.Serialize(result.Response);
        var doc = JsonDocument.Parse(json);

        var message = doc.RootElement.GetProperty("message").GetString();

        Assert.Equal(_message418, message);
    }

    [Fact]
    public async Task April1_Should_Override_503()
    {
        var service = CreateService(
            () => new DateTimeOffset(2026, _month, _day, 10, 0, 0, TimeSpan.Zero)
        );

        for (int i = 0; i < _limit; i++)
            await service.BrewCoffeeAsync();

        var result = await service.BrewCoffeeAsync();

        Assert.Equal(_code418, result.StatusCode);

        var json = JsonSerializer.Serialize(result.Response);
        var doc = JsonDocument.Parse(json);

        var message = doc.RootElement.GetProperty("message").GetString();

        Assert.Equal(_message418, message);
    }
}