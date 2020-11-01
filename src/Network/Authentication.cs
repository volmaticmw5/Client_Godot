using System;
using System.Text;
using System.Linq;
using Godot;
using System.Collections.Generic;

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
			Client.instance.Disconnect(1);
		}
	}

	// Send authentication data to the server
	public static void RequestAuth(Packet packet)
	{
		int id = packet.ReadInt();
		if (id == Client.instance.getCID())
		{
			if(SceneManager.IsWarping())
			{
				using (Packet newPacket = new Packet((int)ClientPackets.getTargetGameServerForWarp))
				{
					newPacket.Write(id);
					newPacket.Write(SceneManager.WarpingSID);
					newPacket.Write(SceneManager.WarpingPid);
					newPacket.Write(SceneManager.WarpingTo);
					Client.SendTCPData(newPacket);
				}
			}
			else
			{
				using (Packet newPacket = new Packet((int)ClientPackets.authenticate))
				{
					newPacket.Write(id);
					newPacket.Write(LoginManager.CurrentUsername);
					newPacket.Write(LoginManager.CurrentPassword);
					Client.SendTCPData(newPacket);
				}
			}
		}
	}

	public static void AuthFailed(Packet packet)
	{
		GD.Print("Failed to authenticate!");
		Client.instance.Disconnect(2);
	}

	public static void AlreadyConnected(Packet packet)
	{
		GD.Print("This account is already connected! Please wait a few seconds!");
		Client.instance.Disconnect(9);
	}

	public static void CharSelectionCB(Packet packet)
	{
		List<CharacterSelectionEntry> characters = new List<CharacterSelectionEntry>();
		int id = packet.ReadInt();
		int session_id = packet.ReadInt();
		for (int i = 0; i < 8; i++)
		{
			CharacterSelectionEntry entry = packet.ReadCharacterSelectionEntry();
			characters.Add(entry);
		}

		Client.instance.setSessionId(session_id);
		SceneManager.ClearScenes();
		SceneManager.TryAddSceneNoDupe(ScenePrefabs.SelectionGUI, "Game");
		CharSelection.SetCharacters(characters.ToArray());
	}

	public static void GoToGameServer(Packet packet)
	{
		GD.Print("Connecting to game server...");
		int cid = packet.ReadInt();
		int session = packet.ReadInt();
		string addr = packet.ReadString();
		int port = packet.ReadInt();

		if(Client.instance.getSessionId() == session)
		{
			Client.instance.Connect(addr, port);
		}
		else
		{
			Client.instance.Disconnect(4);
		}
	}

	internal static void IdentifyMyself(Packet packet)
	{
		int cid = packet.ReadInt();
		string msg = packet.ReadString();
		int sid = Client.instance.getSessionId();
		GD.Print(msg);

		Client.instance.setCID(cid);

		using (Packet newPacket = new Packet((int)ClientPackets.itsme))
		{
			newPacket.Write(cid);
			newPacket.Write(sid);
			Client.SendTCPData(newPacket);
		}
	}
}
