using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

class OtherPlayer
{
    public int pid { get; }
    public string name { get; }
    public Vector3 position { get; set; }

    public OtherPlayer(int _pid, string _name, Vector3 _pos)
    {
        this.pid = _pid;
        this.name = _name;
        this.position = _pos;
    }
}