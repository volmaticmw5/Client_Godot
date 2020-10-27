using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class PlayerMesh : Spatial
{
	public BoneAttachment weaponAttatch;
	[Export] public NodePath WeaponAttatchPath;

	public override void _Ready()
	{
		weaponAttatch = GetNode<BoneAttachment>(WeaponAttatchPath);
	}

	public void AttatchWeapon(Spatial weapon)
	{
		weaponAttatch.AddChild(weapon);
	}

	public void DetatchWeapon()
	{
		if (weaponAttatch == null)
			return;

		for (int i = 0; i < weaponAttatch.GetChildren().Count; i++)
			weaponAttatch.GetChild(i).QueueFree();
	}
}
