using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class iCharSlot
{
	private int pid;
	private string name;

	public iCharSlot(int _pid, string _name)
	{
		this.pid = _pid;
		this.name = _name;
	}

	public int getPid()
	{
		return this.pid;
	}

	public string getName()
	{
		return this.name;
	}
}

public class CharSlot : Node
{
	private iCharSlot slot;

	public void AssignAttr(iCharSlot _slot)
	{
		this.slot = _slot;
	}

	public int getPid()
	{
		return this.slot.getPid();
	}

	public string getName()
	{
		return this.slot.getName();
	}

	public void onSelect()
	{
		CharSelection.selectedPid = this.slot.getPid();
		CharSelection.UpdatePlayerButtonStatus();
	}
}
