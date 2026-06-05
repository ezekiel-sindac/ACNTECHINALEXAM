using CoffeeMachineAPI.Services;
using Microsoft.Extensions.Configuration;

public class CoffeeService
{
    private readonly IRequestCounter _counter;
    private readonly IConfiguration _config;
    private readonly Func<DateTimeOffset> _now;

    public CoffeeService(IRequestCounter counter, IConfiguration configuration, Func<DateTimeOffset>? nowOverride = null)
    {
        _counter = counter;
        _config = configuration;
        _now = nowOverride ?? (() => DateTimeOffset.Now);
    }



    public async Task<(int StatusCode, object? Response)> BrewCoffeeAsync()
    {
        var now = _now();
        int teapotMonth = _config.GetValue<int>("StaticMaintenanceVar:TeapotMonth");
        int teapotDay = _config.GetValue<int>("StaticMaintenanceVar:TeapotDay");

        int code418 = _config.GetValue<int>("StaticMaintenanceVar:ResponseCode418");
        int code503 = _config.GetValue<int>("StaticMaintenanceVar:ResponseCode503");

        int limit = _config.GetValue<int>("StaticMaintenanceVar:OutOfCoffeeCounter");

        string message418 = _config.GetValue<string>("StaticMaintenanceVar:ResponseMessage418");
        string message200 = _config.GetValue<string>("StaticMaintenanceVar:ResponseMessage200");
        

        // April 1 check
        if (now.Month == teapotMonth && now.Day == teapotDay)
        {
            var resp = new
            {
                message = message418,
                prepared = _now().ToString("yyyy-MM-ddTHH:mm:sszzz")
            };

            return (code418, resp);
        }

        int count = _counter.Increment();

        // Every 5th call
        if (count % limit == 0)
            return (code503, null);

        var response = new
        {
            message = message200,
            prepared = _now().ToString("yyyy-MM-ddTHH:mm:sszzz")
        };

        return (200, response);
    }
}