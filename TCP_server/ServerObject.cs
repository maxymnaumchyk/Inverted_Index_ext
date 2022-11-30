using System.Net;
using System.Net.Sockets;
using System.Text;


namespace TCP_server;

public class ServerObject
{
    private TcpListener _tcpListener;
    private TcpListener _tcpListenerpy;
    private List<ClientObject> _clients = new();
    private List<ClientpyObject> _clientspy = new();

    public static class Globals
    {
        public static String query;
        public static String responce;
        public static AutoResetEvent waitHandler_q = new AutoResetEvent(false);
        public static AutoResetEvent waitHandler_r = new AutoResetEvent(false);
    }

    protected internal void AddConnection(ClientObject clientObject)
    {
        _clients.Add(clientObject);
    }
    
    protected internal void AddpyConnection(ClientpyObject clientpyObject)
    {
        _clientspy.Add(clientpyObject);
    }

    protected internal void RemoveConnection(string id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id);
        if (client != null)
            _clients.Remove(client);
    }
    
    protected internal void RemovepyConnection(string id)
    {
        var client = _clientspy.FirstOrDefault(c => c.Id == id);
        if (client != null)
            _clientspy.Remove(client);
    }

    protected internal void Listen()
    {
        try
        {
            _tcpListener = new TcpListener(IPAddress.Any, 8888);
            _tcpListener.Start();
            
            Console.WriteLine("////Telnet portable version 0.2//// \nWaiting for clients requests...");
            
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
    
    protected internal void Listenpy()
    {
        try
        {
            _tcpListenerpy = new TcpListener(IPAddress.Any, 25001);
            _tcpListenerpy.Start();
            
            Console.WriteLine("Waiting for python server response...");
            
            while (true)
            {
                var tcpClient = _tcpListenerpy.AcceptTcpClient();

                var clientpyObject = new ClientpyObject(tcpClient, this);
                var clientpyThread = new Thread(clientpyObject.Process);
                clientpyThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Disconnect();
        }
    }

    protected internal void SendMessage(string message, string id)
    {
        var data = Encoding.Unicode.GetBytes(message);
        foreach (var client in _clients.Where(client => client.Id == id))
        {
            client.Stream.Write(data, 0, data.Length);
        }
    }
    
    protected internal void SendpyMessage(string message, string id)
    {
        var data = Encoding.Unicode.GetBytes(message);
        foreach (var client in _clientspy.Where(client => client.Id == id))
        {
            client.Stream.Write(data, 0, data.Length);
        }
    }
    
    protected internal void Disconnect()
    {
        _tcpListener.Stop();
        _tcpListenerpy.Stop();

        foreach (var client in _clients)
        {
            client.Close();
        }
        
        foreach (var client in _clientspy)
        {
            client.Close();
        }

        Environment.Exit(0);
    }
}
