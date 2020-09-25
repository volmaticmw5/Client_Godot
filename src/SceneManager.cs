using System.Collections.Generic;
using Godot;

/// <summary>
/// All of the scenes that are NOT maps that need cleanup
/// </summary>
public enum ScenePrefabs
{
	LoginGUI,
	SelectionGUI,
}

public enum MapIndexes
{
	map_tests = 1,
}

public class SceneManager : Node
{
	private static SceneManager instance;
	private Dictionary<ScenePrefabs, string[]> Scenes = new Dictionary<ScenePrefabs, string[]>()
	{
		{ ScenePrefabs.LoginGUI, new [] { "res://prefabs/UI/LoginGUI.tscn", "Game/LoginGUI" } },
		{ ScenePrefabs.SelectionGUI, new [] {"res://prefabs/UI/SelectionGUI.tscn", "Game/SelectionGUI"} },
	};
	private static List<string> mapScenesPaths = new List<string>();

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
		foreach (ScenePrefabs s in instance.Scenes.Keys)
		{
			if (scene == s)
			{
				string path = instance.Scenes[s][0];
				var tempInstance = instance.GetTree().Root.GetNodeOrNull(instance.Scenes[s][1]);
				if (tempInstance == null)
				{
					PackedScene packed = (PackedScene)ResourceLoader.Load(instance.Scenes[s][0]);
					instance.GetTree().Root.GetNodeOrNull(TreePath).CallDeferred("add_child", packed.Instance());
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
		foreach (ScenePrefabs s in instance.Scenes.Keys)
		{
			if (scene == s)
			{
				string path = instance.Scenes[s][0];
				PackedScene packed = (PackedScene)ResourceLoader.Load(instance.Scenes[s][0]);
				instance.GetTree().Root.GetNodeOrNull(TreePath).CallDeferred("add_child", packed.Instance());
				return true;
			}
		}
		return false;
	}

	public static void ClearScenes()
	{
		foreach (KeyValuePair<ScenePrefabs, string[]> scene in instance.Scenes)
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
	}

	public static void ClearAllMapScenes()
	{
		foreach (string ms in mapScenesPaths)
		{
			var mInstance = instance.GetTree().Root.GetNodeOrNull(ms);
			if(mInstance != null)
			{
				if (mInstance.IsInsideTree())
					try { mInstance.CallDeferred("queue_free"); } catch { }
				else
					try { mInstance.CallDeferred("free"); } catch { }
			}
		}
	}

	public static void LoadMapScene(int mapIndex)
	{
		string enum_to_str = ((MapIndexes)mapIndex).ToString();
		PackedScene packed = (PackedScene)ResourceLoader.Load($"res://prefabs/maps/{enum_to_str}.tscn");
		instance.GetTree().Root.GetNodeOrNull("Game").CallDeferred("add_child", packed.Instance());
		mapScenesPaths.Add($"Game/{enum_to_str}");
	}

	public static void ToLogin()
	{
		GD.Print("Connection lost, back to login...");
		ClearScenes();
		ClearAllMapScenes();
		TryAddSceneNoDupe(ScenePrefabs.LoginGUI, "Game");
	}

	public static void WarpTo(Packet packet)
	{
		int cid = packet.ReadInt();
		int map = packet.ReadInt();
		Vector3 pos = packet.ReadVector3();
		string name = packet.ReadString();
		int sex = packet.ReadInt();
		int race = packet.ReadInt();

		SceneManager.ClearScenes();
		SceneManager.ClearAllMapScenes();
		SceneManager.LoadMapScene(map);

		PackedScene playerPrefab = (PackedScene)ResourceLoader.Load($"res://prefabs/Player.tscn");
		Player playerInstance = (Player)playerPrefab.Instance();
		instance.GetTree().Root.GetNodeOrNull("Game").CallDeferred("add_child", playerInstance);
		playerInstance.SpawnAt(name, pos, sex, race);

		GD.Print($"go to map #{map} at pos {pos.x},{pos.y},{pos.z} with char name of {name}");
	}
}
