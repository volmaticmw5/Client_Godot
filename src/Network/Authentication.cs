using System;
using System.Text;
using System.Linq;
using Godot;

class Authentication
{
	/// <summary>
	/// The server has requested us to provide a valid PONG, send it
	/// </summary>
	public static void Pong(Packet packet)
	{
		GD.Print("PONG!");
		// Generate a PONG value, which is an ecrypted string based on the client id the server sent us
		string msg = packet.ReadString();
		int id = packet.ReadInt();
		if (msg == "Ping?" && id > 0)
		{
			byte[] pong = Security.Hash("PONG" + id.ToString(), Security.GetSalt());
			Client.instance.setCID(id);

			using (Packet newPacket = new Packet((int)ClientPackets.pong))
			{
				newPacket.Write(id);
				newPacket.Write(pong.Length);
				newPacket.Write(pong);
				Client.SendTCPData(newPacket);
			}
		}
		else
		{
			GD.Print("Invalid response from the server, bye.");
			Client.instance.Disconnect();
		}
	}

	/// <summary>
	/// The server is requesting us to send the authentication data, answer back accordingly
	/// </summary>
	public static void RequestAuth(Packet packet)
	{
		int id = packet.ReadInt();
		if (id == Client.instance.getCID())
		{
			// Send authentication data to the server
			using (Packet newPacket = new Packet((int)ClientPackets.authenticate))
			{
				newPacket.Write(id);
				newPacket.Write("teste"); //user
				newPacket.Write("teste"); //password
				Client.SendTCPData(newPacket);
			}
		}
	}

	internal static void AuthFailed(Packet packet)
	{
		GD.Print("Failed to authenticate!");
		Client.instance.Disconnect();
	}

	internal static void CharSelectionCB(Packet packet)
	{
		int id = packet.ReadInt();
		int session_id = packet.ReadInt();
		Client.instance.setSessionId(session_id);
		string data = packet.ReadString();
		if(data == "")
		{
			GD.Print("Failed to authenticate");
			Client.instance.Disconnect();
		}
		else
		{
			SceneManager.ClearScenes();
			SceneManager.TryAddSceneNoDupe(ScenePrefabs.SelectionGUI, "Game");
			CharSelection.SetCharacters(data);
		}

		GD.Print(data);
	}

	internal static void GoToGameServer(Packet packet)
	{
		int cid = packet.ReadInt();
		int session = packet.ReadInt();
		string addr = packet.ReadString();
		int port = packet.ReadInt();

		if(Client.instance.getSessionId() == session)
		{
			// Disconnect from auth server
			// TODO :: Dont go back to login screen, we are WAITING for the game server to request our session id!
			Client.instance.Disconnect();
			Client.instance.Connect(addr, port);
		}
		else
		{
			Client.instance.Disconnect(); // Do go back to login, session missmatch
		}
	}
}
