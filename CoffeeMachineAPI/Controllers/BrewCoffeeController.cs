using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("brew-coffee")]
public class BrewCoffeeController : ControllerBase
{
    private readonly CoffeeService _coffeeService;

    public BrewCoffeeController(CoffeeService coffeeService)
    {
        _coffeeService = coffeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _coffeeService.BrewCoffeeAsync();

        if (result.Response == null)
            return StatusCode(result.StatusCode);

        return StatusCode(result.StatusCode, result.Response);
    }
}