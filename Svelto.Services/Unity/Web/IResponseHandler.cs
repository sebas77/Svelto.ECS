namespace Svelto.Services
{
    public interface IResponseHandler<ResponseType> : IResponseHandler
    {
        ResponseType response { get; }
    }

    public interface IResponseHandler
    {
        // Called once per frame when data has been received from the network.
        bool ReceiveData(byte[] data, int dataLength);

        // Called when all data has been received from the server and delivered via ReceiveData.
        void CompleteContent();

        // Called when a Content-Length header is received from the server.
        void ReceiveContentLength(int contentLength);
    }
}