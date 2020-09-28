using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Map
{
    public int id { get; }
    private static List<OtherPlayer> visiblePlayers = new List<OtherPlayer>();

    public Map(int _id)
    {
        this.id = _id;
    }

    public static void HandlePlayersInMap(Packet packet)
    {
        int cid = packet.ReadInt();
        string data = packet.ReadString();

        if(data != "")
        {
            string[] players = data.Split(new [] { "/end/" }, StringSplitOptions.None);
            if(players.Length > 0)
            {
                foreach (string playerData in players)
                {
                    if(playerData != "")
                    {
                        string[] pData = playerData.Split(';');
                        Int32.TryParse(pData[0], out int pid);
                        Int32.TryParse(pData[1], out int race);
                        Int32.TryParse(pData[2], out int sex);
                        string name = pData[3];
                        float.TryParse(pData[4], out float x);
                        float.TryParse(pData[5], out float y);
                        float.TryParse(pData[6], out float z);

                        bool exists = false;
                        for (int i = 0; i < visiblePlayers.Count; i++)
                        {
                            if(visiblePlayers[i].pid == pid)
                            {
                                // Update its position, equipment, level, etc...
                                visiblePlayers[i].position = new Godot.Vector3(x, y, z);

                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            PackedScene nOtherScene = (PackedScene)ResourceLoader.Load($"res://prefabs/OtherPlayer.tscn");
                            OtherPlayer otherPlayer = nOtherScene.Instance() as OtherPlayer;
                            otherPlayer.Init(pid, race, sex, name, new Godot.Vector3(x, y, z));
                            SceneManager.GetInstance().GetTree().Root.GetNodeOrNull("Game").CallDeferred("add_child", otherPlayer);
                            visiblePlayers.Add(otherPlayer);
                        }
                    }
                }
            }
        }

        // For debug only /////////
        if(Player.instance != null)
        {
            DebugTexts.toDraw = $"({Player.instance.Transform.origin.x},{Player.instance.Transform.origin.y},{Player.instance.Transform.origin.z}) | '{Player.instance.name}'\n";
            foreach (OtherPlayer player in visiblePlayers)
            {
                DebugTexts.toDraw += $"Player #{player.pid}, '{player.name}' ({player.position.x},{player.position.y},{player.position.z})\n";
            }
        }
        /////////////////////////

        // Send the server OUR position
        if(Player.IsReady())
            Player.SendMyPosition();
    }
}