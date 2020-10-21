using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Inventory : Control
{
	private static List<Item> items_from_server = new List<Item>();
	private static List<Item> items_in_client = new List<Item>();
	public static Inventory instance;

	[Export] public int InventoryWidth;
	[Export] public int InventoryHeight;
	[Export] public string inventorySlotPath;
	[Export] public NodePath EquipSlotWeaponPath;
	[Export] public NodePath InventoryParentPath;
	public Control EquipSlotWeapon;
	public static Control[] inventory_slots;
	public static Control[] equipable_slots;
	public static bool draggingItem = false;
	public GridContainer inventory_parent;
	[Export] public NodePath itemHolderPath;
	public Control itemHolder;

	public override void _Ready()
	{
		itemHolder = GetNode<Control>(itemHolderPath);
		EquipSlotWeapon = GetNode<Control>(EquipSlotWeaponPath);
		inventory_slots = new Control[(InventoryHeight * InventoryWidth) + 1];
		equipable_slots = new Control[2];
		buildInventorySlots();
		buildEquipableSlots();
		instance = this;
	}

	private void buildInventorySlots()
	{
		inventory_parent = GetNode<GridContainer>(InventoryParentPath);
		PackedScene slotPs = (PackedScene)ResourceLoader.Load(inventorySlotPath);
		int pos = 1;
		float margin_left = 0;
		float margin_top = 0;
		for (int y = 0; y < InventoryHeight; y++)
		{
			for (int x = 0; x < InventoryWidth; x++)
			{
				Control slot = slotPs.Instance() as Control;
				slot.Name = $"slot_{pos}";
				slot.RectPosition = new Vector2(margin_left, margin_top);
				inventory_parent.CallDeferred("add_child", slot);
				inventory_slots[pos] = slot;
				margin_left += 25f;
				pos++;
			}
			margin_top += 25f;
			margin_left = 0;
		}
	}

	private void buildEquipableSlots()
	{
		PackedScene slotPs = (PackedScene)ResourceLoader.Load(inventorySlotPath);
		int pos = 1;

		// Weapon slots
		for (int i = 0; i < 2; i++)
		{
			Control slot = slotPs.Instance() as Control;
			slot.Name = $"slot_weapon_{i}";
			slot.RectPosition = new Vector2(0f, 25f * i);
			EquipSlotWeapon.CallDeferred("add_child", slot);
			if(i == 0)
				equipable_slots[pos] = slot;

			pos++;
		}

	}

	public void addItemToInventory(Item item)
	{
		PackedScene itemPS = (PackedScene)ResourceLoader.Load($"res://prefabs/UI/Item_{item.data.size}_slot.tscn");
		Item newItem = itemPS.Instance() as Item;
		newItem.SetItemData(item.iid, item.data, item.count, item.window, item.position);
		itemHolder.CallDeferred("add_child", newItem);
		items_in_client.Add(newItem);
	}

	public static void Update(Packet packet)
	{
		int len = packet.ReadInt();
		List<Item> _items = new List<Item>();
		for (int i = 0; i < len; i++)
			_items.Add(packet.ReadItem());

		if (Inventory.instance == null)
			return;
		
		items_from_server = _items;
		
		for (int s = 0; s < items_from_server.Count; s++)
		{
			bool exists_in_client = false;
			for (int c = 0; c < items_in_client.Count; c++)
			{
				if (items_in_client[c].iid == items_from_server[s].iid)
				{
					items_in_client[c].UpdateFromServer(items_from_server[s]);
					exists_in_client = true;
					break;
				}
			}

			if(!exists_in_client)
			{
				instance.addItemToInventory(items_from_server[s]);
			}
		}
	}
}
