using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace WaterpumpUwpUpdater
{
    public sealed class TileUpdateBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            System.Diagnostics.Debug.WriteLine("Run backgroundtask");
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            PumpState? state = await GetPumpState();
            if (state.HasValue) UpdateTile(state.Value);

            deferral.Complete();
        }

        private static async Task<PumpState?> GetPumpState()
        {
            try
            {
                string content;
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync("http://nas-server/wasserpumpe/state?id-1"))
                    {
                        if (!response.IsSuccessStatusCode) return null;
                        content = await response.Content.ReadAsStringAsync();
                    }
                }

                dynamic obj = JsonConvert.DeserializeObject(content);
                return new PumpState()
                {
                    IsOn = Convert.ToBoolean((int)obj.state),
                    RemainingSeconds = (int)obj.seconds,
                    Temperature = (string)obj.temp,
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        private static void UpdateTile(PumpState state)
        {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
            
            string titleText = state.Temperature;
            tileXml.GetElementsByTagName(textElementName)[0].InnerText = titleText;

            // Create a new tile notification.
            updater.Update(new TileNotification(tileXml));
        }

        private const string textElementName = "text";
    }
}
