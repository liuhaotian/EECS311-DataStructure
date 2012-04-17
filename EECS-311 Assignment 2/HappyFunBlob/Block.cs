using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace HappyFunBlob
{
    /// <summary>
    /// A rectangular obstacle.  Can be pushed off of by the player, but isn't a target for grabbing
    /// </summary>
    class Block : Obstacle, IDisposable
    {
        /// <summary>
        /// The mesh to draw for this object.
        /// </summary>
        SimpleMesh mesh;

        /// <summary>
        /// The "effect" (read: shading program) to use for drawing this object
        /// </summary>
        static BasicEffect effect;

        /// <summary>
        /// Size of the object
        /// </summary>
        float halfWidth = 5, halfHeight = 5, halfDepth = 5;

        /// <summary>
        /// The collision handling pushes the blob a little farther away from the
        /// block to help keep it from getting pulled through corners.
        /// </summary>
        float standoffDistance = 1f;
        

        /// <summary>
        /// Makes a block
        /// </summary>
        public Block(HappyFunBlobGame g, Vector3 position, Initializer i)
            : base(g, position)
        {
            //Profiler.Enter("Block constructor");
            i(this);
            if (effect == null)
            {
                effect = (BasicEffect)g.BasicEffect.Clone();
                effect.DiffuseColor = new Vector3(184 / 256f, 115 / 256f, 51 / 256f);
                effect.SpecularColor = new Vector3(184 / 256f, 115 / 256f, 51 / 256f); // Color.White.ToVector3();
                effect.SpecularPower = 100;
            }
            BuildBox();
            //Profiler.Exit("Block constructor");
        }

                /// <summary>
        /// Called to deallocate the graphics card memory allocated to the block.
        /// </summary>
        void IDisposable.Dispose()
        {
            mesh.Dispose();
            //effect.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Smallest axis-aligned box that accomodates this object.
        /// Since it's an axis-aligned box to begin with, this is the same dimensions
        /// as the block itself.
        /// </summary>
        public override BoundingBox BoundingBox
        {
            get
            {
                if (OscillationAmplitude == 0)
                {
                    Vector3 offset = new Vector3(halfWidth, halfHeight, halfDepth);
                    return new BoundingBox(Position - offset, Position + offset);
                }
                else
                {
                    Vector3 offset = new Vector3(halfWidth, halfHeight, halfDepth);
                    Vector3 oscdir = new Vector3(Math.Abs(OscillationDirection.X), Math.Abs(OscillationDirection.Y), Math.Abs(OscillationDirection.Z));
                    offset += oscdir * OscillationAmplitude;
                    return new BoundingBox(Position - offset, Position + offset);
                }
            }
        }

        /// <summary>
        /// Draws the block on the screen
        /// </summary>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            effect.View = Game.ViewMatrix;
            effect.World = Matrix.CreateTranslation(Position);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                mesh.Draw();
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// Tests the vertices of the blob for interpenetration with the block
        /// This code is optimized for axis-aligned boxes moving along an axis.
        /// </summary>
        /// <param name="a"></param>
        public override void TestCollisions(Amoeba a)
        {
            bool touching = false;

            Vector3[] points = a.VertexPositions;
            Vector3[] pointsPrevious = a.VertexPreviousPositions;
            Vector3 myVelocity = Position - PreviousPosition;
            Vector3 offset = new Vector3(halfWidth, halfHeight, halfDepth);
            BoundingBox endingVolume = new BoundingBox(Position - offset, Position + offset);

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 p = points[i];
                Vector3 initialPosition = pointsPrevious[i];

                if (endingVolume.Contains(initialPosition) != ContainmentType.Disjoint)
                {
                    // The box ends up on top of the vertex's initial position
                    // So assume that the collision is with the leading face of the box
                    points[i] = CollideWithLeadingFace(p, myVelocity, endingVolume);
                    touching = true;
                }
                else
                {

                    // Check to see if the ending position of the box intersects the line
                    // Segment connecting the initial and final positions of the vertex
                    float? intersectionTime;
                    Ray vertexMotionRay = new Ray(initialPosition, p - initialPosition);
                    endingVolume.Intersects(ref vertexMotionRay, out intersectionTime);
                    if (intersectionTime != null && intersectionTime <= 1)
                    {
                        // It did.  We need to adjust p so it doesn't pass through the intersected face.
                        // First, we figure out where the collision occurred.
                        Vector3 intersectionPoint = vertexMotionRay.Position + intersectionTime.Value * vertexMotionRay.Direction;
                        points[i] = CollideWithIntersectedFace(p, intersectionPoint, endingVolume);
                        touching = true;
                    }
                    // Otherwise this vertex missed the box entirely
                }
            }
            Game.Amoeba.TouchingObject = Game.Amoeba.TouchingObject | touching;
            TouchingAmoeba = touching;
        }

        /// <summary>
        /// Tests equality to within a small tolerance factor
        /// </summary>
        static bool CloseTo(float a, float b)
        {
            return Math.Abs(a - b) < 0.0001f;
        }

        /// <summary>
        /// Adjust position of point based on the assumption the point started outside the box and intersected at the specified point.
        /// </summary>
        Vector3 CollideWithIntersectedFace(Vector3 pointToAdjust, Vector3 intersectionPoint, BoundingBox bbox)
        {
            if (CloseTo(intersectionPoint.X, bbox.Min.X))
                pointToAdjust.X = bbox.Min.X - standoffDistance;
            else if (CloseTo(intersectionPoint.X, bbox.Max.X))
                pointToAdjust.X = bbox.Max.X + standoffDistance;
            else if (CloseTo(intersectionPoint.Y, bbox.Min.Y))
                pointToAdjust.Y = bbox.Min.Y - standoffDistance;
            else if (CloseTo(intersectionPoint.Y, bbox.Max.Y))
                pointToAdjust.Y = bbox.Max.Y + standoffDistance;
            else if (CloseTo(intersectionPoint.Z, bbox.Min.Z))
                pointToAdjust.Z = bbox.Min.Z - standoffDistance;
            else if (CloseTo(intersectionPoint.Z, bbox.Max.Z))
                pointToAdjust.Z = bbox.Max.Z + standoffDistance;
            else
                Debug.Assert(false, "Collision location doesn't lie on bounding box");

            return pointToAdjust;
        }

        /// <summary>
        /// Adjust position of face based on the assumption the block moved on top of the point.
        /// </summary>
        Vector3 CollideWithLeadingFace(Vector3 pointToAdjust, Vector3 blockVelocity, BoundingBox bbox)
        {
            if (blockVelocity.X != 0)
            {
                if (Game.Amoeba.Position.X >= PreviousPosition.X)
                    // Collide with right face
                    pointToAdjust.X = bbox.Max.X + standoffDistance;
                else
                    // Collide with left face
                    pointToAdjust.X = bbox.Min.X - standoffDistance;
            }
            else if (blockVelocity.Y != 0)
            {
                if (Game.Amoeba.Position.Y >= PreviousPosition.Y)
                    // Collide with top face
                    pointToAdjust.Y = bbox.Max.Y + standoffDistance;
                else
                    // Collide with bottom face
                    pointToAdjust.Y = bbox.Min.Y - standoffDistance;
            }
            else if (blockVelocity.Z != 0)
            {
                if (Game.Amoeba.Position.Z >= PreviousPosition.Z)
                    // Collide with front face
                    pointToAdjust.Z = bbox.Max.Z + standoffDistance;
                else
                    // Collide with back face
                    pointToAdjust.Z = bbox.Min.Z - standoffDistance;
            }
            else
                // This means the vertex started inside the box.
                Debug.Assert(false, "CollideWithLeadingEdge called when block has zero velocity.");

            return pointToAdjust;
        }

#if OldBlockCollision
        public override void TestCollisions(Amoeba a)
        {
            float pushAway = 1f;
            Vector3[] points = a.vertexPositions;
            bool touching = false;
            Vector3 myVelocity = Position - PreviousPosition;
            Vector3 offset = new Vector3(halfWidth, halfHeight, halfDepth);
            BoundingBox endingVolume = new BoundingBox(PreviousPosition - offset, PreviousPosition + offset);
            BoundingBox volumeSwept = BoundingBox.CreateMerged(endingVolume,
                                                                new BoundingBox(Position - offset, Position + offset));

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 p = points[i];
                Vector3 initialPosition = a.vertexPreviousPositions[i];

                // First, check if the vertex ever intersects the volume swept by the box
                float? intersectionTime;
                Ray vertexMotionRay = new Ray(initialPosition, p - initialPosition);
                volumeSwept.Intersects(ref vertexMotionRay, out intersectionTime);
                if (intersectionTime != null && intersectionTime <= 1)
                {
                    // It did.  Now find which face of the box it hits first
                    // and force the ending position to be just out of contact with
                    // that face
                    
                    //  First check if the box actually moved on top of the initial position.
                    if (endingVolume.Contains(initialPosition) != ContainmentType.Disjoint)
                    {
                        // It did.  Assume the leading plane of the box is the site of collision.
                        //if (myVelocity.X < 0)
                        //    initialPosition.X = p.X = Position.X - halfWidth - pushAway;
                        //else if (myVelocity.X > 0)
                        //    initialPosition.X = p.X = Position.X + halfWidth + pushAway;
                        //else if (myVelocity.Y < 0)
                            initialPosition.Y = p.Y = Position.Y - halfHeight - pushAway;
                        //else if (myVelocity.Y > 0)
                        //    initialPosition.Y = p.Y = Position.Y + halfHeight + pushAway;
                        //else if (myVelocity.Z < 0)
                        //    initialPosition.Z = p.Z = Position.Z - halfDepth - pushAway;
                        //else if (myVelocity.Z > 0)
                        //    initialPosition.Z = p.Z = Position.Z + halfDepth + pushAway;
                        //else
                        //    Debug.Assert(false);
                        touching = true;
                        points[i] = p;
                        a.vertexPreviousPositions[i] = initialPosition;
                    }
                    else
                    {
                        // It didn't.  Test each face manually.
                        float contactTime = 2;
                        Vector3 contactPoint = p;
                        Vector3 vertexVelocity = points[i] - initialPosition;
                        Vector3 relativeVelocity = vertexVelocity - myVelocity;

                        float innerPlane;
                        float outerPlane;

                        // Check X axis - the +X and -X faces
                        if (initialPosition.X < PreviousPosition.X)
                        {
                            // It started on the left side, so has to stay on that side
                            innerPlane = Position.X - halfWidth;
                            outerPlane = innerPlane - pushAway;
                        }
                        else
                        {
                            // It started on the right side
                            innerPlane = Position.X + halfWidth;
                            outerPlane = innerPlane + pushAway;
                        }

                        float cTimeX = (innerPlane - initialPosition.X) / relativeVelocity.X;
                        //AssertProper(cTimeX);
                        //Debug.Assert(cTimeX <= 1f);

                        if (cTimeX >= 0)
                        {
                            contactTime = cTimeX;
                            //contactPoint = initialPosition + cTimeX * vertexVelocity;
                            contactPoint = p;
                            contactPoint.X = outerPlane;
                        }

                        // Now check the +Y and -Y faces
                        if (initialPosition.Y < PreviousPosition.Y)
                        {
                            // It started on the bottom side
                            innerPlane = Position.Y - halfHeight;
                            outerPlane = innerPlane - pushAway;
                        }
                        else
                        {
                            // It started on the top side
                            innerPlane = Position.Y + halfHeight;
                            outerPlane = innerPlane + pushAway;
                        }

                        float cTimeY = (innerPlane - initialPosition.Y) / relativeVelocity.Y;
                        //AssertProper(cTimeY);
                        //Debug.Assert(cTimeY <= 1f);

                        if (cTimeY >= 0 && cTimeY < contactTime)
                        {
                            contactTime = cTimeY;
                            //contactPoint = initialPosition + cTimeY * vertexVelocity;
                            contactPoint = p;
                            contactPoint.Y = outerPlane;
                        }

                        // And the +Z and -Z faces
                        if (initialPosition.Z < PreviousPosition.Z)
                        {
                            // It started on the back side
                            innerPlane = Position.Z - halfDepth;
                            outerPlane = innerPlane - pushAway;
                        }
                        else
                        {
                            // It started on the front side
                            innerPlane = Position.Z + halfDepth;
                            outerPlane = innerPlane + pushAway;
                        }

                        float cTimeZ = (innerPlane - initialPosition.Z) / relativeVelocity.Z;
                        //AssertProper(cTimeZ);
                        //Debug.Assert(cTimeZ <= 1f);


                        if (cTimeZ >= 0 && cTimeZ < contactTime)
                        {
                            contactTime = cTimeZ;
                            //contactPoint = initialPosition + cTimeZ * vertexVelocity;
                            contactPoint = p;
                            contactPoint.Z = outerPlane;
                        }

                        if (contactTime < 1)
                        {
                            points[i] = contactPoint;
                            touching = true;
                        }
                    }
                }
            }
            game.Amoeba.touchingObject |= touching;
            TouchingAmoeba = touching;
        }
#endif

        [Conditional("DEBUG")]
        static void AssertProper(float x)
        {
            Debug.Assert(!float.IsInfinity(x) && !float.IsNaN(x));
        }

        /// <summary>
        /// Build the mesh to be drawn by the graphics card.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void BuildBox()
        {
            Vector3 shapePosition = Vector3.Zero;
            Vector3 shapeSize = new Vector3(halfWidth, halfHeight, halfDepth);

            // Cribbed from http://www.switchonthecode.com/tutorials/creating-a-textured-box-in-xna
            int shapeTriangles = 12;

            var shapeVertices = new VertexPositionNormalTexture[36];

            Vector3 topLeftFront = shapePosition +
                new Vector3(-1.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomLeftFront = shapePosition +
                new Vector3(-1.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topRightFront = shapePosition +
                new Vector3(1.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomRightFront = shapePosition +
                new Vector3(1.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topLeftBack = shapePosition +
                new Vector3(-1.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 topRightBack = shapePosition +
                new Vector3(1.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 bottomLeftBack = shapePosition +
                new Vector3(-1.0f, -1.0f, 1.0f) * shapeSize;
            Vector3 bottomRightBack = shapePosition +
                new Vector3(1.0f, -1.0f, 1.0f) * shapeSize;

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f) * shapeSize;
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f) * shapeSize;
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f) * shapeSize;
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f) * shapeSize;
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f) * shapeSize;
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f) * shapeSize;

            Vector2 textureTopLeft = new Vector2(0.5f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureTopRight = new Vector2(0.0f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureBottomLeft = new Vector2(0.5f * shapeSize.X, 0.5f * shapeSize.Y);
            Vector2 textureBottomRight = new Vector2(0.0f * shapeSize.X, 0.5f * shapeSize.Y);

            // Front face.
            shapeVertices[0] = new VertexPositionNormalTexture(
                topLeftFront, frontNormal, textureTopLeft);
            shapeVertices[1] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[2] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);
            shapeVertices[3] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[4] = new VertexPositionNormalTexture(
                bottomRightFront, frontNormal, textureBottomRight);
            shapeVertices[5] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);

            // Back face.
            shapeVertices[6] = new VertexPositionNormalTexture(
                topLeftBack, backNormal, textureTopRight);
            shapeVertices[7] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[8] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[9] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[10] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[11] = new VertexPositionNormalTexture(
                bottomRightBack, backNormal, textureBottomLeft);

            // Top face.
            shapeVertices[12] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[13] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);
            shapeVertices[14] = new VertexPositionNormalTexture(
                topLeftBack, topNormal, textureTopLeft);
            shapeVertices[15] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[16] = new VertexPositionNormalTexture(
                topRightFront, topNormal, textureBottomRight);
            shapeVertices[17] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);

            // Bottom face. 
            shapeVertices[18] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[19] = new VertexPositionNormalTexture(
                bottomLeftBack, bottomNormal, textureBottomLeft);
            shapeVertices[20] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[21] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[22] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[23] = new VertexPositionNormalTexture(
                bottomRightFront, bottomNormal, textureTopRight);

            // Left face.
            shapeVertices[24] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);
            shapeVertices[25] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[26] = new VertexPositionNormalTexture(
                bottomLeftFront, leftNormal, textureBottomRight);
            shapeVertices[27] = new VertexPositionNormalTexture(
                topLeftBack, leftNormal, textureTopLeft);
            shapeVertices[28] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[29] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);

            // Right face. 
            shapeVertices[30] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[31] = new VertexPositionNormalTexture(
                bottomRightFront, rightNormal, textureBottomLeft);
            shapeVertices[32] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);
            shapeVertices[33] = new VertexPositionNormalTexture(
                topRightBack, rightNormal, textureTopRight);
            shapeVertices[34] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[35] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);

            mesh = new SimpleMesh(Game.GraphicsDevice, PrimitiveType.TriangleList, shapeVertices, shapeTriangles);
        }

        /// <summary>
        /// X-axis width of the block
        /// </summary>
        public float Width
        {
            get
            {
                return halfWidth * 2;
            }
            set
            {
                halfWidth = value / 2;
            }
        }

        /// <summary>
        /// Y-axis height of the block
        /// </summary>
        public float Height
        {
            get
            {
                return halfHeight * 2;
            }
            set
            {
                halfHeight = value / 2;
            }
        }

        /// <summary>
        /// Z-axis depth of the block 
        /// </summary>
        public float Depth
        {
            get
            {
                return halfDepth * 2;
            }
            set
            {
                halfDepth = value / 2;
            }
        }
    }
}
