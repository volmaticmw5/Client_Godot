using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

class PlayerEquip : Player
{
	public static bool hasWeaponEquipped = false;

	public static void EquipWeapon(int vnum)
	{
		if (hasWeaponEquipped)
			return;

		PackedScene weaponPS = (PackedScene)ResourceLoader.Load($"res://prefabs/3d/items/item_{vnum}.tscn");
		Spatial weapon = (Spatial)weaponPS.Instance();
		Player.instance.mesh.AttatchWeapon(weapon);
		hasWeaponEquipped = true;
	}

	public static void DequipWeapon()
	{
		if(hasWeaponEquipped)
			Player.instance.mesh.DetatchWeapon();
		hasWeaponEquipped = false;
	}

	public static void UpdateNewItem(Item item)
	{
		if (item.window == Item.WINDOW.EQUIPABLES && item.data.type == ITEM_TYPES.WEAPON)
			PlayerEquip.EquipWeapon(item.data.vnum);
	}

	public static void UpdateExistingItem(Item.WINDOW window, Item.WINDOW lastWindow, Item item)
	{
		if (window == Item.WINDOW.EQUIPABLES && item.data.type == ITEM_TYPES.WEAPON)
			PlayerEquip.EquipWeapon(item.data.vnum);
		if (lastWindow == Item.WINDOW.EQUIPABLES && item.data.type == ITEM_TYPES.WEAPON)
			PlayerEquip.DequipWeapon();
	}
}
