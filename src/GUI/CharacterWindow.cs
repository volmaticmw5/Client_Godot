using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class CharacterWindow : Control
{
	[Export] public NodePath CharacterSubMenuPath;
	[Export] public NodePath nameLabelPath;
	[Export] public NodePath levelLabelPath;
	[Export] public NodePath expBarNodePath;
	[Export] public int expBarMax = 96;
	[Export] public NodePath vitPointsPath;
	[Export] public NodePath strPointsPath;
	[Export] public NodePath intPointsPath;
	[Export] public NodePath dexPointsPath;
	[Export] public NodePath hpLabelPath;
	[Export] public NodePath paLabelPath;
	[Export] public NodePath maLabelPath;
	[Export] public NodePath asLabelPath;
	[Export] public NodePath unusedLabelPath;
	[Export] public NodePath defenseLabelPath;
	[Export] public NodePath hasteLabelPath;
	[Export] public NodePath WeaponSlotPath;
	private Label nameLabel;
	private Label levelLabel;
	private ColorRect expBar;
	private Label vitPoints;
	private Label strPoints;
	private Label intPoints;
	private Label dexPoints;
	private Label hpLabel;
	private Label paLabel;
	private Label maLabel;
	private Label asLabel;
	private Label unusedLabel;
	private Label defenseLabel;
	private Label hasteLabel;
	public Control CharacterSubMenu;
	public static Control WeaponSlot;
	public static CharacterWindow instance;

	public override void _Process(float delta)
	{
		if (!CharacterWindow.instance.Visible)
			return;

		levelLabel.Text = $"Lv.{Player.data.level.ToString()}";
		vitPoints.Text = Player.data.vit.ToString();
		strPoints.Text = Player.data.str.ToString();
		intPoints.Text = Player.data._int.ToString();
		dexPoints.Text = Player.data.dex.ToString();
		hpLabel.Text = ((int)Player.data.maxHp).ToString();
		paLabel.Text = ((int)Player.data.stats.pAttack).ToString();
		maLabel.Text = ((int)Player.data.stats.mAttack).ToString();
		asLabel.Text = ((int)Player.data.stats.attackSpeed).ToString();
		unusedLabel.Text = ((Player.data.level * 5) - Player.data.vit - Player.data.str - Player.data._int - Player.data.dex).ToString();
		defenseLabel.Text = Player.data.stats.mDefense.ToString();
		hasteLabel.Text = Player.data.stats.movementSpeed.ToString();
	}

	public override void _Ready()
	{
		instance = this;
		CharacterSubMenu = GetNode<Control>(CharacterSubMenuPath);
		WeaponSlot = GetNode<Control>(WeaponSlotPath);
		nameLabel = GetNode<Label>(nameLabelPath);
		levelLabel = GetNode<Label>(levelLabelPath);
		vitPoints = GetNode<Label>(vitPointsPath);
		strPoints = GetNode<Label>(strPointsPath);
		intPoints = GetNode<Label>(intPointsPath);
		dexPoints = GetNode<Label>(dexPointsPath);
		expBar = GetNode<ColorRect>(expBarNodePath);
		hpLabel = GetNode<Label>(hpLabelPath);
		paLabel = GetNode<Label>(paLabelPath);
		maLabel = GetNode<Label>(maLabelPath);
		asLabel = GetNode<Label>(asLabelPath);
		unusedLabel = GetNode<Label>(unusedLabelPath);
		defenseLabel = GetNode<Label>(defenseLabelPath);
		hasteLabel = GetNode<Label>(hasteLabelPath);

		_Hide();
	}

	public static void _Toggle()
	{
		if (instance.Visible)
			_Hide();
		else
			_Show();
	}

	public static void _Show()
	{
		instance.Visible = true;
		GUIManager.GUIQueue.Add(GUIS.CharacterWindow);

		instance.nameLabel.Text = Player.data.name;
	}

	public static void _Hide()
	{
		CharacterWindow.instance.Visible = false;
		for (int i = 0; i < GUIManager.GUIQueue.Count; i++)
		{
			if (GUIManager.GUIQueue[i] == GUIS.CharacterWindow)
				GUIManager.GUIQueue.RemoveAt(i);
		}
	}
}
