using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

class MathHelper
{
    public static float Lerp(float from, float to, float time)
    {
        return (from + time * (to - from));
    }

    public static Vector2 Lerp(Vector2 from, Vector2 to, float by)
    {
        float retX = Lerp(from.X, to.X, by);
        float retY = Lerp(from.Y, to.Y, by);
        return new Vector2(retX, retY);
    }

    public static Vector3 Lerp(Vector3 from, Vector3 to, float by)
    {
        float retX = Lerp(from.X, to.X, by);
        float retY = Lerp(from.Y, to.Y, by);
        float retZ = Lerp(from.Z, to.Z, by);
        return new Vector3(retX, retY, retZ);
    }

    public static float Distance(Vector2 from, Vector2 to)
    {
        return Vector2.Distance(from, to);
    }

    public static float Distance(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }

    public static float Distance(Godot.Vector3 from, Godot.Vector3 to)
    {
        return Vector3.Distance(new Vector3(from.x, from.y, from.z), new Vector3(to.x, to.y, to.z));
    }

    public static int TimestampSeconds()
    {
        return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    public static long TimestampMiliseconds()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
