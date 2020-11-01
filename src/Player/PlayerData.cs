using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

public class PlayerData
{
	public int pid;
	public int aid;
	public int sid;
	public string name;
	public int level;
	public int map;
	public PLAYER_SEXES sex;
	public PLAYER_RACES race;
	public Vector3 pos;
	public int heading;
	public PlayerStats stats;
	public bool attacking;
	public float hp;
	public float mana;

	public PlayerData(int _pid, int _aid, int _sid, string _name, int _level, int _map, PLAYER_SEXES _sex, PLAYER_RACES _race, Vector3 _pos, int _heading, PlayerStats _stats, bool _attacking, float _maxHp, float _maxMana, float _hp, float _mana, float _pdef, float _mdef)
	{
		this.pid = _pid;
		this.aid = _aid;
		this.sid = _sid;
		this.name = _name;
		this.level = _level;
		this.map = _map;
		this.sex = _sex;
		this.race = _race;
		this.pos = _pos;
		this.heading = _heading;
		this.stats = _stats;
		this.attacking = _attacking;
		this.hp = _hp;
		this.mana = _mana;
		this.stats.maxHp = _maxHp;
		this.stats.maxMana = _maxMana;
		this.stats.pDefense = _pdef;
		this.stats.mDefense = _mdef;
	}
}