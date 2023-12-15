using Microsoft.AspNetCore.Mvc;
using southafricantaxtool.API.Models;
using southafricantaxtool.API.Models.Events.ImportantDates;
using southafricantaxtool.Interface.Models;
using southafricantaxtool.Interface.Services;

namespace southafricantaxtool.API.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController(ILogger<EventsController> logger, 
    IStore<ImportantDate> importantDateStore) : ControllerBase
{
    [HttpPost("ImportantDates")]
    public async Task<IActionResult> ImportantDates([FromBody] ImportantDatesInput input)
    {
        try
        {
            Func<ImportantDate, bool>? filter = input switch
            {
                { StartDate: not null, EndDate: not null } when input.EndDate < input.StartDate =>
                    throw new InvalidOperationException("Start date cannot be before the end date"),
                { StartDate: not null, EndDate: not null } => date =>
                    date.Start >= input.StartDate && date.Start <= input.EndDate,
                { StartDate: not null, EndDate: null } or { StartDate: null, EndDate: not null } =>
                    throw new InvalidOperationException("Both dates or no dates needs to be specified."),
                _ => null
            };

            var dates = await importantDateStore.GetAsync(filter);

            return Ok(new GenericResponseModel<List<ImportantDate>>
            {
                Success = true,
                Message = "Retrieved important dates successfully",
                Data = dates
            });
        }
        catch (InvalidOperationException op)
        {
            return BadRequest(new GenericResponseModel<ImportantDatesOutput>
            {
                Success = false,
                Message = op.Message
            });
        }
    }
}