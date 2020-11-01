using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public enum GUIS
{
    Inventory,
    MobHud,
    Console
}

public class GUIManager : Node
{
    [Export] public string MobHudResourcePath;
    private static GUIManager instance;
    private MobHUD currentMobHud;

    public static bool isMouseOverGUI_INVENTORY;
    public static bool isMouseOverGUI_ESCMENU;
    public static bool isMouseOverGUI_SYSOPT;
    public static bool isMouseOverGUI_JOURNAL;
    public static List<GUIS> GUIQueue = new List<GUIS>();

    public override void _Ready()
    {
        instance = this;
    }

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
			{
				if (GUIQueue.Count > 0)
				{
					switch (GUIQueue[GUIQueue.Count - 1])
					{
						case GUIS.Inventory:
							Inventory._Hide();
							break;
						case GUIS.Console:
							Console.Close();
							break;
						case GUIS.MobHud:
							HideMobHUD();
							break;
					}
				}
				else
				{
					//EscMenu.Open();
				}
			}

			if (eventKey.Pressed && eventKey.Scancode == Keybinds.KEYBIND_INVENTORY)
			{
				Inventory._Toggle();
			}
		}
	}

	public static void ShowMobHUD(Mob mob)
    {
        if (instance.currentMobHud != null)
            HideMobHUD();

        PackedScene mhps = ResourceLoader.Load(instance.MobHudResourcePath) as PackedScene;
        MobHUD mobHud = mhps.Instance() as MobHUD;
        instance.currentMobHud = mobHud;
        instance.AddChild(mobHud);
        mobHud.SetHudData(mob);
		GUIQueue.Add(GUIS.MobHud);
    }

	public static void HideMobHUDForMob(int mid)
	{
		if (instance.currentMobHud == null)
			return;

		if(instance.currentMobHud.thisMob.mid == mid)
		{
			instance.currentMobHud.QueueFree();
			instance.currentMobHud = null;

			for (int i = 0; i < GUIManager.GUIQueue.Count; i++)
			{
				if (GUIManager.GUIQueue[i] == GUIS.MobHud)
					GUIManager.GUIQueue.RemoveAt(i);
			}
		}
	}

	public static void HideMobHUD()
    {
        if (instance.currentMobHud != null)
        {
            instance.currentMobHud.QueueFree();
            instance.currentMobHud = null;

			for (int i = 0; i < GUIManager.GUIQueue.Count; i++)
			{
				if (GUIManager.GUIQueue[i] == GUIS.MobHud)
					GUIManager.GUIQueue.RemoveAt(i);
			}
		}
    }

	public static void ShowDamageSignal(Packet packet)
	{
		int dmg = packet.ReadInt();
		GD.Print($"received {dmg} damage");
	}
}
