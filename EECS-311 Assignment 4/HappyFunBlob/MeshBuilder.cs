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
    /// Creates vertex lists for triangle strips to represent arbitrary combinations of triangle strips and fans.
    /// Note: not efficient for triangle lists
    /// </summary>
    public class MeshBuilder<TVertex> where TVertex:struct
    {
        /// <summary>
        /// Makes a Meshbuilder for a mesh with the specified vertex type.  However, you need to manually specify the element size.
        /// </summary>
        public MeshBuilder()
        { }

        #region High-level building operations
        /// <summary>
        /// Start a new triangle strip
        /// </summary>
        public void StartTriangleStrip(TVertex start1, TVertex start2)
        {
            if (!IsEmpty) {
                // Add to empty triangles to allow a break from the previous strip
                AddVertex(LastVertex);
                AddVertex(start1);
            }
            AddVertex(start1);
            AddVertex(start2);
            mode = Mode.strip;
        }

        /// <summary>
        /// Start a new triangle fan
        /// </summary>
        public void StartTriangleFan(TVertex center, TVertex start1)
        {
            if (!IsEmpty)
            {
                // Add to empty triangles to allow a break from the previous strip
                AddVertex(LastVertex);
                AddVertex(start1);
            }
            AddVertex(start1);
            AddVertex(center);
            mode = Mode.fan;
            this.center = center;
        }

        public void AddQuad(TVertex a, TVertex b, TVertex c, TVertex d)
        {
            StartTriangleStrip(d, a);
            AddTriangle(c);
            AddTriangle(b);
        }


        /// <summary>
        /// Add a new triangle to the current segment (strip or fan)
        /// </summary>
        public void AddTriangle(TVertex newVertex)
        {
            if (mode == Mode.fan)
                AddVertex(center);
            AddVertex(newVertex);
        }
        #endregion

        #region VertexBuffer operations
        /// <summary>
        /// Write all generated vertices to VertexBuffer
        /// </summary>
        public void WriteVertexBuffer(VertexBuffer b) {
            b.SetData<TVertex>(vertices, 0, vertexCount);
        }

        /// <summary>
        /// Make a new VertexBuffer containing this geometry.
        /// </summary>
        public VertexBuffer MakeVertexBuffer(GraphicsDevice d, BufferUsage usage)
        {
            VertexBuffer b = new VertexBuffer(d, typeof(TVertex), vertexCount, usage);
            WriteVertexBuffer(b);
            return b;
        }
        #endregion

        #region State variables
        enum Mode { strip, fan };

        Mode mode = Mode.strip;
        TVertex center;  // center vertex when rendering a triangle fan.

        /// <summary>
        /// Holds the actual vertices as a trianglelist
        /// The right thing to do would be to use List&lt;V&gt;, but then we can't ask
        /// for the underlying array.  So we have to use a real array and manage it ourselves.
        /// </summary>
        TVertex[] vertices = new TVertex[30];

        /// <summary>
        /// The actual number of vertices stored in vertices.
        /// </summary>
        int vertexCount;
        #endregion

        #region Low-level vertex accessors/mutators
        TVertex LastVertex
        {
            get
            {
                return vertices[vertexCount - 1];
            }
        }

        /// <summary>
        /// True if there are no vertices in the mesh
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return vertexCount == 0;
            }
        }

        /// <summary>
        /// Number of vertices in the mesh (*not* the number of triangles)
        /// </summary>
        public int Count
        {
            get
            {
                return vertexCount;
            }
        }

        /// <summary>
        /// Adds one vertex to the list.
        /// </summary>
        /// <param name="v"></param>
        void AddVertex(TVertex v)
        {
            CheckSpace(1);
            vertices[vertexCount++] = v;
        }

        /// <summary>
        /// Ensure there is enough room in the vertices buffer for at least newVertexCount vertices.
        /// </summary>
        void CheckSpace(int newVertexCount)
        {
            if (vertexCount + newVertexCount >= vertices.Length)
            {
                TVertex[] newBuffer = new TVertex[vertices.Length * 2];
                vertices.CopyTo(newBuffer, 0);
                vertices = newBuffer;
            }
        }
        #endregion
    }

#if notdef
    /// <summary>
    /// Creates vertex lists for triangle strips to represent arbitrary combinations of triangle strips and fans.
    /// Note: not efficient for triangle lists
    /// </summary>
    public class MeshBuilder<TVertex> where TVertex:struct
    {
        /// <summary>
        /// Makes a Meshbuilder for a mesh with the specified vertex type.  However, you need to manually specify the element size.
        /// </summary>
        /// <param Name="elementSize"></param>
        public MeshBuilder(int elementSize)
        {
            this.elementSize = elementSize;
        }

        #region High-level building operations
        /// <summary>
        /// Start a new triangle strip
        /// </summary>
        public void StartTriangleStrip(TVertex start1, TVertex start2)
        {
            if (!IsEmpty) {
                // Add to empty triangles to allow a break from the previous strip
                AddVertex(LastVertex);
                AddVertex(start1);
            }
            AddVertex(start1);
            AddVertex(start2);
            mode = Mode.strip;
        }

        /// <summary>
        /// Start a new triangle fan
        /// </summary>
        public void StartTriangleFan(TVertex centerVertex, TVertex startVertex)
        {
            if (!IsEmpty)
            {
                // Add to empty triangles to allow a break from the previous strip
                AddVertex(LastVertex);
                AddVertex(startVertex);
            }
            AddVertex(startVertex);
            AddVertex(centerVertex);
            mode = Mode.fan;
            this.center = centerVertex;
        }

        /// <summary>
        /// Add a new triangle to the current segment (strip or fan)
        /// </summary>
        public void AddTriangle(TVertex newVertex)
        {
            if (mode == Mode.fan)
                AddVertex(center);
            AddVertex(newVertex);
        }

        public void AddQuad(TVertex a, TVertex b, TVertex c, TVertex d)
        {
            StartTriangleStrip(d, a);
            AddTriangle(c);
            AddTriangle(b);
        }
        #endregion

        #region VertexBuffer operations
        /// <summary>
        /// Write all generated vertices to VertexBuffer
        /// </summary>
        public void WriteVertexBuffer(VertexBuffer vertexBuffer) {
            vertexBuffer.SetData<TVertex>(vertices, 0, vertexCount);
        }

        /// <summary>
        /// Make a new VertexBuffer containing this geometry.
        /// </summary>
        public VertexBuffer MakeVertexBuffer(GraphicsDevice device, BufferUsage usage)
        {
            VertexBuffer b = new VertexBuffer(device, elementSize*vertexCount, usage);
            WriteVertexBuffer(b);
            return b;
        }
        #endregion

        #region State variables
        enum Mode { strip, fan };

        int elementSize;

        Mode mode = Mode.strip;
        TVertex center;  // center vertex when rendering a triangle fan.

        /// <summary>
        /// Holds the actual vertices as a trianglelist
        /// The right thing to do would be to use List&lt;V&gt;, but then we can't ask
        /// for the underlying array.  So we have to use a real array and manage it ourselves.
        /// </summary>
        TVertex[] vertices = new TVertex[30];

        /// <summary>
        /// The actual number of vertices stored in vertices.
        /// </summary>
        int vertexCount;
        #endregion

        #region Low-level vertex accessors/mutators
        TVertex LastVertex
        {
            get
            {
                return vertices[vertexCount - 1];
            }
        }

        /// <summary>
        /// True if there are no vertices in the mesh
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return vertexCount == 0;
            }
        }

        /// <summary>
        /// Number of vertices in the mesh (*not* the number of triangles)
        /// </summary>
        public int Count
        {
            get
            {
                return vertexCount;
            }
        }

        /// <summary>
        /// Adds one vertex to the list.
        /// </summary>
        /// <param Name="v"></param>
        void AddVertex(TVertex v)
        {
            CheckSpace(1);
            vertices[vertexCount++] = v;
        }

        /// <summary>
        /// Ensure there is enough room in the vertices buffer for at least newVertexCount vertices.
        /// </summary>
        void CheckSpace(int newVertexCount)
        {
            if (vertexCount + newVertexCount >= vertices.Length)
            {
                TVertex[] newBuffer = new TVertex[vertices.Length * 2];
                vertices.CopyTo(newBuffer, 0);
                vertices = newBuffer;
            }
        }
        #endregion
    }
#endif
}