using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class LocaleReader
{
	public static bool ReadItemData()
	{
		if (!System.IO.File.Exists("locale/item_data"))
			return false;

		string raw = System.IO.File.ReadAllText("locale/item_data");
		if (raw == "")
			return false;

		try
		{
			int len = getFinalLocaleId(raw);
			ItemData[] items = new ItemData[len];
			string[] lines = raw.Split(System.Environment.NewLine.ToCharArray());
			for (int l = 0; l < lines.Length; l++)
			{
				if (lines[l] == "" || lines[l][0] == '#')
					continue;

				string[] line_contents = lines[l].Split('\t');
				Int32.TryParse(line_contents[0].ToString(), out int vnum);
				string name = line_contents[1];
				Int32.TryParse(line_contents[2].ToString(), out int level);

				List<PLAYER_RACES> races = new List<PLAYER_RACES>();
				if (line_contents[3].ToString() == "ALL")
				{
					races = Enum.GetValues(typeof(PLAYER_RACES)).Cast<PLAYER_RACES>().Select(v => v.ToString()).ToList().Select(x => Enum.Parse(typeof(PLAYER_RACES), x)).Cast<PLAYER_RACES>().ToList();
				}
				else
				{
					string[] races_string = line_contents[3].Split(',');
					for (int r = 0; r < races_string.Length; r++)
						races.Add((PLAYER_RACES)Enum.Parse(typeof(PLAYER_RACES), races_string[r]));
				}

				Int32.TryParse(line_contents[4].ToString(), out int _stacks);
				bool stacks = false;
				if (_stacks == 1)
					stacks = true;
				ITEM_TYPES type = (ITEM_TYPES)Enum.Parse(typeof(ITEM_TYPES), line_contents[5]);
				ITEM_SUB_TYPES sub_type = (ITEM_SUB_TYPES)Enum.Parse(typeof(ITEM_SUB_TYPES), line_contents[6]);
				BONUS_TYPE bonus_type0 = (BONUS_TYPE)Enum.Parse(typeof(BONUS_TYPE), line_contents[7]);
				float.TryParse(line_contents[8].ToString(), out float bonus_value0);
				BONUS_TYPE bonus_type1 = (BONUS_TYPE)Enum.Parse(typeof(BONUS_TYPE), line_contents[9]);
				float.TryParse(line_contents[10].ToString(), out float bonus_value1);
				BONUS_TYPE bonus_type2 = (BONUS_TYPE)Enum.Parse(typeof(BONUS_TYPE), line_contents[11]);
				float.TryParse(line_contents[12].ToString(), out float bonus_value2);
				BONUS_TYPE bonus_type3 = (BONUS_TYPE)Enum.Parse(typeof(BONUS_TYPE), line_contents[13]);
				float.TryParse(line_contents[14].ToString(), out float bonus_value3);
				BONUS_TYPE bonus_type4 = (BONUS_TYPE)Enum.Parse(typeof(BONUS_TYPE), line_contents[15]);
				float.TryParse(line_contents[16].ToString(), out float bonus_value4);
				BONUS_TYPE bonus_type5 = (BONUS_TYPE)Enum.Parse(typeof(BONUS_TYPE), line_contents[17]);
				float.TryParse(line_contents[18].ToString(), out float bonus_value5);

				ItemData iData = new ItemData(vnum, name, level, races.ToArray(), stacks, type, sub_type, bonus_type0, bonus_value0, bonus_type1, bonus_value1, bonus_type2, bonus_value2, bonus_type3, bonus_value3, bonus_type4, bonus_value4, bonus_type5, bonus_value5);
				items[vnum] = iData;
			}

			Config.Items = items.ToArray();
		}
		catch (Exception e) { GD.PrintErr(e.Message); return false; }

		return true;
	}

	public static bool ReadMobData()
	{
		if (!System.IO.File.Exists("locale/mob_data"))
			return false;

		string raw = System.IO.File.ReadAllText("locale/mob_data");
		if (raw == "")
			return false;

		try
		{
			int len = getFinalLocaleId(raw);
			MobData[] mobs = new MobData[len];
			string[] lines = raw.Split(System.Environment.NewLine.ToCharArray());
			for (int l = 0; l < lines.Length; l++)
			{
				if (lines[l] == "" || lines[l][0] == '#')
					continue;

				string[] line_contents = lines[l].Split('\t');
				Int32.TryParse(line_contents[0].ToString(), out int id);
				string name = line_contents[1];
				float.TryParse(line_contents[2].ToString(), out float moveSpeed);
				float.TryParse(line_contents[2].ToString(), out float attSpeed);

				MobData data = new MobData(id, name, moveSpeed, attSpeed);
				mobs[id] = data;
			}

			Config.Mobs = mobs.ToArray();
		}
		catch (Exception e) { GD.PrintErr(e.Message); return false; }

		return true;
	}

	private static int getFinalLocaleId(string contents)
	{
		int len = 1;
		string[] lines = contents.Split('\n');
		string[] line_contents = lines[lines.Length - 1].Split('\t');
		Int32.TryParse(line_contents[0], out len);
		return len + 1;
	}
}
