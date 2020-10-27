using System;
using System.Collections.Generic;
using System.Text;

public class MobData
{
    public int id;
    public string name;
    public float movementSpeed;
    public float attackSpeed;

    public MobData(int _id, string _name, float _moveSpeed, float _attSpeed)
    {
        this.id = _id;
        this.name = _name;
        this.movementSpeed = _moveSpeed;
        this.attackSpeed = _attSpeed;
    }
}