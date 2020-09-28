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
	public static int buffer_size = 512;

	public static string AuthenticationServerAddr = "192.168.1.65";
	public static int AuthenticationPort = 55000;

	private int ClientID = -1;
	private TCP tcp;
	private int session_id = -1;

	private delegate void PacketHandler(Packet packet);
	private static Dictionary<int, PacketHandler> packetHandlers;

	public override void _Ready()
	{
		if (instance == null)
			instance = this;
		InitializeClientData();
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

	// Close the connection once we quit the application
	public override void _Notification(int what)
	{
		if (what == MainLoop.NotificationWmQuitRequest)
		{
			Disconnect(5);
		}
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
		};
		GD.Print("Initialized client packets.");
	}

	/// <summary>
	/// Initializes the connection to the game server
	/// </summary>
	public async void Connect(string addr, int port)
	{
		if (tcp.socket.Connected)
			Disconnect(6);

		// It's extremely important (apparently) to wait after a disconnect to connect again
		// If we don't wait a bit before connecting to another server, the client seems to disconnect right away, as if the disconnect method called above
		// is not yet finished and disconnects both in the old and the "new" server we're connecting to.
		// TODO :: figure out a nicer way of handling this, maybe some retries, etc..
		await Task.Delay(1000);

		GD.Print($"Connecting to game server on {addr} on port {port}...");
		tcp = new TCP(addr, port);
		tcp.Connect();
	}

	/// <summary>
	/// Initializes the connection to the authentication server
	/// </summary>
	public void ConnectToAuthenticationServer()
	{
		GD.Print($"Connecting to authentication server on {AuthenticationServerAddr} on port {AuthenticationPort}...");
		tcp = new TCP(AuthenticationServerAddr, AuthenticationPort);
		tcp.Connect();
	}

	public void Disconnect(int code = -1)
	{
		if (tcp != null)
		{
			tcp.socket.Close();
			tcp = null;
			GD.Print($"Disconnected from the server ({code}).");
		}
	}

	public static void SendTCPData(Packet packet)
	{
		packet.WriteLength();
		if (Client.instance == null)
		{
			GD.Print("Failed to send data to the server, client instance is no longer available or the client is disconnected.");
			SceneManager.ToLogin();
			return;
		}

		if (Client.instance.tcp == null)
		{
			GD.Print("Failed to send data to the server, client instance is no longer available or the client is disconnected.");
			SceneManager.ToLogin();
			return;
		}

		if (Client.instance.tcp.socket.Connected == false)
		{
			GD.Print("Failed to send data to the server, client instance is no longer available or the client is disconnected.");
			SceneManager.ToLogin();
			return;
		}

		Client.instance.tcp.SendData(packet);
	}

	class TCP
	{
		public TcpClient socket;
		private NetworkStream stream;
		private byte[] receivedBuffer;
		private Packet receivedData;
		private string addr;
		private int port;

		public TCP(string _addr, int _port)
		{
			this.addr = _addr;
			this.port = _port;
		}

		public void Connect()
		{
			socket = new TcpClient
			{
				ReceiveBufferSize = buffer_size,
				SendBufferSize = buffer_size
			};

			receivedBuffer = new byte[buffer_size];
			socket.BeginConnect(this.addr, this.port, ConnectCallback, socket);
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			var succ = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
			if (!succ)
			{
				GD.Print($"Error while connecting to the server.");
				SceneManager.ToLogin();
				return;
			}
			else
			{
				try
				{
					socket.EndConnect(ar);

					if (!socket.Connected)
						return;

					stream = socket.GetStream();

					receivedData = new Packet();

					stream.BeginRead(receivedBuffer, 0, buffer_size, ReceiveCallback, null);
				}
				catch (Exception ex)
				{
					GD.Print($"Error while connecting to the server: {ex.Message}");
					SceneManager.ToLogin();
					return;
				}
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				int byteLength = stream.EndRead(ar);

				if (byteLength <= 0)
				{
					Disconnect(44);
					return;
				}

				byte[] data = new byte[byteLength];
				Array.Copy(receivedBuffer, data, byteLength);

				receivedData.Reset(HandleData(data));
				stream.BeginRead(receivedBuffer, 0, buffer_size, ReceiveCallback, null);
			}
			catch (Exception ex)
			{
				//GD.Print($"Error receiving TCP data: {ex.Message}");
				Disconnect(55);
			}
		}

		private bool HandleData(byte[] data)
		{
			int packetLength = 0;
			receivedData.SetBytes(data);
			if (receivedData.UnreadLength() >= 4)
			{
				packetLength = receivedData.ReadInt();
				if (packetLength <= 0)
					return true;
			}

			while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
			{
				byte[] packetBytes = receivedData.ReadBytes(packetLength);
				ThreadManager.ExecuteOnMainThread(() =>
				{
					using (Packet packet = new Packet(packetBytes))
					{
						int packetId = packet.ReadInt();
						packetHandlers[packetId](packet);
					}
				});

				packetLength = 0;
				if (receivedData.UnreadLength() >= 4)
				{
					packetLength = receivedData.ReadInt();
					if (packetLength <= 0)
						return true;
				}
			}

			if (packetLength <= 1)
				return true;

			return false;
		}

		public void SendData(Packet packet)
		{
			try
			{
				if (socket != null)
				{
					stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Error sending data to the server: {ex}");
				SceneManager.ToLogin();
			}
		}

		public void Disconnect(int code = -1)
		{
			instance.Disconnect(code);
			stream = null;
			receivedBuffer = null;
			receivedData = null;
			socket = null;
		}
	}
}
