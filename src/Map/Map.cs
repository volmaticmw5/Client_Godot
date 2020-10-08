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
        int playerCount = packet.ReadInt();
        List<PlayerData> playersData = new List<PlayerData>();
        for (int i = 0; i < playerCount; i++)
            playersData.Add(packet.ReadPlayerData());

        RemoveNoLongerVisiblePlayers(playersData.ToArray());
        foreach (PlayerData player in playersData)
        {
            if (playerAlreadyInThisMap(player))
                updateThisOtherPlayer(player);
            else
                createOtherPlayer(player);
        }

        DrawDebugInfo();


        // Send the server OUR position , this should be moved somewhere else
        if(Player.IsReady())
            Player.Broadcast();
    }

    private static void RemoveNoLongerVisiblePlayers(PlayerData[] playersData)
    {
        bool exists = false;
        for (int i = 0; i < visiblePlayers.Count; i++)
        {
            exists = false;
            foreach (PlayerData pdata in playersData)
            {
                if (visiblePlayers[i].pid == pdata.pid)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                RemoveOtherPlayerInstanceFromMap(visiblePlayers[i]);
                visiblePlayers.RemoveAt(i);
            }
        }
    }

    private static void RemoveOtherPlayerInstanceFromMap(OtherPlayer player)
    {
        player.QueueFree();
    }

    private static void DrawDebugInfo()
    {
        if (Player.instance != null)
        {
            DebugTexts.toDraw = $"({Player.instance.Transform.origin.x},{Player.instance.Transform.origin.y},{Player.instance.Transform.origin.z}) | '{Player.instance.name}'\n";
            foreach (OtherPlayer player in visiblePlayers)
            {
                DebugTexts.toDraw += $"Player #{player.pid}, '{player.name}' ({player.position.x},{player.position.y},{player.position.z})\n";
            }
        }
    }

    private static void createOtherPlayer(PlayerData player)
    {
        PackedScene nOtherScene = (PackedScene)ResourceLoader.Load($"res://prefabs/OtherPlayer.tscn");
        OtherPlayer otherPlayer = nOtherScene.Instance() as OtherPlayer;
        SceneManager.GetInstance().GetTree().Root.GetNodeOrNull("Game").CallDeferred("add_child", otherPlayer);
        otherPlayer.Init(player);
        visiblePlayers.Add(otherPlayer);
    }

    private static void updateThisOtherPlayer(PlayerData player)
    {
        for (int i = 0; i < visiblePlayers.Count; i++)
        {
            if (visiblePlayers[i].pid == player.pid)
                visiblePlayers[i].UpdateThisPlayer(player);
        }
    }

    private static bool playerAlreadyInThisMap(PlayerData player)
    {
        for (int i = 0; i < visiblePlayers.Count; i++)
        {
            if (visiblePlayers[i].pid == player.pid)
                return true;
        }

        return false;
    }
}