using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterpumpWeb.Models;
using WaterpumpWeb.Models.Exceptions;
using WaterpumpWeb.Services.Devices;

namespace WaterpumpWeb.Controllers
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
        public async Task<ActionResult> TurnOn([FromQuery] string id, [FromQuery] double? millis, [FromQuery] double? time)
        {
            try
            {
                await deviceService.TurnOn(id, millis, time);

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
        public async Task<ActionResult<string>> GetIsOn(string id = null, int? errors = null, 
            int? state = null, int? value = null, int? maxWaitMillis = null)
        {
            try
            {
                TimeSpan? maxWaitTime = maxWaitMillis.HasValue ? TimeSpan.FromMilliseconds(maxWaitMillis.Value) : null;
                bool response = await deviceService.SetMeasurements(id, errors, state, value, maxWaitTime);

                return response.ToString();
            }
            catch (DeviceNotFoundException)
            {
                return NotFound();
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
