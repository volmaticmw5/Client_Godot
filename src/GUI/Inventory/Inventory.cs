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
	public static Control[] slots;
	public static bool draggingItem = false;

	public override void _Ready()
	{
		slots = new Control[(InventoryWidth * InventoryHeight) +1];
		buildInventorySlots();
		instance = this;
	}

	private void buildInventorySlots()
	{
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
				GetNode<GridContainer>("Window/CanvasLayer/InventoryList").CallDeferred("add_child", slot);
				slots[pos] = slot;
				margin_left += 25f;
				pos++;
			}
			margin_top += 25f;
			margin_left = 0;
		}
	}

	public void addItemToInventory(Item item)
	{
		PackedScene itemPS = (PackedScene)ResourceLoader.Load($"res://prefabs/UI/Item_{item.data.size}_slot.tscn");
		Item newItem = itemPS.Instance() as Item;
		newItem.SetItemData(item.iid, item.data, item.count, item.window, item.position);
		GetNode<GridContainer>("Window/CanvasLayer/InventoryList").CallDeferred("add_child", newItem);
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
