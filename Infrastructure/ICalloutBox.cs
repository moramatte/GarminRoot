namespace Infrastructure
{
    public interface ICalloutBox
    {
        void Callout(string caption, string message);
    }

    public interface IClipboardSender
    {
        void ToClipboard(string text);
    }
}