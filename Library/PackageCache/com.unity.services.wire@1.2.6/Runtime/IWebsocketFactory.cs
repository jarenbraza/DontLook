namespace Unity.Services.Wire.Internal
{
    interface IWebsocketFactory
    {
        IWebSocket CreateInstance(string url);
    }
}
