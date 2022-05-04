using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer;

internal class ServerObject
{
    private static TcpListener _tcpListener;
    private readonly List<ClientObject> _clients = new();

    protected internal void AddConnection(ClientObject clientObject)
    {
        _clients.Add(clientObject);
    }

    protected internal void RemoveConnection(string id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id);
        if (client != null)
            _clients.Remove(client);
    }

    protected internal void Listen()
    {
        try
        {
            _tcpListener = new TcpListener(IPAddress.Any, 8888);
            _tcpListener.Start();
            Console.WriteLine("Server is running. Waiting for connections...");

            while (true)
            {
                var tcpClient = _tcpListener.AcceptTcpClient();

                var clientObject = new ClientObject(tcpClient, this);
                var clientThread = new Thread(clientObject.Process);
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Disconnect();
        }
    }

    protected internal void BroadcastMessage(string message, string id)
    {
        var data = Encoding.Unicode.GetBytes(message);
        for (var i = 0; i < _clients.Count; i++)
            if (_clients[i].Id != id)
                _clients[i].Stream.Write(data, 0, data.Length);
    }

    protected internal void Disconnect()
    {
        _tcpListener.Stop();

        for (var i = 0; i < _clients.Count; i++) _clients[i].Close();
        Environment.Exit(0);
    }
}