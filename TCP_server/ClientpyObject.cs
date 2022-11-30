using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace TCP_server;

public class ClientpyObject
{
    protected internal string Id { get; private set; }
    protected internal NetworkStream Stream { get; private set; }
    //private string _userName;
    private TcpClient _client;
    private ServerObject _server;

    public ClientpyObject(TcpClient tcpClient, ServerObject serverObject)
    {
        Id = Guid.NewGuid().ToString();
        _client = tcpClient;
        _server = serverObject;
        serverObject.AddpyConnection(this);
    }

    public void Process()
    {
        try
        {
            Stream = _client.GetStream();

            //initial broadcast
            var messageout = "////Connection with Telnet portable version 0.2 ... [established]////";
            _server.SendpyMessage(messageout, Id);
            
            //initial response
            var messagein = Regex.Replace(GetMessage(), @"[^\u0000-\u007F]", string.Empty);
            Console.WriteLine(messagein);
            while (true)
            {
                try
                {
                    ServerObject.Globals.waitHandler_q.WaitOne();
                    var message = ServerObject.Globals.query;
                    _server.SendpyMessage(message, Id);
                    ServerObject.Globals.waitHandler_q.Reset();
                    
                    ServerObject.Globals.responce = Regex.Replace(GetMessage(), @"[^\u0000-\u007F]", string.Empty);
                    Console.WriteLine("Python server responses: [" + ServerObject.Globals.responce + "]");
                    ServerObject.Globals.waitHandler_r.Set();
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
            _server.RemovepyConnection(Id);
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