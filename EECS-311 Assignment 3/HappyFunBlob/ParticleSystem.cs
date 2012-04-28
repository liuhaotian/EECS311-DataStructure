using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace HappyFunBlob
{
    /// <summary>
    /// Creates a cloud of particles that swirl around the amoeba.
    /// </summary>
    public class ParticleSystem : DrawableGameComponent
    {
        #region Tuning parameters
        /// <summary>
        /// Rate at which the velocity of the amoeba is transfered
        /// to the particles nearby
        /// </summary>
        const float momentumTransferGain = 100f;
        /// <summary>
        /// Strength of attraction to the amoeba
        /// </summary>
        const float amoebaAttractionGain = 2f;
        /// <summary>
        /// Rate at which particles slow down
        /// </summary>
        const float damping = 0.05f;
        /// <summary>
        /// How long a given particle lasts before being killed and respawned elsewhere
        /// </summary>
        int lifeSpan;
        #endregion

        #region Particle state
        /// <summary>
        /// States of the different particles on screen
        /// </summary>
        Particle[] particles;
        /// <summary>
        /// sortPositions[i] is the index within particles of the i'th farthest particle
        /// </summary>
        //int[] sortPositions;
        #endregion

        #region Graphics card interface
        /// <summary>
        /// The shader program that runs in the graphics card to draw the particles
        /// </summary>
        Effect effect;
        /// <summary>
        /// The bitmap image of the particle
        /// </summary>
        static Texture2D sprite;
        /// <summary>
        /// The triangles to be drawn on screen
        /// </summary>
        VertexPositionColorTexture[] vertexBuffer;
        #endregion

        #region Other fields
        /// <summary>
        /// The game object.  Used to access the GraphicsDevice, ViewMatrix, etc.
        /// </summary>
        HappyFunBlobGame game;
        /// <summary>
        /// Random number generator.
        /// </summary>
        Random rand;
        #endregion

        /// <summary>
        /// Creates a swarm of particles tha swirl around the amoeba
        /// </summary>
        /// <param name="g">Game the ParticleSystem is a part of</param>
        /// <param name="count">Number of particles</param>
        /// <param name="lifeSpan">Number ticks each particle survives for before being destroyed and respawned</param>
        public ParticleSystem(HappyFunBlobGame g, int count, int lifeSpan)
            : base(g)
        {
            game = g;
            DrawOrder = 9000;
            rand = new Random((int)DateTime.Now.Ticks);
            this.lifeSpan = lifeSpan;
            particles = new Particle[count];
            //sortPositions = new int[count];
            for (int i = 0; i < count; i++)
            {

                particles[i] = new Particle(new Vector3(rand.FloatInRange(-1, 1),
                                                        rand.FloatInRange(-1, 1),
                                                        rand.FloatInRange(-1, 1)),
                                            new Vector3(rand.FloatInRange(-1, 1),
                                                        rand.FloatInRange(-1, 1),
                                                        rand.FloatInRange(-0, 1)),
                                            i % lifeSpan);
                //sortPositions[i] = i;
            }

            vertexBuffer = new VertexPositionColorTexture[count * 6];
        }

        /// <summary>
        /// Loads particle texture and shader
        /// </summary>
        protected override void LoadContent()
        {
            sprite = game.Content.Load<Texture2D>("gaus2d");
            effect = game.Content.Load<Effect>("RawTexture");
            effect.CurrentTechnique = effect.Techniques["Simplest"];
            effect.Parameters["xTexture"].SetValue(sprite);

            //vertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColorTexture.VertexElements);
            base.LoadContent();
        }

        /// <summary>
        /// Updates positions and velocities of particles
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            Profiler.Enter("ParticleSystem.Update");
            Amoeba amoeba = game.Amoeba;
            Vector3 ameobaVelocity = amoeba.Position - amoeba.PreviousPosition;
            BoundingBox targetBox = new BoundingBox(game.Amoeba.Position - new Vector3(100, 100, 100), game.Amoeba.Position + new Vector3(150, 150, 150));
            BoundingBox unitBox = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            Vector3 source = (amoeba.GrabTarget == null) ? Vector3.Zero : amoeba.GrabTarget.Position;

            // Update all particles
            for (int i = 0; i < particles.Length; i++)
            {
                // Compute spacing between particle and amoeba
                Vector3 delta = amoeba.Position-particles[i].position;
                float dSquared = delta.LengthSquared();
                float dWeight = 1.0f/Math.Max(100, dSquared);
                // Apply forces
                particles[i].velocity *= (1 - damping);
                particles[i].velocity += (amoebaAttractionGain * dWeight) * delta + (momentumTransferGain * dWeight) * ameobaVelocity;
                // Update position
                particles[i].position += particles[i].velocity;
                particles[i].position.Y = Math.Max(particles[i].position.Y, 0.5f);
                // Update age and respawn if necessary
                particles[i].age++;
                if (particles[i].age > lifeSpan)
                {
                    particles[i].age = 0;
                    //reset position
                    particles[i].position = RandomPoint(targetBox);
                    particles[i].velocity = RandomPoint(unitBox);
                }
            }
            base.Update(gameTime);
            Profiler.Exit("ParticleSystem.Update");
        }

        /// <summary>
        /// Depth-sorts particles, and draws all particles in front of the projection plane.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Profiler.Enter("ParticleSystem.Draw");
            // Configure graphics adaptor for alpha blending (transparency)
            game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            //RenderState.AlphaBlendEnable = true;
            //game.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
            //game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            //game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            //game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            //game.GraphicsDevice.RenderState.DepthBufferEnable = true;
            // Update view matrix
            effect.Parameters["xViewProjection"].SetValue(game.ViewMatrix * game.ProjectionMatrix);

            // Depth sort particles
            var invMat = Matrix.Invert(game.ViewMatrix);
            Vector3 cameraPosition = Vector3.Transform(Vector3.Zero, invMat);
            DepthSort(cameraPosition, invMat.Forward);

            // Fill vertex buffer
            var lowerLeft = new Vector2(0f, 0f);
            var upperLeft = new Vector2(0f, 1);
            var lowerRight = new Vector2(1, 0f);
            var upperRight = new Vector2(1, 1);
            var upVector = 3 * invMat.Up;
            var rightVector = 3 * invMat.Right;
            int writePointer = 0;         // Where to write the next vertex in the vertexBuffer
            var c = Color.Silver; //Color.Chocolate;      // Color (including alpha) to draw the particle in
            int count = 0;                // Number of particles to be drawn

            // Walk through the depth-sorted particles back-to-front, copying billboarded
            // quads to the vertexBuffer.  Stop when we get to a negative depth.
            for (int i = particles.Length - 1; i >= 0 && particles[i].depth > 0; i--)
            {
                var p = particles[i].position;
                // Compute its alpha (opacity) value
                var ttl = ((float)lifeSpan - particles[i].age) / (float)lifeSpan;
                float alpha = (ttl < 0.5) ? (510 * ttl) : (510 * (1 - ttl));
                // Fade particles near the image plane in/out, so we don't get popping.
                if (particles[i].depth < 30)
                    alpha *= particles[i].depth / 30.0f;
                c.A = (byte)(alpha);
                // First triangle
                vertexBuffer[writePointer++] = new VertexPositionColorTexture(p, c, lowerLeft);
                vertexBuffer[writePointer++] = new VertexPositionColorTexture(p + upVector, c, upperLeft);
                vertexBuffer[writePointer++] = new VertexPositionColorTexture(p + rightVector, c, lowerRight);
                // Second triangle
                vertexBuffer[writePointer++] = new VertexPositionColorTexture(p + rightVector, c, lowerRight);
                vertexBuffer[writePointer++] = new VertexPositionColorTexture(p + upVector, c, upperLeft);
                vertexBuffer[writePointer++] = new VertexPositionColorTexture(p + upVector + rightVector, c, upperRight);
                // We have one more particle
                count++;
            }

            if (count > 0)
            {
                // Render the quads
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    game.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vertexBuffer, 0, 2 * count);
                }
            }
            // Reset the render states that won't be reset automatically
            //game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Profiler.Exit("ParticleSystem.Draw");
        }

        /// <summary>
        /// Recomputes depths of particles and 
        /// </summary>
        /// <param name="forwardVector"></param>
        void DepthSort(Vector3 cameraPosition, Vector3 forwardVector)
        {
            UpdateDepths(cameraPosition, forwardVector);
            Profiler.Enter("DepthSort");
#if CUSTOMSORT
            InsertionDepthSort();
            //QuicksortDepthSort();
#else
            Array.Sort<Particle>(particles, (a, b) => Comparer.Default.Compare(a.depth, b.depth));
#endif
            Profiler.Exit("DepthSort");
        }

        public void UpdateDepths(Vector3 cameraPosition, Vector3 forwardVector)
        {
            float cameraDepth = Vector3.Dot(forwardVector, cameraPosition);
            // Update depths
            for (int i = 0; i < particles.Length; i++)
                particles[i].depth = Vector3.Dot(particles[i].position, forwardVector) - cameraDepth;
        }

#if CUSTOMSORT
        public void InsertionDepthSort()
        {
            throw new NotImplementedException();
        }

        public void QuicksortDepthSort()
        {
            throw new NotImplementedException();
        }
#endif

        public bool IsDepthSorted()
        {
            float depth = particles[0].depth;
            for (int i = 1; i < particles.Length; i++)
            {
                float newDepth = particles[i].depth;
                if (newDepth < depth)
                    return false;
                else
                    depth = newDepth;
            }
            return true;
        }

        /// <summary>
        /// Picks a random point within a rectangular volume
        /// </summary>
        public Vector3 RandomPoint(BoundingBox b)
        {
            return new Vector3(rand.FloatInRange(b.Min.X, b.Max.X), rand.FloatInRange(b.Min.Y, b.Max.Y), rand.FloatInRange(b.Min.Z, b.Max.Z));
        }
    }

    /// <summary>
    /// Represents the state of a single particle
    /// </summary>
    public class Particle
    {
        /// <summary>
        /// Current position of the particle's center 
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// Particle's speed
        /// </summary>
        public Vector3 velocity;
        /// <summary>
        /// Distance from camera
        /// </summary>
        public float depth;
        /// <summary>
        /// Number of ticks since the particle was spawned
        /// </summary>
        public int age;

        /// <summary>
        /// Creates a particle with the specified position, speed, and age.
        /// </summary>
        public Particle(Vector3 pos, Vector3 vel, int age)
        {
            position = pos;
            velocity = vel;
            depth = 0;
            this.age = age;
        }
    }

    /// <summary>
    /// Adds FloatInRange method to the Random class.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns a float value in the specified range.
        /// </summary>
        public static float FloatInRange(this Random r, float min, float max)
        {
            return (float)(r.NextDouble() * (max - min) + min);
        }
    }
}
