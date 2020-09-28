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

	public int pid { get; set; }
	public Races race { get; set; }
	public Sexes sex { get; set; }
	public string name { get; set; }
	public Vector3 position { get; set; }

	public void Init(int _pid, int _race, int _sex, string _name, Vector3 _pos)
	{
		this.pid = _pid;
		this.name = _name;
		this.position = _pos;
		this.race = (Races)_race;
		this.sex = (Sexes)_sex;
		Spawn();
	}

	public void Spawn()
	{
		string raceName = Enum.GetName(typeof(Races), race).ToString().ToLower();
		string sexName = Enum.GetName(typeof(Sexes), sex).ToString().ToLower();
		PackedScene playerMeshResource = (PackedScene)ResourceLoader.Load($"res://prefabs/3d/characters/player/{raceName}/{sexName}/{sexName}.tscn");
		PlayerMesh mesh = playerMeshResource.Instance() as PlayerMesh;
		CallDeferred("add_child", mesh);
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
		if (lastPos != position)
		{
			MoveTowards(position, delta);
		}

		lastPos = Transform.origin;
	}

	private void MoveTowards(Vector3 _nPosition, float delta)
	{
		Transform t = Transform;
		t.origin = new Vector3(
			Mathf.Lerp(t.origin.x, _nPosition.x, 10.0f * delta), //10.0f is the movement speed
			Mathf.Lerp(t.origin.y, _nPosition.y, 10.0f * delta),
			Mathf.Lerp(t.origin.z, _nPosition.z, 10.0f * delta)
		);
		Transform = t;
		var velocity = FallVector;
		if (!IsOnFloor())
		{
			velocity = -Vector3.Up;
		}

		MoveAndSlide(velocity, Vector3.Up, true);
	}
}
