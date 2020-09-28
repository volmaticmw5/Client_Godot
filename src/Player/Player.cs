using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Player : KinematicBody
{
	public enum Races
	{
		HUMAN = 1,
		INFECTED = 2,
		ORCS = 3
	}

	public enum Sexes
	{
		MALE = 1,
		FEMALE = 1
	}

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

	private static Player instance;
	private PlayerMesh mesh;
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
			if (eventKey.Pressed)
			{
				if (eventKey.Pressed && eventKey.Scancode == Keybinds.KEYBIND_ROTATE_LEFT)
					playerCamera.RotateLeft(this);
				if (eventKey.Pressed && eventKey.Scancode == Keybinds.KEYBIND_ROTATE_RIGHT)
					playerCamera.RotateRight(this);
			}
		}
	}

	public override void _PhysicsProcess(float delta)
	{
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
		ApplyRotationBasedOnMovement(cam_x, cam_z, delta);

		// Movement
		if (motion_target.x != 0 || motion_target.y != 0)
		{
			velocity += mesh.Transform.basis.z;
		}

		velocity += Gravity * delta;
		velocity *= 10f; // Mov speed
		velocity = MoveAndSlide(velocity, new Vector3(0, 1, 0), true, 1);
		velocity = Vector3.Zero;

		orientation = orientation.Orthonormalized();
		mesh.GlobalTransform = new Transform(orientation.basis, mesh.GlobalTransform.origin);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
			playerCamera.DoCameraZoom(this, (InputEventMouseButton)@event);

		if (@event is InputEventKey)
			playerCamera.DoCameraZoom(this, (InputEventKey)@event);
	}

	public void SpawnAt(string _name, Vector3 pos, int sex, int race)
	{
		// Set server stuff
		this.name = _name;

		// Set mesh
		string raceName = Enum.GetName(typeof(Races), race).ToString().ToLower();
		string sexName = Enum.GetName(typeof(Sexes), sex).ToString().ToLower();
		PackedScene playerMeshResource = (PackedScene)ResourceLoader.Load($"res://prefabs/3d/characters/player/{raceName}/{sexName}/{sexName}.tscn");
		mesh = playerMeshResource.Instance() as PlayerMesh;
		AddChild(mesh);
		orientation = mesh.GlobalTransform;
		orientation.origin = new Vector3();

		Transform current = Transform;
		current.origin.x = pos.x;
		current.origin.y = pos.y;
		current.origin.z = pos.z;
		Transform = current;
	}

	public static void SendMyPosition()
	{
		using (Packet packet = new Packet((int)ClientPackets.myPosition))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			packet.Write(instance.Transform.origin);

			Client.SendTCPData(packet);
		}
	}
}
