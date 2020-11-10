using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class OtherPlayer : KinematicBody
{
	private PlayerMesh mesh;
	private Transform orientation;

	public int pid { get; private set; }
	public int level { get; private set; }
	public PLAYER_RACES race { get; private set; }
	public PLAYER_SEXES sex { get; private set; }
	public string name { get; private set; }
	public Vector3 position { get; private set; }
	public PlayerStats stats { get; private set; }
	public int heading { get; private set; }
	public ANIMATION_STATES animation_state { get; private set; }
	public float maxHp { get; private set; }
	public float maxMana { get; private set; }
	public float hp { get; private set; }
	public float mana { get; private set; }

	private float currentAnimTimeScale = 1f;
	private float currentBlendPosition = 1f;
	public AnimationTree animTree;

	public void Init(PlayerData data)
	{
		this.pid = data.pid;
		this.level = data.level;
		this.name = data.name;
		this.position = new Vector3(data.pos.X, data.pos.Y, data.pos.Z);
		this.race = (PLAYER_RACES)data.race;
		this.sex = (PLAYER_SEXES)data.sex;
		this.stats = data.stats;
		this.heading = data.heading;
		this.animation_state = data.animation_state;
		Spawn();
	}

	public void UpdateThisPlayer(PlayerData data)
	{
		this.position = new Vector3(data.pos.X, data.pos.Y, data.pos.Z);
		this.level = data.level;
		this.heading = data.heading;
		this.animation_state = data.animation_state;
	}

	public void Spawn()
	{
		string raceName = Enum.GetName(typeof(PLAYER_RACES), race).ToString().ToLower();
		string sexName = Enum.GetName(typeof(PLAYER_SEXES), sex).ToString().ToLower();
		PackedScene playerMeshResource = (PackedScene)ResourceLoader.Load($"res://prefabs/3d/characters/player/{raceName}/{sexName}/{sexName}.tscn");
		mesh = playerMeshResource.Instance() as PlayerMesh;
		CallDeferred("add_child", mesh);
		animTree = mesh.FindNode("AnimationTree", true, false) as AnimationTree;
		orientation = mesh.GlobalTransform;
		orientation.origin = new Vector3();

		Transform current = Transform;
		current.origin.x = position.x;
		current.origin.y = position.y;
		current.origin.z = position.z;
		Transform = current;
	}

	public override void _Process(float delta)
	{
		UpdateGamePosition(delta);
	}

	private void setAnimState()
	{
		if (animation_state == ANIMATION_STATES.FALLING)
		{
			if((ANIMATION_STATES)animTree.Get("parameters/State/current") != ANIMATION_STATES.WALK)
				animTree.Set("parameters/Jumping/active", true);

			animTree.Set("parameters/State/current", ANIMATION_STATES.FALLING);
		}
		else
		{
			if ((ANIMATION_STATES)animTree.Get("parameters/State/current") == ANIMATION_STATES.FALLING)
				animTree.Set("parameters/HitGround/active", true);

			if (animation_state == ANIMATION_STATES.ATTACK)
				animTree.Set("parameters/State/current", ANIMATION_STATES.ATTACK);
			else
				animTree.Set("parameters/State/current", ANIMATION_STATES.WALK);
		}
	}

	private void setTimeScale()
	{
		if (animation_state == ANIMATION_STATES.WALK)
			currentAnimTimeScale = stats.movementSpeed * ANIMATION_SPEEDS.WALK_ANIM_SPEED;
		else if (animation_state == ANIMATION_STATES.ATTACK)
			currentAnimTimeScale = ANIMATION_SPEEDS.ATTACK_ANIM_SPEED * stats.attackSpeed;
		else if (animation_state == ANIMATION_STATES.FALLING)
			currentAnimTimeScale = ANIMATION_SPEEDS.PLAYER_FALLING_ANIM_SPEED;
	}

	private void UpdateGamePosition(float delta)
	{
		Transform t = Transform;
		mesh.RotationDegrees = new Vector3(mesh.RotationDegrees.x, Mathf.Lerp(mesh.RotationDegrees.y, heading, 50f * delta), mesh.RotationDegrees.z);
		setAnimState();

		if (MathHelper.Distance(new System.Numerics.Vector3(t.origin.x, t.origin.y, t.origin.z), new System.Numerics.Vector3(position.x, position.y, position.z)) <= 0.2f)
		{
			// Return from moving to idle position
			if (animation_state != ANIMATION_STATES.ATTACK && animation_state != ANIMATION_STATES.FALLING) 
			{
				currentAnimTimeScale = 1f;
				currentBlendPosition -= .1f;
				currentBlendPosition = Mathf.Clamp(currentBlendPosition, -1f, 1f);
			}
		}
		else
		{
			setTimeScale();
			currentBlendPosition += .1f;
			currentBlendPosition = Mathf.Clamp(currentBlendPosition, -1f, 1f);
		}
		
		t.origin = new Vector3(
			Mathf.Lerp(t.origin.x, position.x, (float)(stats.movementSpeed * ANIMATION_SPEEDS.WALK_SPEED_MODIFIER) * delta),
			Mathf.Lerp(t.origin.y, position.y, (float)(stats.movementSpeed * ANIMATION_SPEEDS.WALK_SPEED_MODIFIER) * delta),
			Mathf.Lerp(t.origin.z, position.z, (float)(stats.movementSpeed * ANIMATION_SPEEDS.WALK_SPEED_MODIFIER) * delta)
		);
		Transform = t;

		animTree.Set("parameters/Walk/blend_position", currentBlendPosition);
		animTree.Set("parameters/TimeScale/scale", currentAnimTimeScale);
	}
}
