using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// All of the scenes that are NOT maps that need cleanup
/// </summary>
public enum ScenePrefabs
{
	LoginGUI,
	SelectionGUI,
	Inventory,
	Console,
	CharWindow,
	LoadingScreen,
	ItemHolder
}

public enum MapIndexes
{
	map_tests = 1,
}

public class SceneManager : Node
{
	public static int WarpingTo = -1;
	public static int WarpingPid = -1;
	public static bool Warping = false;
	public static int WarpingSID = -1;

	private static SceneManager instance;
	public static Dictionary<ScenePrefabs, string[]> Scenes = new Dictionary<ScenePrefabs, string[]>()
	{
		{ ScenePrefabs.LoginGUI, new [] { "res://prefabs/UI/LoginGUI.tscn", "Game/LoginGUI" } },
		{ ScenePrefabs.SelectionGUI, new [] {"res://prefabs/UI/SelectionGUI.tscn", "Game/SelectionGUI"} },
		{ ScenePrefabs.Inventory, new [] {"res://prefabs/UI/Inventory.tscn", "Game/Inventory"} },
		{ ScenePrefabs.Console, new [] {"res://prefabs/UI/Console.tscn", "Game/Console"} },
		{ ScenePrefabs.CharWindow, new [] { "res://prefabs/UI/CharWindow.tscn", "Game/CharWindow"} },
		{ ScenePrefabs.LoadingScreen, new [] {"res://prefabs/UI/LoadingScreen.tscn", "Game/LoadingScreen"} },
		{ ScenePrefabs.ItemHolder, new [] {"res://prefabs/UI/ItemHolder.tscn", "Game/ItemHolder"} },
	};
	public static string CurrentMapScenePath;

	public override void _Ready()
	{
		if (instance == null)
			instance = this;
	}

	/// <summary>
	/// Add a new scene, as long as it doesn't already exist.
	/// </summary>
	/// <param name="scene"></param>
	/// <param name="treePath"></param>
	public static bool TryAddSceneNoDupe(ScenePrefabs scene, string TreePath = "Game")
	{
		foreach (ScenePrefabs s in Scenes.Keys)
		{
			if (scene == s)
			{
				string path = Scenes[s][0];
				var tempInstance = instance.GetTree().Root.GetNodeOrNull(Scenes[s][1]);
				if (tempInstance == null)
				{
					PackedScene packed = (PackedScene)ResourceLoader.Load(Scenes[s][0]);
					var scn = packed.Instance();
					instance.GetTree().Root.GetNodeOrNull(TreePath).CallDeferred("add_child", scn);
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Add a new scene to the tree
	/// </summary>
	/// <param name="path"></param>
	/// <param name="scene"></param>
	public static bool TryAddScene(ScenePrefabs scene, string TreePath = "Game")
	{
		foreach (ScenePrefabs s in Scenes.Keys)
		{
			if (scene == s)
			{
				string path = Scenes[s][0];
				PackedScene packed = (PackedScene)ResourceLoader.Load(Scenes[s][0]);
				instance.GetTree().Root.GetNodeOrNull(TreePath).CallDeferred("add_child", packed.Instance());
				return true;
			}
		}
		return false;
	}

	public static SceneManager GetInstance()
	{
		return instance;
	}

	public static void ClearScenes()
	{
		foreach (KeyValuePair<ScenePrefabs, string[]> scene in Scenes)
		{
			var tempInstance = instance.GetTree().Root.GetNodeOrNull(scene.Value[1].ToString());
			if (tempInstance == null)
				tempInstance = instance.GetNodeOrNull(scene.Value[1].ToString());

			if(tempInstance != null)
			{
				if(tempInstance.IsInsideTree())
					try { tempInstance.CallDeferred("queue_free"); } catch { }
				else
					try { tempInstance.CallDeferred("free"); } catch { }
			}
		}

		Inventory.items_from_server.Clear();
		Inventory.items_in_client.Clear();
		Inventory.instance = null;
		CharacterWindow.instance = null;
	}

	public static void ClearAllMapScenes()
	{
		var mInstance = instance.GetTree().Root.GetNodeOrNull(CurrentMapScenePath);
		if(mInstance != null)
		{
			if (mInstance.IsInsideTree())
				try { mInstance.CallDeferred("queue_free"); } catch { }
			else
				try { mInstance.CallDeferred("free"); } catch { }
		}

		Map.visibleMobs.Clear();
		Map.visiblePlayers.Clear();
		Map.instance = null;
	}

	public async static Task<int> LoadMapScene(int mapIndex)
	{
		string enum_to_str = ((MapIndexes)mapIndex).ToString();
		PackedScene packed = (PackedScene)ResourceLoader.Load($"res://prefabs/maps/{enum_to_str}.tscn");
		var scene = packed.Instance();
		scene.Name = enum_to_str;
		instance.GetTree().Root.GetNodeOrNull("Game").AddChild(scene);
		CurrentMapScenePath = $"Game/{scene.Name}";

		SceneManager.TryAddSceneNoDupe(ScenePrefabs.Inventory);
		SceneManager.TryAddSceneNoDupe(ScenePrefabs.Console);
		SceneManager.TryAddSceneNoDupe(ScenePrefabs.CharWindow);
		SceneManager.TryAddSceneNoDupe(ScenePrefabs.ItemHolder);

		while (instance.GetTree().Root.GetNodeOrNull(CurrentMapScenePath) == null)
		{
			await Task.Delay(100);
		}
		return 0;
	}

	public static void ToLogin()
	{
		if (Warping)
			return;

		GD.Print("Connection lost, back to login...");
		ClearScenes();
		ClearAllMapScenes();
		TryAddSceneNoDupe(ScenePrefabs.LoginGUI, "Game");
	}

	public async static void WarpTo(Packet packet)
	{
		PlayerData pData = packet.ReadPlayerData();
		Client.instance.setSessionId(pData.sid);
		GD.Print("Clearing scenes and maps");
		ClearScenes();
		ClearAllMapScenes();
		GD.Print("Loading map scene");
		await LoadMapScene(pData.map);
		GD.Print("Loading new player instance");
		PackedScene playerPrefab = (PackedScene)ResourceLoader.Load($"res://prefabs/Player.tscn");
		Player playerInstance = (Player)playerPrefab.Instance();
		instance.GetTree().Root.GetNode(CurrentMapScenePath).CallDeferred("add_child", playerInstance);
		GD.Print("Player instance loaded");
		playerInstance.SpawnAt(pData);
		GD.Print($"Go to map #{pData.map} at pos {pData.pos.X},{pData.pos.Y},{pData.pos.Z} with char name of {pData.name}");

		if (Warping)
		{
			Warping = false;
			WarpingPid = -1;
			WarpingSID = -1;
			WarpingTo = -1;
		}
	}

	// Connect to the authentication server and tell it to get the target game server for this map
	public static async void ReconnectAndWarp(Packet packet)
	{
		int map = packet.ReadInt();

		SceneManager.ClearScenes();
		TryAddSceneNoDupe(ScenePrefabs.LoadingScreen);
		SceneManager.ClearAllMapScenes();
		await Task.Delay(500);
		Warping = true;
		WarpingTo = map;
		WarpingPid = Player.data.pid;
		WarpingSID = Client.instance.getSessionId();
		await Task.Delay(500);
		Client.instance.Disconnect();
		await Task.Delay(500);
		Client.instance.ConnectToAuthenticationServer();
	}

	public static bool IsWarping()
	{
		if (Warping && WarpingPid > 0 && WarpingTo > 0 && WarpingSID > 0)
			return true;

		return false;
	}
}
