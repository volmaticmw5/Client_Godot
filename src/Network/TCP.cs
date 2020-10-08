using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TCP
{
	public static int buffer_size = 512;
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
					Client.packetHandlers[packetId](packet);
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
		if (Client.instance != null)
			Client.instance.Disconnect(code);
		if (Chat.instance != null)
			Chat.instance.Disconnect(code);

		stream = null;
		receivedBuffer = null;
		receivedData = null;
		socket = null;
	}
}
