using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS2Cheat
{
    public class Entity
    {
        public IntPtr pawnAddress { get; set; }
        public IntPtr controllerAddress { get; set; }
        public Vector3 position { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector2 position2D { get; set; }
        public Vector2 viewPosition2D { get; set; }
        public int team { get; set; }
        public int health { get; set; }
        public string name { get; set; }
        public uint lifeState { get; set; }
        public float distance { get; set; }
    }
}
