using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Config : Node
{
    public static ItemData[] Items;
    public static MobData[] Mobs;

    public override void _Ready()
    {
        ReadConfigs();
    }

    public void ReadConfigs()
    {
        if (!LocaleReader.ReadItemData()) { GD.PrintErr("Failed to read item_data!"); }
        if (!LocaleReader.ReadMobData()) { GD.PrintErr("Failed to read mob_data!"); }
        GD.Print("Configs read successfully");
    }
}
