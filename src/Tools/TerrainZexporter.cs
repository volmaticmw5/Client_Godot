using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Godot;

[Tool]
public class TerrainZexporter : RayCast
{
	[Export] public bool ExportMapData = false;
	[Export] public Vector2 StartingPosition;
	[Export] public string TerrainNodeName = "HTerrain";
	[Export] public Vector2 Size;
	[Export] public float MaxHeight = 10f;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed)
			{
				if (eventKey.Scancode == (int)KeyList.F1)
					ExportMapData = true;
			}
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		if (ExportMapData)
		{
			if (StartingPosition == null || Size == null)
			{
				ExportMapData = false;
				GD.PrintErr("Invalid starting or ending positions, map data exporting failed.");
				return;
			}

			process();
		}
	}

	private void process()
	{
		float[,] data = new float[(int)(Size.x), (int)(Size.y)];
		long starting_time = MathHelper.TimestampMiliseconds();
		ExportMapData = false;
		GD.Print("Starting map export...");
		Transform t = Transform;
		t.origin = new Vector3(StartingPosition.x, MaxHeight, StartingPosition.y);
		Transform = t;

		for (int x = 0; x < (int)(Size.x); x++)
		{
			for (int y = 0; y < (int)(Size.y); y++)
			{
				data[x, y] = 0f;
				var spaceState = GetWorld().DirectSpaceState;
				var result = spaceState.IntersectRay(t.origin, new Vector3(t.origin.x, -1000f, t.origin.z));
				if(result.Count > 0)
				{
					Vector3 point = (Vector3)result["position"];
					if (point.y != 0f)
						data[x, y] = (float)Math.Round(point.y, 4);
				}

				t.origin = new Vector3(StartingPosition.x + (float)x, MaxHeight, StartingPosition.y + (float)y);
				Transform = t;
			}
		}

		string out_file_name = "map_dump_" + MathHelper.TimestampMiliseconds();
		using (Stream stream = System.IO.File.Open(out_file_name, FileMode.Create))
		{
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, data);
		}

		t.origin = new Vector3(StartingPosition.x, StartingPosition.y, 0f);
		Transform = t;

		long elapsed_time = Math.Abs(MathHelper.TimestampMiliseconds() - starting_time);
		GD.Print($"Finished exporing map to {out_file_name} in {elapsed_time} ms");
	}
}
