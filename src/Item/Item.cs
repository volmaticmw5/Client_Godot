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
		STORAGE
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
		if (window == WINDOW.INVENTORY)
		{
			clearSlotModulates();
			Inventory.slots[slotPos].Modulate = new Color(1f, 0.8f, 1f);
			if(data.size > 1 && slotPos + Inventory.instance.InventoryWidth < Inventory.slots.Length)
			{
				if (Inventory.slots[slotPos + Inventory.instance.InventoryWidth] != null)
					Inventory.slots[slotPos + Inventory.instance.InventoryWidth].Modulate = new Color(1f, 0.8f, 1f);
			}
		}
	}

	private void clearSlotModulates()
	{
		for (int i = 0; i < Inventory.slots.Length; i++)
			if (Inventory.slots[i] != null)
				Inventory.slots[i].Modulate = new Color(1f, 1f, 1f);
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
					GD.Print("use item");
					//currentSlot.item.RequestUse();
					//nItemDesc.HideDesc();
					//nItemDescWeapon.HideDesc();
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
		RectGlobalPosition = Inventory.slots[position].RectGlobalPosition;
		if (data.size == 1)
			SetSize(new Vector2(25f, 25f));
		UpdateItemCountLabel();
		instanced = false;
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
		if (lastPos == position)
			return;

		Name = "icon_" + data.vnum + "_" + position;
		RectGlobalPosition = Inventory.slots[position].RectGlobalPosition;

		lastPos = position;
	}

	private int getSlotPositionUnderMouse()
	{
		foreach (Control slot in Inventory.slots)
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
		RectGlobalPosition = Inventory.slots[position].RectGlobalPosition;
	}

	private WINDOW getTargetWindow()
	{
		bool isInventory = false;
		foreach (Control slot in Inventory.slots)
		{
			if (slot == null)
				continue;

			if (slot.GetGlobalRect().HasPoint(new Vector2(GetGlobalRect().Position.x + (RectSize.x / 2), GetGlobalRect().Position.y)))
			{
				isInventory = true;
				break;
			}
		}

		if (isInventory && window == WINDOW.INVENTORY)
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
		if (targetWindow == WINDOW.INVENTORY)
		{
			int pos = getSlotPositionUnderMouse();
			requestItemMove(pos);
			interruptDragging();
		}
		else if(targetWindow == WINDOW.STORAGE)
		{
			GD.Print($"move to {targetWindow.ToString()} window");
			interruptDragging();
		}
		else if(targetWindow == WINDOW.NONE)
		{
			GD.Print("thow item away?");
			interruptDragging();
		}

		clearSlotModulates();
	}

	private void requestItemMove(int newPos)
	{
		using (Packet packet = new Packet((int)ClientPackets.itemChangePosition))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			packet.Write(iid);
			packet.Write(newPos);
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

