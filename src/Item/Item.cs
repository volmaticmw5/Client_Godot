using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Item : Button
{
	public enum WINDOW
	{
		NONE,
		INVENTORY,
		STORAGE,
		EQUIPABLES
	}

	public long iid { get; private set; }
	public int count;
	public WINDOW window;
	public int position;
	public int lastPos;
	public ItemData data;
	[Export] public NodePath iconPath;
	[Export] public NodePath iconCountPath;
	private TextureRect icon;
	private Label iconCount;
	private bool instanced;
	private bool dragging;
	private Vector2 dragPosition;
	private WINDOW lastWindow;

	public void UpdateFromServer(Item server_item)
	{
		this.position = server_item.position;
		this.window = server_item.window;
		this.count = server_item.count;
		UpdateItemCountLabel();
		UpdateItemPosition();
	}

	public void SetItemData(long _iid, ItemData _data, int _count, WINDOW _window, int _pos)
	{
		this.iid = _iid;
		this.data = _data;
		this.count = _count;
		this.window = _window;
		this.position = _pos;
		lastPos = this.position;
		lastWindow = this.window;
	}

	public override void _Ready()
	{
		instanced = true;
	}

	public override void _Process(float delta)
	{
		setSlotItemIcon();
		if(dragging)
			lightUpTargetSlot();
	}

	private void lightUpTargetSlot()
	{
		if (Inventory.instance == null)
			return;

		WINDOW targetWindow = getTargetWindow();
		if (targetWindow == WINDOW.NONE)
		{
			clearSlotModulates();
			return;
		}

		int slotPos = getSlotPositionUnderMouse();
		clearSlotModulates();
		Inventory.inventory_slots[slotPos].Modulate = new Color(1f, 0.8f, 1f);
		if(data.size > 1 && slotPos + Inventory.instance.InventoryWidth < Inventory.inventory_slots.Length)
		{
			if (Inventory.inventory_slots[slotPos + Inventory.instance.InventoryWidth] != null)
				Inventory.inventory_slots[slotPos + Inventory.instance.InventoryWidth].Modulate = new Color(1f, 0.8f, 1f);
		}
	}

	private void clearSlotModulates()
	{
		for (int i = 0; i < Inventory.inventory_slots.Length; i++)
			if (Inventory.inventory_slots[i] != null)
				Inventory.inventory_slots[i].Modulate = new Color(1f, 1f, 1f);
	}

	int lastRightclickCheck = 0;
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Inventory.draggingItem && dragging)
			RectGlobalPosition = GetGlobalMousePosition() - dragPosition;


		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton iemb = @event as InputEventMouseButton;
			if (iemb.ButtonIndex == 2 && GetGlobalRect().HasPoint(iemb.Position))
			{
				if (lastRightclickCheck == 1)
				{
					int pos = getSlotPositionUnderMouse();
					if (pos > 0)
						requestItemUsage(pos);
					else
					{
						if(window == WINDOW.EQUIPABLES)
						{
							if (data.type == ITEM_TYPES.WEAPON)
								requestItemUsage(1);
						}
					}
				}
				else
					lastRightclickCheck = 0;
			}
			lastRightclickCheck++;
		}
	}

	private void setSlotItemIcon()
	{
		if (!instanced)
			return;

		Name = "icon_" + data.vnum + "_" + position;
		icon = GetNode<TextureRect>(iconPath);
		iconCount = GetNode<Label>(iconCountPath);
		icon.Texture = ResourceLoader.Load<Texture>($"res://prefabs/UI/icons/items/{data.vnum}.png");
		setPosition();
		setSize();
		UpdateItemCountLabel();
		instanced = false;
	}

	private void setSize()
	{
		if (data.size == 1)
			SetSize(new Vector2(25f, 25f));
		else
			SetSize(new Vector2(25f, 50f));
	}

	private void setPosition()
	{
		if (window == WINDOW.INVENTORY)
			RectGlobalPosition = Inventory.inventory_slots[position].RectGlobalPosition;
		else if (window == WINDOW.EQUIPABLES)
			RectGlobalPosition = Inventory.equipable_slots[position].RectGlobalPosition;
	}

	private void UpdateItemCountLabel()
	{
		try
		{
			if (!data.stacks)
				iconCount.Text = "";
			else
				iconCount.Text = count.ToString();
		}
		catch { }
	}

	private void UpdateItemPosition()
	{
		Name = "icon_" + data.vnum + "_" + position;
		if (lastPos != position || lastWindow != window)
		{
			if (window == WINDOW.INVENTORY)
			{
				RectGlobalPosition = Inventory.inventory_slots[position].RectGlobalPosition;
			}
			else if (window == WINDOW.EQUIPABLES)
			{
				RectGlobalPosition = Inventory.equipable_slots[position].RectGlobalPosition;
			}
		}

		lastPos = position;
		lastWindow = window;
	}

	private int getSlotPositionUnderMouse()
	{
		foreach (Control slot in Inventory.inventory_slots)
		{
			if(slot == null)
				continue;

			if (slot.GetGlobalRect().HasPoint(new Vector2(GetGlobalRect().Position.x + (RectSize.x / 2), GetGlobalRect().Position.y)))
			{
				string[] slotNameArray = slot.Name.Split('_');
				int pos = Int32.Parse(slotNameArray[1]);
				return pos;
			}
		}
		return -1; 
	}

	private void returnItemToOriginalPosition()
	{
		if(window == WINDOW.INVENTORY)
		{
			RectGlobalPosition = Inventory.inventory_slots[position].RectGlobalPosition;
		}
		else
		{
			RectGlobalPosition = Inventory.equipable_slots[position].RectGlobalPosition;
		}
	}

	private WINDOW getTargetWindow()
	{
		bool isInventory = false;
		foreach (Control slot in Inventory.inventory_slots)
		{
			if (slot == null)
				continue;

			if (slot.GetGlobalRect().HasPoint(new Vector2(GetGlobalRect().Position.x + (RectSize.x / 2), GetGlobalRect().Position.y)))
			{
				isInventory = true;
				break;
			}
		}

		if (isInventory)
			return WINDOW.INVENTORY;

		return WINDOW.NONE;
	}

	public void StartDrag()
	{
		if (Inventory.draggingItem)
			return;

		if (dragging)
			interruptDragging();
		else
		{
			Inventory.draggingItem = true;
			dragging = true;
			dragPosition = GetGlobalMousePosition() - RectGlobalPosition;
		}
	}

	public void interruptDragging()
	{
		Inventory.draggingItem = false;
		dragging = false;
		dragPosition = Vector2.Zero;
		returnItemToOriginalPosition();
	}

	public void EndDrag()
	{
		WINDOW targetWindow = getTargetWindow();

		int pos = getSlotPositionUnderMouse();
		if(targetWindow == WINDOW.NONE)
		{
			GD.Print("thow item away?");
		}
		else
		{
			requestItemMove(targetWindow, pos);
		}

		interruptDragging();
		clearSlotModulates();
	}

	private void requestItemMove(WINDOW target_window, int newPos)
	{
		using (Packet packet = new Packet((int)ClientPackets.itemChangePosition))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			packet.Write(iid);
			packet.Write(newPos);
			packet.Write((int)target_window);
			Client.SendTCPData(packet);
		}
	}

	private void requestItemUsage(int pos)
	{
		GD.Print($"request item usage at window {window.ToString()} and position {pos}");
		using (Packet packet = new Packet((int)ClientPackets.itemUse))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			packet.Write(pos);
			packet.Write((int)window);
			Client.SendTCPData(packet);
		}
	}

	public void MouseHover()
	{
		// Replace with function body.
	}


	public void MouseLeave()
	{
		// Replace with function body.
	}
}

