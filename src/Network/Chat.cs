using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Chat : Node
{
    public static Chat instance;
    private TCP tcp;

    private string addr;
    private int port;

    public override void _Ready()
    {
        if (instance == null)
            instance = this;
    }

    public async void Disconnect(int code = -1)
    {
        if (tcp != null)
        {
            tcp.socket.Close();
            tcp = null;
            GD.Print($"Disconnected from the chat server ({code}), will attempt reconnecting in 30 seconds...");

            await Task.Delay(30000);
            ConnectToChatServer();
        }
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest)
            Disconnect(5);
    }

    internal static void ConnectToChatServerAt(Packet packet)
    {
        string addr = packet.ReadString();
        int port = packet.ReadInt();
        instance.addr = addr;
        instance.port = port;
        instance.ConnectToChatServer();
    }

    internal static void ChatConnectionCallback(Packet packet)
    {
        string msg = packet.ReadString();
        GD.Print(msg);
    }

    private async void ConnectToChatServer()
    {
        if(tcp != null)
        {
            if (tcp.socket.Connected)
            {
                Disconnect(6);
                return;
            }
        }

        await Task.Delay(1000);
        GD.Print($"Connecting to chat server on {addr} on port {port}...");
        tcp = new TCP(addr, port);
        tcp.Connect();
    }

    public static void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        if (Chat.instance == null)
        {
            GD.Print("Failed to send data to the chat server, client instance is no longer available or the client is disconnected.");
            SceneManager.ToLogin();
            return;
        }

        if (Chat.instance.tcp == null)
        {
            GD.Print("Failed to send data to the chat server, client instance is no longer available or the client is disconnected.");
            SceneManager.ToLogin();
            return;
        }

        if (Chat.instance.tcp.socket.Connected == false)
        {
            GD.Print("Failed to send data to the chat server, client instance is no longer available or the client is disconnected.");
            SceneManager.ToLogin();
            return;
        }

        Chat.instance.tcp.SendData(packet);
    }
}
