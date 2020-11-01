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
	public int focus; // please remove this var on release, they're only here in order to debug
	public int gid; // please remove this var on release, they're only here in order to debug
	private bool instanced;

	public void Init(MobData _data, int _mid, float _hp, float _maxHp, Vector3 _pos, int _focus, int _gid)
	{
		this.data = _data;
		this.mid = _mid;
		this.position = _pos;
		this.maxHp = _maxHp;
		this.hp = _hp;
		this.focus = _focus;
		this.gid = _gid;
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
			var velocity = Transform.origin.DirectionTo(position) * data.movementSpeed * ANIMATION_SPEEDS.MOB_WALK_SPEED_MODIFIER * delta;
			MoveAndCollide(velocity);
			//LookAt(position, Vector3.Left);
		}

		lastPos = t.origin;
	}

	public void UpdateFromServer(Mob mob)
	{
		if (!instanced)
			return;

		this.position = mob.position;
		this.hp = mob.hp;
		this.focus = mob.focus;
		this.gid = mob.gid;
	}

	public void hideHud()
	{
		GUIManager.HideMobHUDForMob(this.mid);
	}

	private void onClick(object camera, object @event, Vector3 click_position, Vector3 click_normal, int shape_idx)
	{
		if (@event is InputEventMouseButton b)
			if (b.Pressed && b.ButtonIndex == 2)
				GUIManager.ShowMobHUD(this);
	}
}
