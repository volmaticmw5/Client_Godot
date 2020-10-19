using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Console : Control
{
	[Export] public NodePath InputNodePath;
	[Export] public NodePath ConsoleContentsPath;

	private LineEdit inputNode;
	private RichTextLabel consoleContents;
	private bool typing = false;
	private static Console instance;

	public override void _Ready()
	{
		instance = this;
		inputNode = GetNode<LineEdit>(InputNodePath);
		consoleContents = GetNode<RichTextLabel>(ConsoleContentsPath);
	}

	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventKey key)
		{
			if(key.Scancode == Keybinds.KEYBIND_ACCEPT)
			{
				if (!typing)
					openConsole();
				else
					closeConsole();
			}
		}
	}

	private void openConsole()
	{
		//add to guimanager queue, block player movement
		inputNode.GrabFocus();
		typing = true;
	}

	private void closeConsole()
	{
		if(inputNode.Text != "")
			sendChatEvent(inputNode.Text);

		inputNode.Clear();
		inputNode.ReleaseFocus();
		typing = false;
	}

	private void addEntryToConsole(string entry)
	{
		consoleContents.Text += $"\n{entry}";
	}

	private void sendChatEvent(string msg)
	{
		if (!Client.instance.isConnected())
			return;

		using (Packet packet = new Packet((int)ClientPackets.chatMsg))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			packet.Write(msg);

			Client.SendTCPData(packet);
		}
	}

	private void _on_LineEdit_text_entered(String new_text)
	{
		closeConsole();
	}

	internal static void NewConsoleEntry(Packet packet)
	{
		instance.addEntryToConsole(packet.ReadString());
	}

	internal static void UnknownCmdCB(Packet packet)
	{
		throw new NotImplementedException();
	}
}

