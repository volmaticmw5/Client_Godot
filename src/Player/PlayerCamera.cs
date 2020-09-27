using Godot;
using System;

public class PlayerCamera : Player
{
	private Vector2 currentZoom;
	private float zoomVal = 0f;

	public void DoCameraZoom(Player instance, InputEventMouseButton emb)
	{
		if (currentZoom == null)
			currentZoom = new Vector2();

		if (emb.IsPressed())
		{
			if (emb.ButtonIndex == (int)ButtonList.WheelUp)
			{
				if (zoomVal > instance.MinZoom)
				{
					currentZoom.x = instance.camera.Translation.z + instance.ZoomSpeed;
					currentZoom.y = instance.camera.Translation.y - 0.05f;
					zoomVal -= instance.ZoomSpeed;
					UpdateCameraZoom(instance);
				}
			}
			else if (emb.ButtonIndex == (int)ButtonList.WheelDown)
			{
				if (zoomVal < instance.MaxZoom)
				{
					currentZoom.x = instance.camera.Translation.z - instance.ZoomSpeed;
					currentZoom.y = instance.camera.Translation.y + 0.05f;
					zoomVal += instance.ZoomSpeed;
					UpdateCameraZoom(instance);
				}
			}
		}
	}

	public void DoCameraZoom(Player instance, InputEventKey emb)
	{
		if (currentZoom == null)
			currentZoom = new Vector2();

		if (emb.IsPressed())
		{
			if (emb.Scancode == Keybinds.KEYBIND_ZOOM_IN)
			{
				if (zoomVal > instance.MinZoom)
				{
					currentZoom.x = instance.camera.Translation.z + instance.ZoomSpeed;
					currentZoom.y = instance.camera.Translation.y - 0.05f;
					zoomVal -= instance.ZoomSpeed;
					UpdateCameraZoom(instance);
				}
			}
			else if (emb.Scancode == Keybinds.KEYBIND_ZOOM_OUT)
			{
				if (zoomVal < instance.MaxZoom)
				{
					currentZoom.x = instance.camera.Translation.z - instance.ZoomSpeed;
					currentZoom.y = instance.camera.Translation.y + 0.05f;
					zoomVal += instance.ZoomSpeed;
					UpdateCameraZoom(instance);
				}
			}
		}
	}

	private void UpdateCameraZoom(Player instance)
	{
		var translation = instance.camera.Translation;
		translation.z = currentZoom.x;
		translation.y = currentZoom.y;
		instance.camera.Translation = translation;
	}

	public void RotateRight(Player instance)
	{
		instance.cameraBase.RotateY(20f * instance.CameraRotationSpeed);
		instance.cameraBase.Orthonormalize();
		instance.camera_x_rot = Mathf.Clamp(instance.camera_x_rot + .25f * instance.CameraRotationSpeed, Mathf.Deg2Rad(instance.CameraXmin), Mathf.Deg2Rad(instance.CameraXmax));
		instance.cameraRot.Rotation = new Vector3(instance.camera_x_rot, instance.cameraBase.Rotation.y, instance.cameraBase.Rotation.z);
	}

	public void RotateLeft(Player instance)
	{
		instance.cameraBase.RotateY(-20f * instance.CameraRotationSpeed);
		instance.cameraBase.Orthonormalize();
		instance.camera_x_rot = Mathf.Clamp(instance.camera_x_rot + .25f * instance.CameraRotationSpeed, Mathf.Deg2Rad(instance.CameraXmin), Mathf.Deg2Rad(instance.CameraXmax));
		instance.cameraRot.Rotation = new Vector3(instance.camera_x_rot, instance.cameraBase.Rotation.y, instance.cameraBase.Rotation.z);
	}

	public void HandleCameraRotation(Player instance, InputEventMouseMotion m)
	{
		instance.cameraBase.RotateY(-m.Relative.x * instance.currentCameraRotationSpeed);
		instance.cameraBase.Orthonormalize();
		instance.camera_x_rot = Mathf.Clamp(instance.camera_x_rot + m.Relative.y * instance.currentCameraRotationSpeed, Mathf.Deg2Rad(instance.CameraXmin), Mathf.Deg2Rad(instance.CameraXmax));
		instance.cameraRot.Rotation = new Vector3(instance.camera_x_rot, instance.cameraBase.Rotation.y, instance.cameraBase.Rotation.z);
	}
}
