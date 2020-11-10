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
	public int exp;
	public int vit;
	public int str;
	public int _int;
	public int dex;
	public int map;
	public PLAYER_SEXES sex;
	public PLAYER_RACES race;
	public Vector3 pos;
	public int heading;
	public PlayerStats stats;
	public ANIMATION_STATES animation_state;
	public float hp;
	public float mana;
	public float maxHp;
	public float maxMana;

	public PlayerData(int _pid, string _name, int _level, int _map, PLAYER_SEXES _sex, PLAYER_RACES _race, Vector3 _pos, int _heading, PlayerStats _stats, ANIMATION_STATES _animation_state, int _aid = 0, int _sid = 0, float _maxHp = 100, float _hp = 100, float _mn = 100, float _maxMn = 100, int _exp = 0, int _vit = 0, int _str = 0, int _int = 0, int _dex = 0)
	{

		this.pid = _pid;
		this.aid = _aid;
		this.sid = _sid;
		this.name = _name;
		this.level = _level;
		this.exp = _exp;
		this.vit = _vit;
		this.str = _str;
		this._int = _int;
		this.dex = _dex;
		this.map = _map;
		this.sex = _sex;
		this.race = _race;
		this.pos = _pos;
		this.stats = _stats;
		this.heading = _heading;
		this.animation_state = _animation_state;
		this.maxHp = _maxHp;
		this.maxMana = _maxMn;
		this.hp = _hp;
		this.mana = _mn;
	}
}