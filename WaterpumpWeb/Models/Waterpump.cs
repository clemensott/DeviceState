using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace WaterpumpWeb.Models
{
    public class Waterpump
    {
        private static Waterpump instance;

        public static async Task<Waterpump> GetInstance()
        {
            if (instance == null)
            {
                Waterpump newInstance = new Waterpump();
                await newInstance.LoadState();

                instance = newInstance;
            }

            return instance;
        }

        public bool IsOn => IsForeverOn || DateTime.Now < IsOnUntil;

        public bool IsForeverOn { get; private set; }

        public DateTime IsOnUntil { get; private set; }

        public TimeSpan Remaining => IsOnUntil - DateTime.Now;

        public Waterpump()
        {
        }

        private async Task LoadState()
        {
            const string sql = "SELECT is_forever_on, is_on_until FROM waterpump LIMIT 1";

            IDataRecord data = await DbHelper.ExecuteReadFirstAsync(sql);
            IsForeverOn = data.GetInt64(data.GetOrdinal("is_forever_on")) != 0;
            IsOnUntil = DateTime.Parse(data.GetString(data.GetOrdinal("is_on_until")));
        }

        public Task TurnOn()
        {
            IsForeverOn = true;
            IsOnUntil = DateTime.MinValue;

            return UpdateDatabase();
        }

        public Task TurnOn(TimeSpan time)
        {
            IsForeverOn = false;
            IsOnUntil = DateTime.Now + time;

            return UpdateDatabase();
        }

        public Task TurnOff()
        {
            IsForeverOn = false;
            IsOnUntil = DateTime.MinValue;

            return UpdateDatabase();
        }

        public Task UpdateDatabase()
        {
            const string sql = "UPDATE waterpump SET is_forever_on = @isForeverOn, is_on_until = @isOnUntil WHERE TRUE;";

            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@isForeverOn", IsForeverOn),
                new KeyValuePair<string, object>("@isOnUntil", IsOnUntil),
            };

            return DbHelper.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
