using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Map : Spatial
{
	public static List<OtherPlayer> visiblePlayers = new List<OtherPlayer>();
	public static List<Mob> visibleMobs = new List<Mob>();
	public static Map instance;

	public override void _Ready()
	{
		instance = this;
	}

	public static void HandleMobsInMap(Packet packet)
	{
		int cid = packet.ReadInt();
		int mCount = packet.ReadInt();
		List<Mob> mobs = new List<Mob>();
		for (int i = 0; i < mCount; i++)
			mobs.Add(packet.ReadMob());

		if (instance == null)
			return;

		RemoveNoLongerVisibleMobs(mobs.ToArray());
		foreach (Mob mob in mobs)
		{
			if (mobExistsInMap(mob))
				updateMob(mob);
			else
				createMob(mob);
		}
	}

	public static void HandlePlayersInMap(Packet packet)
	{
		int cid = packet.ReadInt();
		int playerCount = packet.ReadInt();
		List<PlayerData> playersData = new List<PlayerData>();
		for (int i = 0; i < playerCount; i++)
			playersData.Add(packet.ReadPlayerData());

		RemoveNoLongerVisiblePlayers(playersData.ToArray());
		foreach (PlayerData player in playersData)
		{
			if (playerAlreadyInThisMap(player))
				updateThisOtherPlayer(player);
			else
				createOtherPlayer(player);
		}

		// Send the server OUR position , this should be moved somewhere else
		if(Player.IsReady())
			Player.Broadcast();
	}

	private static void RemoveNoLongerVisibleMobs(Mob[] mobsData)
	{
		bool exists = false;
		for (int i = 0; i < visibleMobs.Count; i++)
		{
			exists = false;
			foreach (Mob data in mobsData)
			{
				if (visibleMobs[i].mid == data.mid)
				{
					exists = true;
					break;
				}
			}

			if (!exists)
			{
				visibleMobs[i].hideHud();
				visibleMobs[i].QueueFree();
				visibleMobs.RemoveAt(i);
			}
		}
	}

	private static void RemoveNoLongerVisiblePlayers(PlayerData[] playersData)
	{
		bool exists = false;
		for (int i = 0; i < visiblePlayers.Count; i++)
		{
			exists = false;
			foreach (PlayerData pdata in playersData)
			{
				if (visiblePlayers[i].pid == pdata.pid)
				{
					exists = true;
					break;
				}
			}

			if (!exists)
			{
				visiblePlayers[i].QueueFree();
				visiblePlayers.RemoveAt(i);
			}
		}
	}

	private static void createOtherPlayer(PlayerData player)
	{
		PackedScene nOtherScene = (PackedScene)ResourceLoader.Load($"res://prefabs/OtherPlayer.tscn");
		OtherPlayer otherPlayer = nOtherScene.Instance() as OtherPlayer;
		SceneManager.GetInstance().GetTree().Root.GetNodeOrNull(SceneManager.CurrentMapScenePath).CallDeferred("add_child", otherPlayer);
		otherPlayer.Init(player);
		visiblePlayers.Add(otherPlayer);
	}

	private static void updateThisOtherPlayer(PlayerData player)
	{
		for (int i = 0; i < visiblePlayers.Count; i++)
		{
			if (visiblePlayers[i].pid == player.pid)
				visiblePlayers[i].UpdateThisPlayer(player);
		}
	}

	private static bool playerAlreadyInThisMap(PlayerData player)
	{
		for (int i = 0; i < visiblePlayers.Count; i++)
		{
			if (visiblePlayers[i].pid == player.pid)
				return true;
		}

		return false;
	}

	private static bool mobExistsInMap(Mob mob)
	{
		for (int i = 0; i < visibleMobs.Count; i++)
		{
			if (visibleMobs[i].mid == mob.mid)
				return true;
		}

		return false;
	}

	private static void updateMob(Mob mob)
	{
		for (int i = 0; i < visibleMobs.Count; i++)
		{
			if (visibleMobs[i].mid == mob.mid)
				visibleMobs[i].UpdateFromServer(mob);
		}
	}

	private static void createMob(Mob mob)
	{
		PackedScene nOtherScene = (PackedScene)ResourceLoader.Load($"res://prefabs/3d/mobs/{mob.data.id}.tscn");
		Mob nMob = nOtherScene.Instance() as Mob;
		SceneManager.GetInstance().GetTree().Root.GetNodeOrNull(SceneManager.CurrentMapScenePath).CallDeferred("add_child", nMob);
		nMob.Init(mob.data, mob.mid, mob.hp, mob.maxHp, mob.position, mob.focus, mob.gid);
		visibleMobs.Add(nMob);
	}
}
