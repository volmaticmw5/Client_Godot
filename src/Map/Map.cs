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
                        string name = pData[1];
                        float.TryParse(pData[2], out float x);
                        float.TryParse(pData[3], out float y);
                        float.TryParse(pData[4], out float z);

                        bool exists = false;
                        for (int i = 0; i < visiblePlayers.Count; i++)
                        {
                            if(visiblePlayers[i].pid == pid)
                            {
                                // Update its position, equipment, level, etc...
                                visiblePlayers[i].position = new System.Numerics.Vector3(x, y, z);

                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            OtherPlayer nPlayer = new OtherPlayer(pid, name, new System.Numerics.Vector3(x, y, z));
                            visiblePlayers.Add(nPlayer);
                        }
                    }
                }
            }
        }

        // Send the server OUR position
        Player.SendMyPosition();
    }
}