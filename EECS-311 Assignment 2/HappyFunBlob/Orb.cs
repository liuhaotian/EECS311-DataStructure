using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HappyFunBlob
{
    /// <summary>
    /// A spherical obstacle.  Can be grabbed by player.
    /// </summary>
    public class Orb : Obstacle, IDisposable
    {
        #region Instance variables
        SimpleMesh sphereMesh;
        public float Radius { get; private set; }
        public float RadiusSquared { get; private set; }
        float radiusPlusOneSquared;
        Vector3 rotationAxis = Vector3.Forward;
        public float SpinRate { get; private set; }

        static BasicEffect effect;
        #endregion

        public Orb(HappyFunBlobGame game, Vector3 position, Initializer initialize)
            : base(game, position)
        {
            //Profiler.Enter("Orb constructor");
            Color = Microsoft.Xna.Framework.Color.Gold.ToVector3();
            Radius = 6;
            SpinRate = 0.04f;
            if (initialize != null)
            {
                //Profiler.Enter("Run initializers");
                initialize(this);
                //Profiler.Exit("Run initializers");
            }
            RadiusSquared = Radius * Radius;
            radiusPlusOneSquared = (Radius + 1) * (Radius + 1);

            if (game != null)
            {
                //Profiler.Enter("Clone effect");
                if (effect == null)
                    effect = (BasicEffect)game.BasicEffect.Clone();
                //Profiler.Exit("Clone effect");
                //Profiler.Enter("make Sphere");
                sphereMesh = SimpleMesh.Sphere(game.GraphicsDevice, Radius - 0.5f, 3, Vector3.Zero);
                //Profiler.Exit("make Sphere");
            }
            //Profiler.Exit("Orb constructor");
        }

        void IDisposable.Dispose()
        {
            sphereMesh.Dispose();
            //effect.Dispose();
            base.Dispose();
        }

        public override void TestCollisions(Amoeba player)
        {
            bool touched = false;

            Vector3[] points = player.VertexPositions;
            bool pushoff = (player.Grabbing && (player.GrabTarget != this));
            float pushRadius = pushoff ? (Radius+1) : Radius;
            Vector3 windAxis = pushoff ? (0 * rotationAxis) : rotationAxis; // pushoff ? (-5*rotationAxis) : rotationAxis;
            BoundingSphere bs = new BoundingSphere(Position, Radius);

            // This is sort of a mess: an accretion of features and special
            // cases that should get rationalized at some point.
            for (int i = 0; i < points.Length; i++)
            {
                // First detect and fix the case where a vertex tunnels through
                // the sphere.
                Vector3 p = points[i];
                Vector3 pp = player.VertexPreviousPositions[i];
                Ray trajectory = new Ray(pp, p - pp);
                float? intersectTime;
                bs.Intersects(ref trajectory, out intersectTime);
                if (!pushoff && intersectTime != null && intersectTime <= 1)
                {
                    points[i] = trajectory.Position + intersectTime.Value * trajectory.Direction;
                }

                // Now check if it's close enough to acquire spin from the orb
                Vector3 boffset = points[i] - Position;
                float bDistSquared = boffset.LengthSquared();
                if (!pushoff && bDistSquared < radiusPlusOneSquared)
                {
                    // It is - add spin
                    points[i] += SpinRate * Vector3.Cross(boffset, windAxis);
                    boffset = points[i] - Position;
                    bDistSquared = boffset.LengthSquared();
                }

                // Now if it's reasonably close, move it to the surface
                if (bDistSquared < RadiusSquared * 1.1f)
                {
                    float dist = (float)Math.Sqrt(bDistSquared);

                    if (!pushoff)
                        Game.Amoeba.VertexPreviousPositions[i] = points[i] += boffset * ((pushRadius - dist) / dist);
                    if (player.GripSurface)
                        player.VertexFrozen[i] = true;
                    touched = true;
                }
            }
            player.TouchingObject = player.TouchingObject | touched;
            TouchingAmoeba = touched;
        }

        bool IsGrabTarget
        {
            get
            {
                return Game.Amoeba.GrabTarget == this;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            //Game.GraphicsDevice.VertexDeclaration = vertexDeclaration;
            effect.World = Matrix.Multiply(Matrix.CreateRotationZ(2f*(float)gameTime.TotalGameTime.TotalSeconds), Matrix.CreateTranslation(Position));
            effect.View = Game.ViewMatrix;
            effect.DiffuseColor = IsGrabTarget ? (0.5f * (float)Math.Sin(Environment.TickCount / 100) + 0.5f) * Color : Color;
            effect.SpecularColor = Color;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                sphereMesh.Draw();
            }

            base.Draw(gameTime);
        }

        public override BoundingBox BoundingBox
        {
            get {
                Vector3 offset = new Vector3(Radius, Radius, Radius);
                return new BoundingBox(Position - offset, Position + offset);
            }
        }

        public override bool InView
        {
            get
            {
                return Game.ViewFrustum.Contains(new BoundingSphere(Position, Radius))!= ContainmentType.Disjoint;
            }
        }

    }
}
