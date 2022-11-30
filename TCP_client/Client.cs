using System.Net.Sockets;
using System.Text;


namespace TCP_client;

public class Client
{
    private readonly int _port;
    private readonly string _name;
    private readonly string _host;
    
    private TcpClient _client;
    private NetworkStream _stream;
    
    public Client(string name, string host = "127.0.0.1", int port = 8888)
    {
        _name = name;
        _host = host;
        _port = port;

        _client = new TcpClient();
        _stream = null;
    }

    public void RunClient()
    {
        try
        {
            _client.Connect(_host, _port);
            _stream = _client.GetStream();

            var message = _name;
            var data = Encoding.Unicode.GetBytes(message);
            _stream.Write(data, 0, data.Length);

            var receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();
            Console.WriteLine("Welcome " + message + "!");
            SendMessage();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Disconnect();
        }
    }
    
    void SendMessage()
    {
        //Console.Write("debug");
             
        while (true)
        {
            var message = Console.ReadLine();
            var data = Encoding.Unicode.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }
    }

    private void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                var data = new byte[64];
                var builder = new StringBuilder();
                var bytes = 0;
                do
                {
                    bytes = _stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (_stream.DataAvailable);
 
                var message = builder.ToString();
                Console.WriteLine(message);
            }
            catch
            {
                Console.WriteLine("Connection is lost!");
                Console.ReadLine();
                Disconnect();
            }
        }
    }

    private void Disconnect()
    {
        _stream.Close();
        _client.Close();
        Environment.Exit(0);
    }
}