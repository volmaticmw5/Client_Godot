using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Mob : KinematicBody
{
	public MobData data;
	public Vector3 position;
	private Vector3 lastPos;
	private Transform t;
	public int mid;
	public float hp;
	public float maxHp;
	private bool instanced;

	public void Init(MobData _data, int _mid, float _hp, float _maxHp, Vector3 _pos)
	{
		this.data = _data;
		this.mid = _mid;
		this.position = _pos;
		this.maxHp = _maxHp;
		this.hp = _hp;
	}

	public override void _Ready()
	{
		this.instanced = true;
	}

	public override void _Process(float delta)
	{
		t = Transform;
		if (MathHelper.Distance(lastPos, position) > 10f)
		{
			t.origin = position;
			Transform = t;
		}
		else
		{
			t.origin = new Vector3(
				Mathf.Lerp(t.origin.x, position.x, (float)(data.movementSpeed * ANIMATION_SPEEDS.MOB_WALK_SPEED_MODIFIER) * delta),
				Mathf.Lerp(t.origin.y, position.y, (float)(data.movementSpeed * ANIMATION_SPEEDS.MOB_WALK_SPEED_MODIFIER) * delta),
				Mathf.Lerp(t.origin.z, position.z, (float)(data.movementSpeed * ANIMATION_SPEEDS.MOB_WALK_SPEED_MODIFIER) * delta)
			);
			Transform = t;
		}
	   
		lastPos = t.origin;
	}

	public void UpdateFromServer(Mob mob)
	{
		if (!instanced)
			return;

		this.position = mob.position;
		this.hp = mob.hp;
	}

	public void hideHud()
	{
		GUIManager.HideMobHUDForMob(this.mid);
	}

	/*public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseButton b)
		{
			if (b.IsPressed())
				return;
			if (b.ButtonIndex == 2)
				GUIManager.ShowMobHUD(this);
		}
	}*/

	private void onClick(object camera, object @event, Vector3 click_position, Vector3 click_normal, int shape_idx)
	{
		if (@event is InputEventMouseButton b)
			if (b.Pressed && b.ButtonIndex == 2)
				GUIManager.ShowMobHUD(this);
	}
}
