using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

class DebugTexts : RichTextLabel
{
	public override void _Process(float delta)
	{
		if (SceneManager.Warping)
			return;

		Text = "------------------ DEBUG --------------------\n";
		Text += Player.instance.name + " (" + Player.data.pid + ") Lv." + Player.data.level + " | AS: " + Player.instance.animTree.Get("parameters/State/current") + "| AScale: " + Player.instance.animTree.Get("parameters/TimeScale/scale") + "\n";
		Text += $"HP {Player.data.hp}/{Player.data.maxHp} | MN {Player.data.mana}/{Player.data.maxMana}\n";
		Text += "---------------- PLAYERS -------------------\n";
		foreach (var player in Map.visiblePlayers)
			Text += $"{player.name} ({player.pid}) Lv. {player.level} | HP {player.hp}/{player.maxHp} | MN {player.mana}/{player.maxMana} | AS: {player.animation_state} | AScale: {player.animTree.Get("parameters/TimeScale/scale")}\n";
		Text += "----------------- MOBS --------------------\n";
		foreach (var mob in Map.visibleMobs)
		{
			Text += $"{mob.data.name} ({mob.mid}) | Group {mob.gid} | Focus {mob.focus} | HP {mob.hp}/{mob.maxHp}\n";
			Text += $"Server Pos: {mob.position.x},{mob.position.y},{mob.position.z}\n";
		}
	}
}
