using System.Net.Sockets;
using System.Text;

namespace TCP_server;

public class ClientObject
{
    protected internal string Id { get; private set; }
    protected internal NetworkStream Stream { get; private set; }
    private string _userName;
    private TcpClient _client;
    private ServerObject _server;

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        Id = Guid.NewGuid().ToString();
        _client = tcpClient;
        _server = serverObject;
        serverObject.AddConnection(this);
    }

    public void Process()
    {
        try
        {
            Stream = _client.GetStream();
            var message = GetMessage();
            _userName = message;
            
            //initial broadcast
            message = "////Welcome to Telnet portable version 0.2//// \nPlease type in your query (space separated):";
            _server.SendMessage(message, Id);
            while (true)
            {
                try
                {
                    //main implementation
                    ServerObject.Globals.query = GetMessage();
                    Console.WriteLine("Client requests: [" + ServerObject.Globals.query + "]");
                    ServerObject.Globals.waitHandler_q.Set();
                    
                    ServerObject.Globals.waitHandler_r.WaitOne();
                    var messageOut = ServerObject.Globals.responce;
                    _server.SendMessage(messageOut, Id);
                    ServerObject.Globals.waitHandler_r.Reset();
                }
                catch
                {
                    //final broadcast (of a specific user)
                    
                    // message = String.Format("debug");
                    // Console.WriteLine(message);
                    // _server.BroadcastMessage(message, Id);
                    break;
                }
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

    //read message and transfer it to string
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
        Stream.Close();
        _client.Close();
    }
}