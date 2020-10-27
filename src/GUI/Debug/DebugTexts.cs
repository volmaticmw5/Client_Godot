using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

class DebugTexts : RichTextLabel
{
	public static string toDraw;

	public override void _Process(float delta)
	{
		Text = toDraw;
	}
}
