using System.Net.Sockets;
using System.Text;

namespace ChatClient;

internal class Program
{
    private const string _host = "127.0.0.1";
    private const int _port = 8888;
    private static string _userName;
    private static TcpClient _client;
    private static NetworkStream _stream;

    private static void Main(string[] args)
    {
        Console.Write("Enter Name: ");
        _userName = Console.ReadLine();
        _client = new TcpClient();
        try
        {
            _client.Connect(_host, _port);
            _stream = _client.GetStream();

            var message = _userName;
            var data = Encoding.Unicode.GetBytes(message);
            _stream.Write(data, 0, data.Length);

            var receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();
            Console.WriteLine("Welcome, {0}", _userName);
            SendMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    private static void SendMessage()
    {
        Console.WriteLine("Enter message: ");

        while (true)
        {
            var message = Console.ReadLine();
            var data = Encoding.Unicode.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }
    }

    private static void ReceiveMessage()
    {
        while (true)
            try
            {
                var data = new byte[64];
                var builder = new StringBuilder();
                var bytes = 0;
                do
                {
                    bytes = _stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (_stream.DataAvailable);

                var message = builder.ToString();
                Console.WriteLine(message);
            }
            catch
            {
                Console.WriteLine("Connection interrupted!");
                Console.ReadLine();
                Disconnect();
            }
    }

    private static void Disconnect()
    {
        if (_stream != null)
            _stream.Close();
        if (_client != null)
            _client.Close();
        Environment.Exit(0);
    }
}