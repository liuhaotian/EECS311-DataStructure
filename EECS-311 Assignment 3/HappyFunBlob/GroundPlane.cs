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
    /// A simple green ground plane.
    /// </summary>
    public class GroundPlane : Obstacle
    {
        #region Instance variables
        /// <summary>
        /// Mesh for the ground plane.
        /// </summary>
        SimpleMesh groundPlane;
        /// <summary>
        /// Effect (i.e. shader program) to use when rendering ground plane mesh
        /// </summary>
        BasicEffect groundPlaneEffect;
        #endregion

        /// <summary>
        /// Makes a simple green ground plane.
        /// </summary>
        public GroundPlane(HappyFunBlobGame game)
            : base(game, Vector3.Zero)
        {
        }

        /// <summary>
        /// The bbox for the ground plane is infinite
        /// </summary>
        public override BoundingBox BoundingBox
        {
            get {
                Vector3 offset = new Vector3(999999999, 0.001f, 999999999);
                return new BoundingBox(-offset, offset);
            }
        }

        /// <summary>
        /// Handle amoeba vertices that are below ground.
        /// This is a simple test of the Y coordinate, followed by correcting
        /// it if need be.
        /// </summary>
        public override void TestCollisions(Amoeba player)
        {
            bool touched = false;
            Vector3[] vertexPositions = player.VertexPositions;
            for (int i = 0; i < vertexPositions.Length; i++)
                // Process collisions with groundplane
                if (vertexPositions[i].Y < 0)
                {
                    vertexPositions[i].Y = 0;
                    touched = true;
                }
            Game.Amoeba.TouchingObject = Game.Amoeba.TouchingObject | touched;
            TouchingAmoeba = touched;
        }

        /// <summary>
        /// This has almost nothing to do.
        /// It just updates the Position property so that its
        /// "center" is always immediately below the player
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            Vector3 p = Game.Amoeba.Position;
            p.Y = 0;
            Position = p;

            base.Update(gameTime);
        }

        /// <summary>
        /// Sets of the effect and mesh so that we're ready to draw the ground plane.
        /// </summary>
        public override void Initialize()
        {
            groundPlaneEffect = new BasicEffect(Game.GraphicsDevice);
            groundPlaneEffect.DiffuseColor = new Vector3(0.2f, 0.2f, 0.2f);
            //groundPlaneEffect.AmbientLightColor = Color.Green.ToVector3() * 0.3f;
            groundPlaneEffect.SpecularColor = Vector3.Zero;

            groundPlaneEffect.LightingEnabled = true;
            groundPlaneEffect.EnableDefaultLighting();

            groundPlaneEffect.View = Game.ViewMatrix;
            groundPlaneEffect.Projection = Game.ProjectionMatrix;
            groundPlaneEffect.World = Matrix.Identity;

            groundPlane = SimpleMesh.TriangleList(Game.GraphicsDevice,
                                                    new VertexPositionNormalTexture(new Vector3(9999, 0, -9999), Vector3.Up, new Vector2(1, 0)),
                                                    new VertexPositionNormalTexture(new Vector3(-9999, 0, 9999), Vector3.Up, new Vector2(0, 1)),
                                                    new VertexPositionNormalTexture(new Vector3(-9999, 0, -9999), Vector3.Up, new Vector2(0, 0)),
                                                    new VertexPositionNormalTexture(new Vector3(9999, 0, 9999), Vector3.Up, new Vector2(1, 1)),
                                                    new VertexPositionNormalTexture(new Vector3(-9999, 0, 9999), Vector3.Up, new Vector2(0, 1)),
                                                    new VertexPositionNormalTexture(new Vector3(9999, 0, -9999), Vector3.Up, new Vector2(1, 0))
                                                    );


            base.Initialize();
        }

        /// <summary>
        /// Draaw the plane in the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            groundPlaneEffect.View = Game.ViewMatrix;
            groundPlaneEffect.Projection = Game.ProjectionMatrix;
            foreach (EffectPass pass in groundPlaneEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                groundPlane.Draw();
            }
        }
    }
}
