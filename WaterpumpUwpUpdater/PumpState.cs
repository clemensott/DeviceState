namespace WaterpumpUwpUpdater
{
    struct PumpState
    {
       public bool IsOn { get; set; }

        public int RemainingSeconds { get; set; }

        public string Temperature { get; set; }
    }
}
