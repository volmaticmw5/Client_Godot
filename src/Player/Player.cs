using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public enum PLAYER_RACES
{
	HUMAN = 1,
	INFECTED = 2,
	ORCS = 3
}

public enum PLAYER_SEXES
{
	MALE = 1,
	FEMALE = 1
}

public enum ANIMATION_STATES
{
	WALK = 0,
	ATTACK = 1,
}

public class ANIMATION_SPEEDS
{
	public static float WALK_SPEED_MODIFIER = 3f;
	public static float WALK_ANIM_SPEED = 1.5f;
	public static float ATTACK_ANIM_SPEED = 3.7f;
	public static float MOB_WALK_SPEED_MODIFIER = 2f;
}

public class Player : KinematicBody
{
	public PlayerStats stats;

	[Export] public NodePath CameraBasePath;
	[Export] public Vector3 Gravity = new Vector3(0, -20f, 0);
	[Export] public int MotionInterpolateSpeed = 10;
	[Export] public int RotationInterpolateSpeed = 10;
	[Export] public float MaxZoom = 2.5f;
	[Export] public float MinZoom = .5f;
	[Export] public float ZoomSpeed = .1f;
	[Export] public float CameraRotationSpeed = 0.002f;
	[Export] public float CameraXmin = -40f;
	[Export] public float CameraXmax = -30f;

	public static Player instance;
	public static PlayerData myPlayerData { get; private set; }
	private bool spawned;
	public bool attacking;
	private bool previousAttackCheck;
	private float currentAnimTimeScale = 1f;
	private float currentBlendPosition = 1f;
	public PlayerMesh mesh;
	public AnimationTree animTree;
	private Vector2 motion;
	private Vector3 velocity = new Vector3();
	private Transform orientation;
	public PlayerCamera playerCamera;
	public Spatial cameraBase;
	public Spatial cameraRot;
	public Spatial camera;
	public float camera_x_rot = 0.0f;
	public float currentCameraRotationSpeed = 0f;

	public string name { set; get; }

	public override void _Ready()
	{
		instance = this;
		cameraBase = GetNode<Spatial>(CameraBasePath);
		cameraRot = cameraBase.GetNode<Spatial>("CameraRot");
		camera = cameraRot.GetNode<Spatial>("Camera");
		playerCamera = new PlayerCamera();
		animTree = mesh.FindNode("AnimationTree", true, false) as AnimationTree;
		PlayerEquip.ResetEquipables();
		using (Packet packet = new Packet((int)ClientPackets.playerInstancedSignal))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			Client.SendTCPData(packet);
		}
	}

	private void ApplyRotationBasedOnMovement(Vector3 cam_x, Vector3 cam_z, float delta)
	{
		Vector3 target = -cam_x * motion.x - cam_z * motion.y;
		if (target.Length() > 0.001f)
		{
			var q_from = new Quat(orientation.basis);
			var q_to = new Quat(new Transform().LookingAt(target, new Vector3(0, 1, 0)).basis);

			orientation.basis = new Basis(q_from.Slerp(q_to, delta * RotationInterpolateSpeed));
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton b)
		{
			if (b.IsPressed() && b.ButtonIndex == 2)
			{
				//if (!GUIManager.isMouseOverGUI_INVENTORY && !GUIManager.isMouseOverGUI_ESCMENU && !GUIManager.isMouseOverGUI_SYSOPT)
					currentCameraRotationSpeed = CameraRotationSpeed;
			}

			if (!b.IsPressed() && b.ButtonIndex == 2)
			{
				currentCameraRotationSpeed = 0f;
			}
		}

		if (@event is InputEventMouseMotion m)
		{
			playerCamera.HandleCameraRotation(this, m);
		}

		if (@event is InputEventKey eventKey)
		{
			if(attacking && eventKey.Scancode == Keybinds.KEYBIND_ATTACK && !eventKey.Pressed && spawned)
			{
				attacking = false;
				previousAttackCheck = false;
			}

			if (eventKey.Pressed)
			{
				if(eventKey.Scancode == Keybinds.KEYBIND_ATTACK && IsOnFloor() && spawned) // todo :: water support, fall support
				{
					attacking = true;
				}
				else
				{
					if (eventKey.Pressed && eventKey.Scancode == Keybinds.KEYBIND_ROTATE_LEFT)
						playerCamera.RotateLeft(this);
					if (eventKey.Pressed && eventKey.Scancode == Keybinds.KEYBIND_ROTATE_RIGHT)
						playerCamera.RotateRight(this);
				}
			}
		}
	}

	public override void _Process(float delta)
	{
		if(attacking && !previousAttackCheck && spawned)
		{
			animTree.Set("parameters/TimeScale/scale", ANIMATION_SPEEDS.ATTACK_ANIM_SPEED * stats.attackSpeed);
			animTree.Set("parameters/State/current", ANIMATION_STATES.ATTACK);
			previousAttackCheck = true;
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		if (!spawned)
			return;

		var motion_target = new Vector2(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
			Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_back")
		);
		motion = motion.LinearInterpolate(motion_target, MotionInterpolateSpeed * delta);

		// Camera rotation stuff
		var cam_z = camera.GlobalTransform.basis.z;
		var cam_x = -camera.GlobalTransform.basis.x;
		cam_z.y = 0;
		cam_z = cam_z.Normalized();
		cam_x.y = 0;
		cam_x = cam_x.Normalized();

		if (!attacking)
		{
			animTree.Set("parameters/State/current", ANIMATION_STATES.WALK);
			ApplyRotationBasedOnMovement(cam_x, cam_z, delta);
		}
		else
		{
			ApplyRotationBasedOnMovement(cam_x, cam_z, delta);
			return; 
		}

		// Movement
		if (motion_target.x != 0 || motion_target.y != 0)
		{
			currentBlendPosition += .1f;
			currentAnimTimeScale = stats.movementSpeed * ANIMATION_SPEEDS.WALK_ANIM_SPEED;
			velocity += mesh.Transform.basis.z;
		}
		else
		{
			currentAnimTimeScale = 1f;
			currentBlendPosition -= .1f;
		}

		currentBlendPosition = Mathf.Clamp(currentBlendPosition, -1f, 1f);
		velocity += Gravity * delta;
		velocity *= (float)(stats.movementSpeed * ANIMATION_SPEEDS.WALK_SPEED_MODIFIER); // Mov speed
		velocity = MoveAndSlide(velocity, new Vector3(0, 1, 0), true, 1);
		velocity = Vector3.Zero;

		orientation = orientation.Orthonormalized();
		mesh.GlobalTransform = new Transform(orientation.basis, mesh.GlobalTransform.origin);

		animTree.Set("parameters/Walk/blend_position", currentBlendPosition);
		animTree.Set("parameters/TimeScale/scale", currentAnimTimeScale);


		myPlayerData.heading = Mathf.FloorToInt(this.mesh.RotationDegrees.y);
		myPlayerData.pos = new System.Numerics.Vector3(
			Transform.origin.x.ToString("0.000").ToFloat(), 
			Transform.origin.y.ToString("0.000").ToFloat(), 
			Transform.origin.z.ToString("0.000").ToFloat());
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
			playerCamera.DoCameraZoom(this, (InputEventMouseButton)@event);

		if (@event is InputEventKey)
			playerCamera.DoCameraZoom(this, (InputEventKey)@event);
	}

	public void SpawnAt(PlayerData _pData)
	{
		myPlayerData = _pData;
		this.name = myPlayerData.name;
		this.stats = myPlayerData.stats;
		string raceName = Enum.GetName(typeof(PLAYER_RACES), myPlayerData.race).ToString().ToLower();
		string sexName = Enum.GetName(typeof(PLAYER_SEXES), myPlayerData.sex).ToString().ToLower();
		PackedScene playerMeshResource = (PackedScene)ResourceLoader.Load($"res://prefabs/3d/characters/player/{raceName}/{sexName}/{sexName}.tscn");
		mesh = playerMeshResource.Instance() as PlayerMesh;
		AddChild(mesh);
		orientation = mesh.GlobalTransform;
		orientation.origin = new Vector3();

		Transform current = Transform;
		current.origin.x = myPlayerData.pos.X;
		current.origin.y = myPlayerData.pos.Y;
		current.origin.z = myPlayerData.pos.Z;
		Transform = current;
		//SetRotationDegrees(new Vector3(mesh.RotationDegrees.x, myPlayerData.heading, mesh.RotationDegrees.z)); < - this breaks the camera movement

		spawned = true;
	}

	public static bool IsReady()
	{
		if (instance == null)
			return false;

		if (!instance.spawned)
			return false;

		return true;
	}

	public static void Broadcast()
	{
		if (SceneManager.Warping)
			return;

		using (Packet packet = new Packet((int)ClientPackets.playerBroadcast))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			packet.Write(myPlayerData);
			packet.Write(Player.instance.attacking);

			Client.SendTCPData(packet);
		}
	}
}
