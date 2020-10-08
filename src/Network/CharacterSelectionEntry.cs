using System;
using System.Collections.Generic;
using System.Text;

public class CharacterSelectionEntry
{
    public int pid { get; }
    public string name { get; }
    public bool isValidCharacter = false;

    public CharacterSelectionEntry(int _pid, string _name, bool valid)
    {
        this.pid = _pid;
        this.name = _name;
        this.isValidCharacter = valid;
    }
}

