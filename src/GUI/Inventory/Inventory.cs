using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Inventory : Control
{
	public static List<Item> items_from_server = new List<Item>();
	public static List<Item> items_in_client = new List<Item>();
	public static Inventory instance;

	[Export] public int InventoryWidth;
	[Export] public int InventoryHeight;
	[Export] public string inventorySlotPath;
	[Export] public NodePath InventoryParentPath;
	[Export] public NodePath backgroundPath;
	[Export] public string ItemHolderPath;
	public TextureRect background;
	public static Control[] inventory_slots;
	public static bool draggingItem = false;
	public GridContainer inventory_parent;

	public override void _Ready()
	{
		background = GetNode<TextureRect>(backgroundPath);
		inventory_slots = new Control[(InventoryHeight * InventoryWidth) + 1];
		buildInventorySlots();
		instance = this;
		_Hide();
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

	public void addItemToInventory(Item item)
	{
		PackedScene itemPS = (PackedScene)ResourceLoader.Load($"res://prefabs/UI/Item_slot.tscn");
		Item newItem = itemPS.Instance() as Item;
		newItem.SetItemData(item.iid, item.data, item.count, item.window, item.position);
		GetTree().Root.GetNode<CanvasLayer>(ItemHolderPath).CallDeferred("add_child", newItem);
		items_in_client.Add(newItem);

		PlayerEquip.UpdateNewItem(item);
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

	public static void _Toggle()
	{
		if (Inventory.instance.background.Visible)
			Inventory._Hide();
		else
			Inventory._Show();
	}

	public static void _Show()
	{
		Inventory.instance.background.Visible = true;
		GUIManager.GUIQueue.Add(GUIS.Inventory);
	}

	public static void _Hide()
	{
		Inventory.instance.background.Visible = false;
		for (int i = 0; i < GUIManager.GUIQueue.Count; i++)
		{
			if (GUIManager.GUIQueue[i] == GUIS.Inventory)
				GUIManager.GUIQueue.RemoveAt(i);
		}
	}
}
