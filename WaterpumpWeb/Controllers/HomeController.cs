using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;
using WaterpumpWeb.Models;
using WaterpumpWeb.Models.Exceptions;
using WaterpumpWeb.Models.ViewModels;
using WaterpumpWeb.Services.Devices;

namespace WaterpumpWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDevicesService devicesService;

        [ViewData]
        public HtmlBackground Background { get; }

        public HomeController(IDevicesService devicesService)
        {
            this.devicesService = devicesService;
            Background = HtmlBackgroundHelper.GetRandomBackground();
        }

        private async Task<DeviceStateModel> GetDeviceStateModel(string id, int? maxWaitMillis)
        {
            TimeSpan? maxWaitTime = maxWaitMillis.HasValue ? TimeSpan.FromMilliseconds(maxWaitMillis.Value) : null;
            DeviceState state = await devicesService.GetState(id, maxWaitTime);
            return new DeviceStateModel(state.Id, state.Name, state.OnState, state.ActoOnlineState, state.Value);
        }

        [HttpGet]
        public Task<IActionResult> Index()
        {
            string deviceId = null;
            if (Request.Headers.TryGetValue("X-Device-Id", out StringValues values))
            {
                deviceId = values.FirstOrDefault();
            }
            return Index(deviceId);
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> Index(string deviceId = null)
        {
            try
            {
                DeviceStateModel deviceStateModel = await GetDeviceStateModel(deviceId, null);
                DeviceTurnOnOffModel deviceTurnOnOffModel = new DeviceTurnOnOffModel(5);
                HomeModel homeModel = new HomeModel(deviceStateModel, deviceTurnOnOffModel);
                return View(homeModel);
            }
            catch (DeviceNotFoundException e)
            {
                DeviceNotFoundModel deviceNotFoundModel = new DeviceNotFoundModel(e.DeviceId);
                return View("DeviceNotFound", deviceNotFoundModel);
            }
        }

        [HttpGet("htmx/deviceState")]
        public async Task<IActionResult> HtmxDeviceState([FromQuery] string id, [FromQuery] int? maxWaitMillis = null)
        {
            DeviceStateModel deviceStateModel = await GetDeviceStateModel(id, maxWaitMillis);
            return View("DeviceState", deviceStateModel);
        }
    }
}
