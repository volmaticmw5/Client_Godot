using Godot;

public class GameManager : Node
{
    public static bool firstBoot = true;

    public override void _Ready()
    {
        if (firstBoot)
        {
            firstBoot = false;
            SceneManager.TryAddSceneNoDupe(ScenePrefabs.LoginGUI);
        }
    }
}