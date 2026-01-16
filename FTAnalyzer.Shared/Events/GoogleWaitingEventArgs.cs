namespace FTAnalyzer.Events
{
    public class GoogleWaitingEventArgs(string message) : EventArgs
    {
        public string Message { get; private set; } = message;
    }
}
