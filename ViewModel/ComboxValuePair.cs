namespace DeviceMonitor.ViewModel
{
    public class ComboxValuePair
    {
        public string DisplayText { get; private set; }
        public int IntValue { get; private set; }

        public ComboxValuePair(string text, int intValue)
        {
            DisplayText = text;
            IntValue = intValue;
        }
    }
}