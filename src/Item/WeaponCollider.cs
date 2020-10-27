using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

class WeaponCollider : Spatial
{ 
	private void _on_Area_area_entered(Node body)
	{
		if (!Player.instance.attacking)
			return;

		Mob mob = body.GetParentOrNull<Mob>();
		if (mob != null)
			sendHitSignalToServer(mob.mid);
	}

	private void sendHitSignalToServer(int mid)
	{
		using (Packet packet = new Packet((int)ClientPackets.weaponHit))
		{
			packet.Write(Client.instance.getCID());
			packet.Write(Client.instance.getSessionId());
			packet.Write(mid);
			Client.SendTCPData(packet);
		}
	}
}



