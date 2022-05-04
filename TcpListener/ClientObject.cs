using System.Net.Sockets;
using System.Text;

namespace ChatServer;

internal class ClientObject
{
    private readonly TcpClient _client;
    private readonly ServerObject _server;
    private string _userName;

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        Id = Guid.NewGuid().ToString();
        _client = tcpClient;
        _server = serverObject;
        serverObject.AddConnection(this);
    }

    protected internal string Id { get; }
    protected internal NetworkStream Stream { get; private set; }

    public void Process()
    {
        try
        {
            Stream = _client.GetStream();
            var message = GetMessage();
            _userName = message;

            message = _userName + " joined the chat";
            _server.BroadcastMessage(message, Id);
            Console.WriteLine(message);
            while (true)
                try
                {
                    message = GetMessage();
                    message = string.Format("{0}: {1}", _userName, message);
                    Console.WriteLine(message);
                    _server.BroadcastMessage(message, Id);
                }
                catch
                {
                    message = string.Format("{0}: left the chat", _userName);
                    Console.WriteLine(message);
                    _server.BroadcastMessage(message, Id);
                    break;
                }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            _server.RemoveConnection(Id);
            Close();
        }
    }

    private string GetMessage()
    {
        var data = new byte[64];
        var builder = new StringBuilder();
        var bytes = 0;

        do
        {
            bytes = Stream.Read(data, 0, data.Length);
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        } while (Stream.DataAvailable);

        return builder.ToString();
    }

    protected internal void Close()
    {
        if (Stream != null)
            Stream.Close();
        if (_client != null)
            _client.Close();
    }
}