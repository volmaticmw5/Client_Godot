using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class OtherPlayer : KinematicBody
{
	private Vector3 FallVector = new Vector3(0.0f, -20.0f, 0.0f);
	private PlayerMesh mesh;
	private Transform orientation;
	private Vector3 lastPos;

	public int pid { get; private set; }
	public int level { get; private set; }
	public PLAYER_RACES race { get; private set; }
	public PLAYER_SEXES sex { get; private set; }
	public string name { get; private set; }
	public Vector3 position { get; private set; }
	public PlayerStats stats { get; private set; }
	public int heading { get; private set; }
	public bool attacking { get; private set; }

	private float currentAnimTimeScale = 1f;
	private float currentBlendPosition = 1f;
	private AnimationTree animTree;

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
		this.attacking = data.attacking;
		Spawn();
	}

	public void UpdateThisPlayer(PlayerData data)
	{
		this.position = new Vector3(data.pos.X, data.pos.Y, data.pos.Z);
		this.level = data.level;
		this.heading = data.heading;
		this.attacking = data.attacking;
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

		if(attacking)
		{
			animTree.Set("parameters/TimeScale/scale", ANIMATION_SPEEDS.ATTACK_ANIM_SPEED * stats.attackSpeed);
			animTree.Set("parameters/State/current", ANIMATION_STATES.ATTACK);
		}

		lastPos = Transform.origin;
	}

	private void UpdateGamePosition(float delta)
	{
		SetRotationDegrees(new Vector3(mesh.RotationDegrees.x, heading, mesh.RotationDegrees.z));

		if (!attacking)
		{
			animTree.Set("parameters/State/current", ANIMATION_STATES.WALK);
		}

		if (MathHelper.Distance(new System.Numerics.Vector3(lastPos.x, lastPos.y, lastPos.z), new System.Numerics.Vector3(position.x, position.y, position.z)) <= 0.5f)
		{
			currentAnimTimeScale = 1f;
			currentBlendPosition -= .1f;
			
			animTree.Set("parameters/Walk/blend_position", currentBlendPosition);
			animTree.Set("parameters/TimeScale/scale", currentAnimTimeScale);
			return;
		}

		var velocity = FallVector;
		Transform t = Transform;

		currentAnimTimeScale = stats.movementSpeed * ANIMATION_SPEEDS.WALK_ANIM_SPEED;
		currentBlendPosition += .1f;
		currentBlendPosition = Mathf.Clamp(currentBlendPosition, -1f, 1f);
		velocity += mesh.Transform.basis.z;

		t.origin = new Vector3(
			Mathf.Lerp(t.origin.x, position.x, (float)(stats.movementSpeed * ANIMATION_SPEEDS.WALK_SPEED_MODIFIER) * delta),
			Mathf.Lerp(t.origin.y, position.y, (float)(stats.movementSpeed * ANIMATION_SPEEDS.WALK_SPEED_MODIFIER) * delta),
			Mathf.Lerp(t.origin.z, position.z, (float)(stats.movementSpeed * ANIMATION_SPEEDS.WALK_SPEED_MODIFIER) * delta)
		);
		Transform = t;

		animTree.Set("parameters/Walk/blend_position", currentBlendPosition);
		animTree.Set("parameters/TimeScale/scale", currentAnimTimeScale);

		MoveAndSlide(velocity, Vector3.Up, true);
	}
}
