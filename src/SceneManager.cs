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

public class SceneManager : Node
{
	private static SceneManager instance;
	private Dictionary<ScenePrefabs, string[]> Scenes = new Dictionary<ScenePrefabs, string[]>()
	{
		{ ScenePrefabs.LoginGUI, new [] { "res://prefabs/UI/LoginGUI.tscn", "Game/LoginGUI" } },
		{ ScenePrefabs.SelectionGUI, new [] {"res://prefabs/UI/SelectionGUI.tscn", "Game/SelectionGUI"} },
	};

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
}
