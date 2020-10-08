using System;
using System.Collections.Generic;
using System.Text;

public class PlayerStats
{
    public int movementSpeed;
    public int attackSpeed;

    public PlayerStats(int _movementSpeed = 1, int _attackSpeed = 1)
    {
        this.movementSpeed = _movementSpeed;
        this.attackSpeed = _attackSpeed;
    }
}
