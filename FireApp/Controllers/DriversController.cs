
using FireApp.Models;
using FireApp.Services;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[Controller]")]
public class DriversController : ControllerBase
{
    private static List<Driver> drivers = new List<Driver>();

    private readonly ILogger<DriversController> _logger;

    public DriversController(ILogger<DriversController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult AddDriver(Driver driver)
    {
        if (ModelState.IsValid)
        {
            drivers.Add(driver);

            BackgroundJob.Enqueue<IServiceManagement>(c => c.SendEmail());

            return CreatedAtAction("GetDriver", new { driver.Id }, driver);
        }
        return BadRequest();
    }

    [HttpGet]
    public IActionResult GetDriver(Guid Id)
    {
        var driver = drivers.FirstOrDefault(driver => driver.Id == Id);
        if (driver == null)
        {
            return NotFound();
        }

        return Ok(driver);
    }

    [HttpDelete]
    public IActionResult DeleteDriver(Guid Id)
    {
        var driver = drivers.FirstOrDefault(driver => driver.Id == Id);
        if (driver == null)
            return NotFound();

        driver.Status = 0;

        RecurringJob.AddOrUpdate<IServiceManagement>("sync data", c => c.SyncData(), Cron.Minutely, new RecurringJobOptions() { TimeZone = TimeZoneInfo.Local });
        
        return NoContent(); ;
    }

}