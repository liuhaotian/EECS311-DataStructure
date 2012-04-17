// Copyright 2007, 2008, 2009 Ian Horswill
// This file is part of Twig.
//
// Twig is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 of
//  the License, or (at your option) any later version.
//
// Twig is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Twig.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HappyFunBlob
{
    /// <summary>
    /// Triangle mesh consisting of a single triangle list, strip, or fan.
    /// </summary>
    public class SimpleMesh : IDisposable
    {
        /// <summary>
        /// Buffer for the mesh's vertices
        /// </summary>
        VertexBuffer vertexBuffer;
        /// <summary>
        /// Number of triangles in mesh
        /// </summary>
        int count;
        /// <summary>
        /// Mesh format (TriangleList, TriangleStrip, TriangleFan).
        /// </summary>
        PrimitiveType type;

        /// <summary>
        /// Deallocates graphics card memory associated with this object.
        /// </summary>
        public void Dispose()
        {
            vertexBuffer.Dispose();
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        /// <summary>
        /// Creates a SimpleMesh
        /// </summary>
        public SimpleMesh(GraphicsDevice device, PrimitiveType type, VertexPositionNormalTexture[] vertices, int count) {
            this.count = count;
            this.type = type;
            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
        }

        /// <summary>
        /// Makes a SimpleMesh from a MeshBuilder
        /// </summary>
        /// <param Name="device"></param>
        /// <param Name="builder"></param>
        public SimpleMesh(GraphicsDevice device, MeshBuilder<VertexPositionNormalTexture> builder)
        {
            this.count = builder.Count-2;
            this.type = PrimitiveType.TriangleStrip;
            vertexBuffer = builder.MakeVertexBuffer(device, BufferUsage.WriteOnly);
            //vertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
        }

        /// <summary>
        /// Draws the mesh on its device.
        /// </summary>
        public void Draw() {
            GraphicsDevice device = vertexBuffer.GraphicsDevice;
            // Uncomment to disable backface culling to test meshes.
            //device.RenderState.CullMode = CullMode.None;
            //device.VertexDeclaration = vertexDeclaration;
            //device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            //device.DrawPrimitives(type, 0, count);
            device.SetVertexBuffer(vertexBuffer);
            device.DrawPrimitives(type, 0, count);
        }

        /// <summary>
        /// Makes a triangle strip mesh.
        /// </summary>
        public static SimpleMesh TriangleStrip(GraphicsDevice device, VertexPositionNormalTexture[] vertices)
        {
            return new SimpleMesh(device, PrimitiveType.TriangleStrip, vertices, vertices.Length-2);
        }

        /// <summary>
        /// Makes a triangle list mesh.
        /// </summary>
        public static SimpleMesh TriangleList(GraphicsDevice device, params VertexPositionNormalTexture[] verticies)
        {
            return new SimpleMesh(device, PrimitiveType.TriangleList, verticies, verticies.Length - 2);
        }

        static Vector3 XPLUS = new Vector3(1,  0,  0);	// X
        static Vector3 XMIN  = new Vector3(-1,  0,  0);	// -X
        static Vector3 YPLUS = new Vector3(0,  1,  0);	//  Y
        static Vector3 YMIN  = new Vector3(0, -1,  0);	// -Y
        static Vector3 ZPLUS = new Vector3(0,  0,  1);	//  Z
        static Vector3 ZMIN  = new Vector3(0,  0, -1);	// -Z


        /// <summary>
        /// Creates a SimpleMesh containing a sphere.
        /// </summary>
        public static SimpleMesh Sphere(GraphicsDevice device, float radius, int level, Vector3 position)
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            // Vertices of a unit octahedron
            Triangulate(vertices, radius, level, XPLUS, ZPLUS, YPLUS);
            Triangulate(vertices, radius, level, YPLUS, ZPLUS, XMIN);
            Triangulate(vertices, radius, level, XMIN, ZPLUS, YMIN);
            Triangulate(vertices, radius, level, YMIN, ZPLUS, XPLUS);
            Triangulate(vertices, radius, level, XPLUS, YPLUS, ZMIN);
            Triangulate(vertices, radius, level, YPLUS, XMIN, ZMIN);
            Triangulate(vertices, radius, level, XMIN, YMIN, ZMIN);
            Triangulate(vertices, radius, level, YMIN, XPLUS, ZMIN);

            for (int i = 0; i < vertices.Count; i++)
            {
                VertexPositionNormalTexture v = vertices[i];
                v.Position += position;
                vertices[i] = v;
            }

            return new SimpleMesh(device, PrimitiveType.TriangleList, vertices.ToArray(), vertices.Count / 3);
        }

        public static SimpleMesh Box(GraphicsDevice device, Vector3 center, float width, float height, float depth)
        {
            MeshBuilder<VertexPositionNormalTexture> b = new MeshBuilder<VertexPositionNormalTexture>();
            Vector3 halfWidth = 0.5f * width * Vector3.UnitX;
            Vector3 halfHeight = 0.5f * height * Vector3.UnitY;
            Vector3 halfDepth = 0.5f * depth * Vector3.UnitZ;

            // Front
            b.AddQuad(new VertexPositionNormalTexture(center - halfWidth + halfHeight + halfDepth, Vector3.UnitZ, Vector2.Zero),
                        new VertexPositionNormalTexture(center + halfWidth + halfHeight + halfDepth, Vector3.UnitZ, Vector2.Zero),
                        new VertexPositionNormalTexture(center - halfWidth - halfHeight + halfDepth, Vector3.UnitZ, Vector2.Zero),
                        new VertexPositionNormalTexture(center + halfWidth - halfHeight + halfDepth, Vector3.UnitZ, Vector2.Zero));

            // Back
            b.AddQuad(new VertexPositionNormalTexture(center + halfWidth + halfHeight - halfDepth, -Vector3.UnitZ, Vector2.Zero),
                        new VertexPositionNormalTexture(center - halfWidth + halfHeight - halfDepth, -Vector3.UnitZ, Vector2.Zero),
                        new VertexPositionNormalTexture(center + halfWidth - halfHeight - halfDepth, -Vector3.UnitZ, Vector2.Zero),
                        new VertexPositionNormalTexture(center - halfWidth - halfHeight - halfDepth, -Vector3.UnitZ, Vector2.Zero));

            return new SimpleMesh(device, b);
        }



        static void Triangulate(List<VertexPositionNormalTexture> vertices, float radius, int level, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            p1 *= radius / p1.Length();
            p2 *= radius / p2.Length();
            p3 *= radius / p3.Length();

            if (level == 0)
            {
                Vector3 n1 = p1; // +MathUtil.NoiseVector(radius * normalNoiseLevel);
                Vector3 n2 = p1; // +MathUtil.NoiseVector(radius * normalNoiseLevel);
                Vector3 n3 = p1; // +MathUtil.NoiseVector(radius * normalNoiseLevel);
                n1.Normalize();
                n2.Normalize();
                n3.Normalize();

                vertices.Add(new VertexPositionNormalTexture(p1, n1, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(p2, n2, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(p3, n3, Vector2.Zero));
            }
            else
            {
                Vector3 m12 = (p1 + p2) * 0.5f;
                Vector3 m23 = (p2 + p3) * 0.5f;
                Vector3 m31 = (p3 + p1) * 0.5f;

                Triangulate(vertices, radius, level - 1, p1, m12, m31);
                Triangulate(vertices, radius, level - 1, m12, p2, m23);
                Triangulate(vertices, radius, level - 1, m31, m23, p3);
                Triangulate(vertices, radius, level - 1, m12, m23, m31);
            }
        }
    }
}
