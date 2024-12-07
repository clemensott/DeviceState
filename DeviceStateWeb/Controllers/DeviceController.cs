using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DeviceStateWeb.Models;
using DeviceStateWeb.Models.Exceptions;
using DeviceStateWeb.Services.Devices;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace DeviceStateWeb.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly IDevicesService deviceService;

        public DeviceController(IDevicesService deviceService)
        {
            this.deviceService = deviceService;
        }

        [HttpGet("on")]
        public async Task<ActionResult> TurnOn([FromQuery] string id, [FromQuery] double? millis,
            [FromQuery] double? minutes, [FromQuery] bool defaultTime)
        {
            try
            {
                await deviceService.TurnOn(id, millis, minutes, defaultTime);

                return Ok();
            }
            catch (DeviceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("off")]
        public async Task<ActionResult> TurnOff([FromQuery] string id)
        {
            try
            {
                await deviceService.TurnOff(id);

                return Ok();
            }
            catch (DeviceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("ison")]
        public async Task GetIsOn(string id = null, int? errors = null,
            int? state = null, int? value = null, int? maxWaitMillis = null)
        {
            try
            {
                Console.WriteLine($"GetIsOn1: id={id} errors={errors} state={state} value={value} maxWaitMillis={maxWaitMillis}");
                TimeSpan? maxWaitTime = maxWaitMillis.HasValue ? TimeSpan.FromMilliseconds(maxWaitMillis.Value) : null;
                Task<bool> task = deviceService.SetMeasurements(id, errors, state, value, maxWaitTime);

                Response.StatusCode = (int)HttpStatusCode.OK;
                while (!task.IsCompleted) {
                    await Response.WriteAsync(".");
                    await Task.Delay(100);
                }

                bool response = await task;
                Console.WriteLine($"GetIsOn2: aborted={HttpContext.RequestAborted.IsCancellationRequested} response={response}");

                await Response.WriteAsync(response.ToString());
            }
            catch (DeviceNotFoundException)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        [HttpGet("remaining")]
        public async Task<ActionResult<int>> GetRemaining([FromQuery] string id = null)
        {
            try
            {
                TimeSpan? remaining = await deviceService.GetRemainingOnTime(id);

                return (int)(remaining?.TotalMilliseconds ?? 0);
            }
            catch (DeviceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("state")]
        public async Task<ActionResult<DeviceState>> GetState([FromQuery] string id = null, [FromQuery] int? maxWaitMillis = null)
        {
            try
            {
                TimeSpan? maxWaitTime = maxWaitMillis.HasValue ? TimeSpan.FromMilliseconds(maxWaitMillis.Value) : null;
                return await deviceService.GetState(id, maxWaitTime);
            }
            catch (DeviceNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
