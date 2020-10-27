using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class MobHUD : Control
{
	public Mob thisMob;
	[Export] public int hpSizeMax = 240;
	[Export] public NodePath NameLabelPath;
	[Export] public NodePath HpLabelPath;
	[Export] public NodePath HpFillPath;
	private Label nameLabel;
	private Label hpLabel;
	private ColorRect hpBar;
	private float maxHp;
	private float lastHp;

	public static MobHUD instance;

	public override void _Ready()
	{
		instance = this;
		nameLabel = GetNode<Label>(NameLabelPath);
		hpLabel = GetNode<Label>(HpLabelPath);
		hpBar = GetNode<ColorRect>(HpFillPath);
		hpBar.RectSize = new Vector2((float)hpSizeMax, 10f);
	}

	public void SetHudData(Mob mob)
	{
		thisMob = mob;
		nameLabel.Text = thisMob.data.name;
		maxHp = thisMob.maxHp;
		hpLabel.Text = Math.Floor(thisMob.hp).ToString() + "/" + Math.Floor(maxHp).ToString();
		lastHp = -1f;
	}

	public override void _Process(float delta)
	{
		if (thisMob == null)
			GUIManager.HideMobHUD();

		if(lastHp != thisMob.hp)
		{
			hpLabel.Text = Math.Floor(thisMob.hp).ToString() + "/" + Math.Floor(maxHp).ToString();
			float percentageFill = (thisMob.hp / maxHp) * 100;
			hpBar.RectSize = new Vector2((percentageFill * hpSizeMax) / 100, 10f);
			lastHp = thisMob.hp;
		}
	}

	public void Update(float hp)
	{
		hpLabel.Text = Math.Floor(hp).ToString() + "/" + Math.Floor(maxHp).ToString();
		if(hp <= 0)
			GUIManager.HideMobHUD();
	}
	
	private void close()
	{
		GUIManager.HideMobHUD();
	}
}
