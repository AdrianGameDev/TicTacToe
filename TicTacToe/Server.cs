using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Server
{
    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;

    public event Action<string> OnMessageReceived;

    public Server()
    {
        server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        WaitForClient();
    }

    private void WaitForClient()
    {
        server.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
    }

    private void OnClientConnect(IAsyncResult ar)
    {
        client = server.EndAcceptTcpClient(ar);
        stream = client.GetStream();
        Task.Run(() => ReceiveMessages());
    }

    public void SendMessage(string message)
    {
        if (client != null && client.Connected)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }

    private void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (client.Connected)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                OnMessageReceived?.Invoke(message);
            }
        }
    }
}
