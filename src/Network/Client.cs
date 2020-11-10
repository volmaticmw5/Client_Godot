using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using Godot;
using System.Threading.Tasks;

public class Client : Node
{
	public static Client instance;

	public static string AuthenticationServerAddr = "192.168.1.65";
	public static int AuthenticationPort = 55000;

	private int ClientID = -1;
	public TCP tcp;
	private int session_id = -1;

	public delegate void PacketHandler(Packet packet);
	public static Dictionary<int, PacketHandler> packetHandlers;

	public override void _Ready()
	{
		if (instance == null)
			instance = this;
		InitializeClientData();
	}

	public bool isConnected()
	{
		if (Client.instance.tcp == null)
			return false;
		if (Client.instance.tcp.socket == null)
			return false;
		if (Client.instance.tcp.socket.Connected)
			return true;
		return false;
	}

	internal void setSessionId(int _session_id)
	{
		GD.Print("set session id to " + _session_id);
		this.session_id = _session_id;
	}

	internal int getSessionId()
	{
		return this.session_id;
	}

	public int getCID()
	{
		return ClientID;
	}

	public void setCID(int val)
	{
		this.ClientID = val;
	}

	public override void _Notification(int what)
	{
		if (what == MainLoop.NotificationWmQuitRequest)
			Disconnect(5);
	}

	private void InitializeClientData()
	{
		packetHandlers = new Dictionary<int, PacketHandler>()
		{
			{(int)ServerPackets.connectSucess, Authentication.Pong },
			{(int)ServerPackets.requestAuth, Authentication.RequestAuth },
			{(int)ServerPackets.authResult, Authentication.AuthFailed },
			{(int)ServerPackets.alreadyConnected, Authentication.AlreadyConnected },
			{(int)ServerPackets.charSelection, Authentication.CharSelectionCB },
			{(int)ServerPackets.goToServerAt, Authentication.GoToGameServer },
			{(int)ServerPackets.identifyoself, Authentication.IdentifyMyself },
			{(int)ServerPackets.warpTo, SceneManager.WarpTo },
			{(int)ServerPackets.playersInMap, Map.HandlePlayersInMap },
			{(int)ServerPackets.mobsInMap, Map.HandleMobsInMap },
			{(int)ServerPackets.chatCb, Console.NewConsoleEntry },
			{(int)ServerPackets.updateInventory, Inventory.Update },
			{(int)ServerPackets.updatePlayer, Player.Update },
			{(int)ServerPackets.damageSignal, GUIManager.ShowDamageSignal },
			{(int)ServerPackets.reconnectWarp, SceneManager.ReconnectAndWarp },
		};
		GD.Print("Initialized client packets.");
	}

	public async void Connect(string addr, int port)
	{
		if(tcp != null)
		{
			if (tcp.socket.Connected)
				Disconnect(6);
		}

		// It's extremely important (apparently) to wait after a disconnect to connect again
		// If we don't wait a bit before connecting to another server, the client seems to disconnect right away, as if the disconnect method called above
		// is not yet finished and disconnects both in the old and the "new" server we're connecting to.
		// TODO :: figure out a nicer way of handling this, maybe some retries, etc..
		await Task.Delay(1000);

		GD.Print($"Connecting to game server on {addr} on port {port}...");
		tcp = new TCP(addr, port, SERVER_TYPE.GAME);
		tcp.Connect();
	}

	public void ConnectToAuthenticationServer()
	{
		GD.Print($"Connecting to authentication server on {AuthenticationServerAddr} on port {AuthenticationPort}...");
		tcp = new TCP(AuthenticationServerAddr, AuthenticationPort, SERVER_TYPE.GAME);
		tcp.Connect();
	}

	public void Disconnect(int code = -1)
	{
		if (tcp != null)
		{
			tcp.socket.Close();
			tcp = null;
			GD.Print($"Disconnected from the game server ({code}).");
		}
	}

	public static void SendTCPData(Packet packet)
	{
		packet.WriteLength();
		if (Client.instance == null)
		{
			GD.Print("Failed to send data to the game server, client instance is no longer available or the client is disconnected.");
			SceneManager.ToLogin();
			return;
		}

		if (Client.instance.tcp == null)
		{
			GD.Print("Failed to send data to the game server, client instance is no longer available or the client is disconnected.");
			SceneManager.ToLogin();
			return;
		}

		if (Client.instance.tcp.socket.Connected == false)
		{
			GD.Print("Failed to send data to the game server, client instance is no longer available or the client is disconnected.");
			SceneManager.ToLogin();
			return;
		}

		Client.instance.tcp.SendData(packet);
	}
}
