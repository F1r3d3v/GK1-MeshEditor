﻿using System.Numerics;

namespace GK1_MeshEditor
{
    internal class Vertex
    {
        public Vector3 P { get; set; }
        public Vector3 Pu { get; set; }
        public Vector3 Pv { get; set; }
        public Vector3 N { get; set; }
        public Vector2 UV { get; set; }

        public Vertex(Vector3 p, Vector3 pu, Vector3 pv, Vector2 uv)
        {
            P = p;
            Pu = Vector3.Normalize(pu);
            Pv = Vector3.Normalize(pv);
            N = Vector3.Normalize(Vector3.Cross(pu, pv));
            UV = uv;
        }
    }
}
