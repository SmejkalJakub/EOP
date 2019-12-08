namespace ITU_EOP
{
    /// <summary>
    /// Jednoduchá třída pro zasílání zpráv zkrzte Messenger díky které je možno komunikovat mezi ViewModely
    /// </summary>
    public class SimpleMessage
    {
        public SimpleMessage()
        {
        }

        /// <summary>
        /// Enum type který rozlišuje druhy zpráv
        /// </summary>
        public enum MessageType
        {
            SwitchToTargetsView,
            SwitchToApplicationListView,
            SwitchToStatisticsView,
            TimerTick,
            StopTimer,
            StartTimer,
            ApplicationDeactivated,
            ForceStopTimer,
            ForceStartTimer,
            NotInStatisticsView,
        }

        public MessageType Type { get; set; }

        public string Message { get; set; }
    }
}
