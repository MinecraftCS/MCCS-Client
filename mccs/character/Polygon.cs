﻿using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.character
{
    public class Polygon
    {
        public Vertex[] vertices;

        public Polygon(Vertex[] vertices)
        {
            this.vertices = vertices;
        }

        public Polygon(Vertex[] vertices, int u0, int v0, int u1, int v1) : this(vertices)
        {
            vertices[0] = vertices[0].remap(u1, v0);
            vertices[1] = vertices[1].remap(u0, v0);
            vertices[2] = vertices[2].remap(u0, v1);
            vertices[3] = vertices[3].remap(u1, v1);
        }

        public void render()
        {
            GL.Color3(1.0f, 1.0f, 1.0f);
            for (int i = 3; i >= 0; i--)
            {
                Vertex v = vertices[i];
                GL.TexCoord2(v.u / 64.0f, v.v / 32.0f);
                GL.Vertex3(v.pos.x, v.pos.y, v.pos.z);
            }
        }
    }
}
