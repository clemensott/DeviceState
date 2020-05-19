using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterpumpWeb.Models;

namespace WaterpumpWeb.Controllers
{
    [Route("api/[controller]")]
    [Route("wasserpumpe")]
    public class WaterpumpController : Controller
    {
        private static Waterpump waterpump;

        private static async Task<Waterpump> GetWaterpump()
        {
            if (waterpump == null) waterpump = await Waterpump.GetInstance();

            return waterpump;
        }

        [HttpGet("on")]
        public async Task<ActionResult> TurnOn([FromQuery] double? millis, [FromQuery] double? time, [FromQuery] int id)
        {
            Waterpump waterpump = await GetWaterpump();
            if (millis.HasValue)
            {
                TimeSpan onTime = millis < 0 ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(millis.Value);
                await waterpump.TurnOn(onTime);
            }
            else if (time.HasValue) await waterpump.TurnOn(TimeSpan.FromMinutes(time.Value));
            else await waterpump.TurnOn();

            return Ok();
        }

        [HttpGet("off")]
        public async Task<ActionResult> TurnOff()
        {
            Waterpump waterpump = await GetWaterpump();
            await waterpump.TurnOff();

            return Ok();
        }

        [HttpGet("ison")]
        public async Task<ActionResult<string>> GetIsOn([FromQuery] int? id = null, int? errors = null, int? state = null, int? temp = null)
        {
            Waterpump waterpump = await GetWaterpump();
            IsOnRequest request = new IsOnRequest(id, errors, state, temp, waterpump.IsOn);

            const string insertSql =
            @"
                INSERT INTO is_on_requests (device_id, error_count, pump_state, raw_temp, response, created)
                VALUES (@deviceId, @errorCount, @pumpState, @rawTemp, @response, @created);
            ";

            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[]{
                new KeyValuePair<string, object>("@deviceId", request.DeviceId),
                new KeyValuePair<string, object>("@errorCount", request.ErrorCount),
                new KeyValuePair<string, object>("@pumpState", request.PumpState),
                new KeyValuePair<string, object>("@rawTemp", request.RawTemp),
                new KeyValuePair<string, object>("@response", request.Response),
                new KeyValuePair<string, object>("@created", request.Created),
            };

            await DbHelper.ExecuteNonQueryAsync(insertSql, parameters);

            const string deleteSql =
            @"
                DELETE
                FROM is_on_requests
                WHERE id NOT IN (SELECT id FROM is_on_requests ORDER BY created DESC LIMIT 10000);
            ";
            await DbHelper.ExecuteNonQueryAsync(deleteSql);

            return request.Response.ToString();
        }

        [HttpGet("remaining")]
        public async Task<ActionResult<int>> GetRemaining()
        {
            Waterpump waterpump = await GetWaterpump();
            return (int)waterpump.Remaining.TotalMilliseconds;
        }

        [HttpGet("state")]
        public async Task<ActionResult<PumpState>> GetState([FromQuery] int? deviceId = null)
        {
            Waterpump waterpump = await GetWaterpump();
            double remainingOnMillis = waterpump.Remaining.TotalMilliseconds;
            IsOnRequest[] requestsWithID = await GetIsOnRequests(deviceId);

            if (requestsWithID.Length == 0)
            {
                return new PumpState(null, remainingOnMillis, null, Temperature.Empty);
            }

            IsOnRequest request = requestsWithID.First();

            IsOnRequest[] requestsWithIdAndTemp = requestsWithID.Where(r => r.RawTemp != -1 && (DateTime.Now - r.Created).TotalSeconds < 15).ToArray();
            double averageRawTemp = requestsWithIdAndTemp.Length > 0 ? requestsWithIdAndTemp.Average(r => r.RawTemp.Value) : -1;

            bool? isOn = request.PumpState.HasValue ? (bool?)(request.PumpState != 0) : null;
            double lastUpdateMillisAgo = (DateTime.Now - request.Created).TotalMilliseconds;
            Temperature temp = await GetTemp(averageRawTemp);

            return new PumpState(isOn, remainingOnMillis, lastUpdateMillisAgo, temp);
        }


        private async static Task<IsOnRequest[]> GetIsOnRequests(int? deviceId)
        {
            string sql;
            KeyValuePair<string, object>[] parameters;

            if (deviceId.HasValue)
            {
                sql =
                @"
                    SELECT id, device_id, error_count, pump_state, raw_temp, response, created
                    FROM is_on_requests
                    WHERE device_id = @deviceId AND raw_temp IS NOT NULL
                    ORDER BY created DESC;
                ";
                parameters = new KeyValuePair<string, object>[] {
                    new KeyValuePair<string, object>("@deviceId", deviceId),
                };
            }
            else
            {
                sql =
                @"
                    SELECT id, device_id, error_count, pump_state, raw_temp, response, created
                    FROM is_on_requests
                    WHERE raw_temp IS NOT NULL
                    ORDER BY created DESC;
                ";
                parameters = null;
            }

            return (await DbHelper.ExecuteReadAllAsync(sql, parameters))
                .Select(IsOnRequest.FromDataRecord).ToArray();
        }

        private static async Task<Temperature> GetTemp(double rawTemp)
        {
            if (rawTemp < 0) return Temperature.Empty;

            try
            {
                double lineVoltage = -1, lineTemp = -1;
                string[] lines = await System.IO.File.ReadAllLinesAsync(@"TempPoints.txt");
                (double raw, double temp)[] tuples = lines.Where(l => IsTempPointLine(l, out lineVoltage, out lineTemp))
                    .Select(l => (raw: lineVoltage, temp: lineTemp)).OrderBy(t => t.raw).ToArray();

                if (tuples.Select(t => t.raw).Distinct().Count() < 2) return Temperature.Empty;

                if (!tuples.Any(t => t.raw <= rawTemp)) return Temperature.FromSmallerThan(tuples.Min(t => t.temp));
                if (!tuples.Any(t => t.raw > rawTemp)) return Temperature.FromGreaterThan(tuples.Max(t => t.temp));

                (double beforeVoltage, double beforeTemp) = tuples.Last(t => t.raw <= rawTemp);
                (double afterVoltage, double afterTemp) = tuples.First(t => t.raw > rawTemp);
                double relVoltage = (rawTemp - beforeVoltage) / (afterVoltage - beforeVoltage);

                return Temperature.FromValue((afterTemp - beforeTemp) * relVoltage + beforeTemp);
            }
            catch
            {
                return Temperature.Empty;
            }
        }

        private static bool IsTempPointLine(string line, out double raw, out double temp)
        {
            raw = -1;
            temp = -1;

            string[] parts = line.Split('=');

            return parts.Length == 2 && double.TryParse(parts[0], out raw) && double.TryParse(parts[1], out temp);
        }

    }
}
