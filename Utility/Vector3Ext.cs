using UnityEngine;

namespace KitchenRenovation.Utility
{
    public static class Vector3Ext
    {
        public static Vector3Int ToInt(this Vector3 v) => new Vector3Int((int)v.x, (int)v.y, (int)v.z);
        public static Vector3 ToFloat(this Vector3Int v) => new Vector3(v.x, v.y, v.z);
    }
}
