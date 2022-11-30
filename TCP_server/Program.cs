using TCP_server;

var server = new ServerObject();

try
{
    var listenThread = new Thread(server.Listen);
    listenThread.Start();
    var listenpyThread = new Thread(server.Listenpy);
    listenpyThread.Start();
}
catch (Exception e)
{
    server.Disconnect();
    Console.WriteLine(e);
}