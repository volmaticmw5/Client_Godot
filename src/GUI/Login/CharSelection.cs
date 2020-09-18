using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class CharSelection : Control
{
	public static List<iCharSlot> Characters = new List<iCharSlot>();
	private PackedScene s_charslot = (PackedScene)ResourceLoader.Load("res://prefabs/UI/CharacterSlot.tscn");
	public static int selectedPid = -1;
	private static CharSelection instance;
	private Button playBtn;

	public static void SetCharacters(string data)
	{
		if (Characters != null)
			Characters.Clear();

		if(data != "")
		{
			string[] chars = data.Split("/end/");
			for (int i = 0; i < chars.Length; i++)
			{
				if (i > 7) break;
				string[] contents = chars[i].Split(';');
				Characters.Add(new iCharSlot(Int32.Parse(contents[0]), contents[1]));
			}
		}
	}

	public override void _Ready()
	{
		instance = this;
		playBtn = GetTree().Root.FindNode("PlayBtn", true, false) as Button;
		playBtn.Disabled = true;
		Node parent = GetTree().Root.FindNode("CharContainer", true, false);
		if (parent != null)
		{
			for (int i = 0; i < Characters.Count; i++)
			{
				CharSlot newCharslot = s_charslot.Instance() as CharSlot;
				newCharslot.GetNode<Label>("NameLabel").Text = Characters[i].getName();
				newCharslot.AssignAttr(Characters[i]);

				parent.AddChild(newCharslot, true);
			}
		}
		else
		{
			GD.PrintErr("Failed to find the character container ndoe when creating character list.");
		}
	}

	internal static void UpdatePlayerButtonStatus()
	{
		if(selectedPid > 0)
			instance.playBtn.Disabled = false;
		else
			instance.playBtn.Disabled = true;
	}

	public void Play()
	{
		if (selectedPid <= 0 || Client.instance.getSessionId() < 0)
			return;

		using (Packet newPacket = new Packet((int)ClientPackets.enterMap))
		{
			newPacket.Write(Client.instance.getCID());
			newPacket.Write(Client.instance.getSessionId());
			newPacket.Write(selectedPid);
			Client.SendTCPData(newPacket);
		}
	}
}
