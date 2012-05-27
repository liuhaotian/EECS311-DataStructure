using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HappyFunBlob
{
    /// Here's the basic idea of how the code works:
    /// We represent the skin of the creature as a polygone mesh.  It's just
    /// a bunch of triangles that share corners and edges.  However, from now on, 
    /// we're going to say "vertex" (plural: vertices) instead of "corner".
    ///
    /// To animate the creature, we need only update the positions of the vertices
    /// from frame to frame, based on the underlying physics.  There are three physics
    /// issues that matter here: the elasticity of the skin, the pressure difference
    /// between the inside and outside of the skin, and any collisions with objects
    /// outside the creature.
    ///
    /// The elasticity of the skin is modeled by treating every edge of every triangle
    /// as a spring.  The spring knows what length it wants to be and exerts forces on
    /// the vertices it connects to move them toward that ideal length.
    ///
    /// Pressure is computed by computing the volume of the mesh (this is easy through
    /// the magic of Stokes' theorem, which you may or may not remember), and taking
    /// the ratio of the current volume and the ideal volume.
    ///
    /// Collisions are handled by comparing each vertex with the position of a
    /// candidate obstacle and moving the vertex if it interpenetrates the obstacle.
    /// The current version of the system doesn't allow the mesh to exert forces on
    /// the obstacle, although that would be simple to do.
    ///
    /// The main problem with the code is that it performs collision checks between
    /// the mesh and other objects, but not between different parts of the mesh (too
    /// expensive).  So that means parts of the character's skin can pass through
    /// one another, resulting in parts of the mesh being inside-out.  If too much
    /// of the mesh is inside out, that will result in the system thinking the
    /// character has negative volume. That leads to the pressure forces trying to
    /// push the mesh in a way that stretches it more inside-out, and the final result
    /// is an enormous inside-out sphere.  There are a lot of hacks in the code to
    /// prevent this from happening.  The main ones come into effect which the creature
    /// is compressed (low volume) more than 5% or 10%.  In these cases, grabbing is
    /// disabled, and damping is increased dramatically, to absorb any energy that
    /// might invert the mesh.  If the system really gets desperate, it just resets
    /// the mesh back to the initial sphere.

    /// <summary>
    /// The "player character" of the game, so to speak.
    /// Implements a gas-filled-balloon soft-body physics model
    /// </summary>
    public class Amoeba : GameObject
    {
        #region Dynamics controls
        /// These are all the fields that control the dynamics of the character
        /// The actual motion of the character is give by the fields in the
        /// dynamic state section, following this section.

        /// <summary>
        /// Spring constant for the mesh springs.  Higher values means stronger springs.
        /// </summary>
        float springK = 0.05f;

        /// <summary>
        /// Strength of gravity.
        /// </summary>
        const float gravity = -0.01f;

        /// <summary>
        /// Acceleration vector for gravity.
        /// </summary>
        Vector3 gVector;

        /// <summary>
        /// Damping factor for vertex motions.  Large values simulate motion is a more
        /// viscous fluid.
        /// </summary>
        const float damping = 0.005f;
        
        /// <summary>
        /// Constant for computing pressure from relative volume.
        /// Think of it as the temperature of the gas - bigger values
        /// mean it wants to expand more.
        /// </summary>
        const float pressureForce = 20000f;

        /// <summary>
        /// Limit of stretch of the springs.  Bigger value means more stretch allowed.
        /// </summary>
        const float maxStretchFactor = 3;

        /// <summary>
        /// Whether the character is actively trying to grip surface.  Not used in
        /// current game.
        /// </summary>
        public bool GripSurface { get; private set; }

        /// <summary>
        /// What orb the character is reaching for.
        /// </summary>
        public Orb GrabTarget { get; private set; }

        /// <summary>
        /// Whether the character is reaching for a target.
        /// </summary>
        public bool Grabbing { get; private set; }

        /// <summary>
        /// Whether the character is currently touching something
        /// (and so has something to push/pull against)
        /// </summary>
        public bool TouchingObject { get; set; }
        #endregion

        #region Dynamic state
        /// All the fields representing the current position and motion
        /// of the character.  The motion is controlled by the fields
        /// the the dynamics controlls section, above.

        /// <summary>
        /// Position of the centroid of the object during the previous frame
        /// </summary>
        public Vector3 PreviousPosition { get; private set; }

        /// <summary>
        /// Current position of each vertex
        /// </summary>
        public Vector3[] VertexPositions { get; private set; }
        /// <summary>
        /// Position of each vertex from the previous frame
        /// </summary>
        public Vector3[] VertexPreviousPositions { get; private set; }
        Vector3[] originalVertexPositions;

        /// <summary>
        /// Approximate normal vectors of each vertex
        /// </summary>
        Vector3[] vertexNormals;

        /// <summary>
        /// Accelerations to be applied to the vertices for the
        /// next update.  These are essentially forces, which means
        /// we should divide by the mass of the vertex to get the
        /// accelerations, but we just assume all vertices are the
        /// same mass and so can ignore the difference between
        /// force and acceleration.
        /// </summary>
        Vector3[] accelerations;
        

        /// <summary>
        /// Whether a vertex is locked in position
        /// (not currently in use, as of the writing of these comments)
        /// </summary>
        public bool[] VertexFrozen { get; private set; }

        /// <summary>
        /// Original radius of the mesh when it was built as a sphere.
        /// </summary>
        float radius;

        /// <summary>
        /// Current volume of the mesh
        /// </summary>
        float volume;

        /// <summary>
        /// Equilibrium volume of the mesh
        /// </summary>
        float targetVolume = 8363;

        /// <summary>
        /// Ratio of volume to targetVolume.
        /// </summary>
        float compression = 1;

        /// <summary>
        /// True if currently reaching for the target
        /// </summary>
        bool reallyGrabbing;
        #endregion

        #region Mesh-related fields
        /// <summary>
        /// The normal of each triangle.  This is a vector pointing
        /// perpendicularly out from the triangle.
        /// 
        /// What we really need to know are the normals of mesh at each
        /// vertex (the graphics card needs to know it so it can shade
        /// the mesh properly and the physics system needs it so it can
        /// move the vertex in response to pressure).  However, we can only
        /// compute the normals of triangles, so we compute all the
        /// triangle normals and then say the normal of a vertex is the
        /// average of the normals of the triangles it's part of.
        /// </summary>
        Vector3[] triangleNormals;

        /// <summary>
        /// The normal is a unit vector.  This is the normal multiplied
        /// by the area of the triangle.  Used in computing the volume
        /// of the mesh.
        /// </summary>
        //Vector3[] triangleNormalAreas;

        /// <summary>
        /// Total number of triangles
        /// </summary>
        ushort triangleCount=0;

        /// <summary>
        /// All the edges in the mesh.
        /// Edges[v] is an array of *some* of the other vertices vertex number v
        /// is connected to.  Each entry in that array then corresponds to an edge.
        /// 
        /// Why only *some*?  Because this structure is used to iterate over all
        /// edges in the system.  We don't want to list the edges twice (e.g. listing
        /// both an edge from v1 to v2 and a separate edge from v2 to v1), so
        /// we store the edge under the vertex with the lowest vertex index.  That
        /// is, if v1 is less than v2, then edges[v1] will have v2 in it but edges[v2]
        /// won't have v1 in it.  If v2 is less, then edges[v2] will have v1, but not
        /// vice-versa.
        /// </summary>
        Edge[][] edges;
        Edge[][] symmetricalEdges;
        ushort[] distanceFromGrabPoint;
        #endregion

        #region Fields used to talk to the graphics card
        /// The graphics card wants data sent to it in a particular
        /// format that isn't necessarily convenient for the rest
        /// of the code, so we store it separately and update it
        /// at the end of each frame, before we redraw the mesh.

        /// <summary>
        /// Used to tell the graphics card what format the vertex data is in.
        /// </summary>
        //VertexDeclaration vertexDeclaration;
        /// <summary>
        /// Vertex information in the format communicated to the graphics card.
        /// </summary>
        VertexPositionNormalTexture[] vertexBufferData;
        /// <summary>
        /// List of all triangles in the mesh in the format the graphics card
        /// wants them in.  Each entry is a vertex number.  Every three vertices
        /// specify a triangel to be drawn.
        /// </summary>
        short[] indexBufferData;

        /// <summary>
        /// This is a thingy called a shader, which is a program that
        /// runs in the graphics card to do the actual drawing.  We're
        /// just using the standard default boring shade that Microsoft
        /// provides.
        /// </summary>
        BasicEffect effect;
        #endregion

        #region Other instance variables
        /// <summary>
        /// The object that's currently supporting the character
        /// </summary>
        public Obstacle Footing
        {
            get
            {
                return footing;
            }

            set
            {
                if (value != footing)
                {
                    if (footing != null)
                        footing.Dismount();
                    footing = value;
                    if (footing != null)
                        footing.Mount();
                }
            }
        }
        Obstacle footing;
        #endregion

        /// <summary>
        /// Makes an Amoeba character: builds the mesh and initializes the physics system.
        /// In the current system, Amoebas are always player characters and so you
        /// would never want to make more than one.
        /// </summary>
        /// <param Name="g">The game this character will be part of.</param>
        /// <param Name="radius">The radius of the character (it starts as a sphere)</param>
        public Amoeba(HappyFunBlobGame game, float radius) : base(game)
        {
            // BUILD MESH
            this.radius = radius;
            BuildMesh();

            Vector3 startingCenter = new Vector3(0, 120, 0);

            //BuildInnerShell();
            //AddStiffeningSprings();

            // VERTEX AND INDEX DATA STRUCTURES
            vertexBufferData = new VertexPositionNormalTexture[buildVertices.Count];
            indexBufferData = new short[triangleList.Count];
            for (int i = 0; i < triangleList.Count; i++)
                indexBufferData[i] = (short)triangleList[i];
            triangleNormals = new Vector3[triangleCount];
            //triangleNormalAreas = new Vector3[triangleCount];
            //vertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionNormalTexture.VertexElements);
            VertexPositions = buildVertices.ToArray();
            VertexPreviousPositions = buildVertices.ToArray();
            distanceFromGrabPoint = new ushort[VertexPositions.Length];
            markQueue = new ushort[VertexPositions.Length];

            for (int i = 0; i < buildVertices.Count; i++)
            {
                VertexPositions[i] += startingCenter;
                VertexPreviousPositions[i] += startingCenter;
            }

            originalVertexPositions = buildVertices.ToArray();
            vertexNormals = new Vector3[VertexPositions.Length];
            VertexFrozen = new bool[VertexPositions.Length];
            accelerations = new Vector3[buildVertices.Count];
            edges = new Edge[buildEdges.Count][];
            for (int i = 0; i < edges.Length; i++)
                edges[i] = buildEdges[i].ToArray();
            symmetricalEdges = new Edge[buildSymmetricalEdges.Count][];
            for (int i = 0; i < symmetricalEdges.Length; i++)
                symmetricalEdges[i] = buildSymmetricalEdges[i].ToArray();

            // Clear out the non-optimized versions of the mesh so the GC doesn't 
            // keep promoting them.  This is a minor optimization, and you shouldn't
            // worry about it.
            buildVertices = null;
            buildEdges = null;
            buildSymmetricalEdges = null;

            UpdateNormals();

            // GRAPHICS
            effect = new BasicEffect(game.GraphicsDevice);
            effect.Projection = game.ProjectionMatrix;
            effect.DiffuseColor = Color.Blue.ToVector3();
            effect.LightingEnabled = true;
            effect.EnableDefaultLighting();
            effect.PreferPerPixelLighting = true;
        }


        #region Update method
        /// <summary>
        /// Update the motion of the character based on physics,
        /// keyboard commands from the user, and any collisions
        /// with Obstacle objects like Orbs.
        /// 
        /// This is called automatically by the system.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            Profiler.Enter("Amoeba.Update");
            KeyboardState k = Keyboard.GetState();
            GamePadState p = GamePad.GetState(PlayerIndex.One);
            Grabbing = (k.IsKeyDown(Keys.Space) || p.IsButtonDown(Buttons.A)) && TouchingObject;

            if (Game.GroundPlane.TouchingAmoeba)
                GrabTarget = Game.StartOrb;

            if ((k.IsKeyDown(Keys.Tab) && previousKeyState.IsKeyUp(Keys.Tab))
                || (p.IsButtonDown(Buttons.LeftTrigger) && previousPadState.IsButtonUp(Buttons.LeftTrigger))
                || (p.IsButtonDown(Buttons.LeftShoulder) && previousPadState.IsButtonUp(Buttons.LeftShoulder)))
                //NextGrabTarget();
                AutoTarget();

            // Doesn't work yet - switches targets immediately on touching current target, which is bad
            //AutoTarget();

            gVector = TouchingObject ? Vector3.Zero : new Vector3(0, gravity, 0);

            //gripSurface = k.IsKeyDown(Keys.G);

            if (k.IsKeyDown(Keys.A))
                targetVolume += 100;
            if (k.IsKeyDown(Keys.S) && targetVolume > 3000)
                targetVolume -= 100;

            if (!GripSurface)
                for (int i = 0; i < VertexFrozen.Length; i++)
                    VertexFrozen[i] = false;

            //for (ushort i = 0; i < vertexPositions.Length; i++)
            //    accelerations[i] = Vector3.Zero;

            ComputeNonSpringForces();
            AddSpringForces();

            MaybeGrabTargetOrb(Grabbing);
            //UnwindTowardGrabPoint();
            MaybeCrawlToFirstOrb();

            IntegratePositions();
            ConstrainEdgeLengths();
            ProcessCollisions();

            UpdateFooting();

            UpdateNormals();

            previousKeyState = k;
            previousPadState = p;

            base.Update(gameTime);
            Profiler.Exit("Amoeba.Update");
        }
        /// <summary>
        /// The keys that were pressed last time we checked the
        /// keyboard, so we can determine which keys have just
        /// recently been pressed.
        /// </summary>
        KeyboardState previousKeyState = Keyboard.GetState();
        GamePadState previousPadState;
        #endregion



        #region Physics
        /// <summary>
        /// Compute forces on and accelerations of vertices
        /// due to pressure and gravity.
        /// </summary>
        void ComputeNonSpringForces()
        {
            Profiler.Enter("ComputeNonSpringForces");
            float pressure = 1 / Math.Max(volume, 5000);
            float exteriorPressure = 1 / targetVolume;
            float deltaPressure = pressure - exteriorPressure;
            compression = targetVolume / volume;

#if DEBUGCOMPRESSION
            if (compression > 1.2f)
                game.paused = true;
#endif

            if (volume < 100)
            {
                ResetMesh();
                deltaPressure = 0;
            }

            // Turn off gravity if we're holding on to something
            // This is just to make it look cooler, so the character doesn't
            // "drip" off the side of an orb.
            Vector3 g = gVector * (TouchingObject ? 1 : 3);

            // Since all vertices have the same mass, we treat forces as being accelerations
            // This is the same as assuming that all verices are unit mass
            for (ushort i = 0; i < VertexPositions.Length; i++)
                accelerations[i] = g + pressureForce * deltaPressure * vertexNormals[i];
            Profiler.Exit("ComputeNonSpringForces");
        }

        void ResetMesh()
        {
            for (ushort i = 0; i < VertexPositions.Length; i++)
                VertexPositions[i] = VertexPreviousPositions[i] = originalVertexPositions[i] + Position;
        }

        public void ResetPosition(Vector3 position)
        {
            Position = position;
            PreviousPosition = position;
            ResetMesh();
        }

        /// <summary>
        /// For each edge in edges[], compute the force it applies
        /// to the vertices on either end, and add it into accelerations[]
        /// </summary>
        void AddSpringForces()
        {
            Profiler.Enter("AddSpringForces");
            for (ushort v = 0; v < VertexPositions.Length; v++)
                foreach (Edge e in edges[v])
#if NonOptimizedAddSpringForces
                    AddSpringForce(v, e);
#else
                {
                    // Kids!  Don't do this at home.
                    // This is an ugly inlining and optimization of AddSpringForce()
                    // This kind of optimization usually isn't worth it, but this loop
                    // is the slowest single part of the the whole update system, and this
                    // speeds it up by about 35%.  But you don't do this kind of optimization
                    // until you have timing data that confirms the code you're optimizing is a bottleneck.
                    ushort v2 = e.vertex;
                    Vector3 offset;
                    Vector3.Subtract(ref VertexPositions[v], ref VertexPositions[v2], out offset);
                    float oLength = offset.Length();
                    float delta = e.length - oLength;
                    Vector3 acceleration;
                    Vector3.Multiply(ref offset, delta * springK / oLength, out acceleration);
                    Vector3.Add(ref accelerations[v], ref acceleration, out accelerations[v]);
                    Vector3.Subtract(ref accelerations[v2], ref acceleration, out accelerations[v2]);
                }
#endif

            Profiler.Exit("AddSpringForces");
        }

        /// <summary>
        /// Compute the force applied by the spring to its vertices
        /// and add it into their accelerations.
        /// </summary>
        void AddSpringForce(ushort v1, Edge e)
        {
            ushort v2 = e.vertex;
            Vector3 p1 = VertexPositions[v1];
            Vector3 p2 = VertexPositions[v2];
            Vector3 offset = p1 - p2;
            float oLength = offset.Length();
            float delta = e.length - oLength;
            Vector3 acceleration = offset * (delta * springK / oLength);
            accelerations[v1] += acceleration;
            accelerations[v2] -= acceleration;
        }

        void UnwindTowardGrabPoint()
        {
            float unwindGain = 1f;
            float forceLimit = .5f;
            float forceLimitSq = forceLimit * forceLimit;
            if (reallyGrabbing  && (Footing is Orb) && (Footing != GrabTarget))
            {
                Profiler.Enter("UnwindTowardGrabPoint");
                for (ushort i = 0; i < accelerations.Length; i++)
                {
                    if (distanceFromGrabPoint[i] > 10)
                    {
                        int targetDistance = distanceFromGrabPoint[i] - 1;
                        Vector3 sum = Vector3.Zero;
                        int count = 0;
                        foreach (var e in edges[i])
                            if (distanceFromGrabPoint[e.vertex] == targetDistance)
                            {
                                sum += VertexPositions[e.vertex];
                                count++;
                            }
                        if (count > 0)
                        {
                            Vector3 unwindForce = unwindGain * ((sum/count) - VertexPositions[i]);
                            float strength = unwindForce.LengthSquared();
                            if (strength > forceLimitSq)
                                unwindForce *= (float)(forceLimit / Math.Sqrt(strength));
                            accelerations[i] = unwindForce;
                        }
                    }
                }
                Profiler.Exit("UnwindTowardGrabPoint");
            }
        }

        /// <summary>
        /// Look for edges that are too long and forcibly move its
        /// vertices closer together.  Allows mesh to be stretchy
        /// but not crazy stretchy.
        /// </summary>
        void ConstrainEdgeLengths()
        {
            Profiler.Enter("ConstrainEdgeLengths");

            // We run the whole thing twice, since adjusting one edge might mess
            // up another.  Running more times would be better but would
            // take too long.
            int constraintIterations;

            if (compression > 1.1f)
                constraintIterations = 4;
            else if (compression > 1.05f)
                constraintIterations = 2;
            else
                constraintIterations = 1;

            for (int i = 0; i < constraintIterations; i++)
            {
                // FOR EACH VERTEX
                for (ushort v = 0; v < VertexPositions.Length; v++)
                    if (!VertexFrozen[v])
                        // AND FOR EACH VERTEX CONNECTED TO IT
                        foreach (Edge e in edges[v])
                            if (!VertexFrozen[e.vertex])
                            {
                                // DETERMINE IF ITS TOO LONG
                                Vector3 p1 = VertexPositions[v];
                                Vector3 p2 = VertexPositions[e.vertex];
                                Vector3 delta = p1 - p2;
                                float maxLength = e.length * maxStretchFactor;
                                float maxLengthSquared = maxLength * maxLength;
                                float realLengthSquared = delta.LengthSquared();
                                if (realLengthSquared > maxLengthSquared)
                                {
                                    // AND IF SO FORCIBLY SHORTEN IT
                                    delta *= maxLengthSquared / (realLengthSquared + maxLengthSquared) - 0.5f;
                                    VertexPositions[v] += delta;
                                    VertexPositions[e.vertex] -= delta;
                                }
                            }
            }
            Profiler.Exit("ConstrainEdgeLengths");
        }

        /// <summary>
        /// Determine the new positions of vertices based on the current and previous
        /// positions, as well as their accelerations and damping.  This uses a cute
        /// technique called Verlet integration, which is outside the scope of this course.
        /// But there's a good description of it on wikipedia, if you're interested.
        /// </summary>
        void IntegratePositions()
        {
            Profiler.Enter("IntegratePositions");
            Vector3 integral = Vector3.Zero;
            float realDamping = damping * ((volume < targetVolume * 0.9f) ? 4 : 1);
            float k1 = (2 - realDamping);
            float k2 = (1 - realDamping);
            // INTEGRATE POSITIONS
            for (ushort i = 0; i < VertexPositions.Length; i++)
            {
                Vector3 current = VertexPositions[i];
                if (!VertexFrozen[i])
                    integral += VertexPositions[i] = k1 * current - k2 * VertexPreviousPositions[i] + ClipFinite(accelerations[i], 10);
                VertexPreviousPositions[i] = current;
            }

            PreviousPosition = Position;
            Position = integral / VertexPositions.Length;
            Profiler.Exit("IntegratePositions");
        }

        /// <summary>
        /// Check that all of a vector's components are finite, and
        /// reset them to limit, if there's not.
        /// 
        /// "Infinite" values can happen if dividing by zero or a very
        /// small number.
        /// 
        /// This is a stopgap to keep things from blowing up
        /// when the numerical simulation misbehaves.
        /// </summary>
        static Vector3 ClipFinite(Vector3 v, float limit)
        {
            if (Math.Abs(v.X) > limit)
            {
                if (v.X > 0)
                    v.X = limit;
                else
                    v.X = -limit;
            }

            if (Math.Abs(v.Y) > limit)
            {
                if (v.Y > 0)
                    v.Y = limit;
                else
                    v.Y = -limit;
            }

            if (Math.Abs(v.Z) > limit)
            {
                if (v.Z > 0)
                    v.Z = limit;
                else
                    v.Z = -limit;
            }

            return v;
        }
        #endregion



        #region Grabbing and targeting
        void MaybeGrabTargetOrb(bool enable)
        {
            reallyGrabbing = false;
            // Only allow grabbing it we're close to target volume.
            // Otherwise, it's easy for the mesh to fold itself
            // inside out.
            if (enable && volume>(targetVolume*0.97f))
            {
                Profiler.Enter("MaybeGrabTargetOrg");
                reallyGrabbing = true;
                if (GrabTarget == null)
                    NextGrabTarget();

                // Find nearest vertex of ameoeba to target
                ushort nearestVertex = 0;
                float distSq = float.PositiveInfinity;
                for (ushort i = 0; i < VertexPositions.Length; i++)
                {
                    float d = (VertexPositions[i] - GrabTarget.Position).LengthSquared();
                    if (d < distSq)
                    {
                        distSq = d;
                        nearestVertex = i;
                    }
                }
                float grabDistanceSquared = distSq * 1.3f;
                if (grabDistanceSquared < 100 * 100 || Game.GroundPlane.TouchingAmoeba)
                {
                    grabDistanceSquared = Math.Min(grabDistanceSquared, 100 * 100);
                    for (ushort i = 0; i < VertexPositions.Length; i++)
                    {
                        if ((VertexPositions[i] - GrabTarget.Position).LengthSquared() < grabDistanceSquared)
                            accelerations[i] += ((distSq > GrabTarget.RadiusSquared) ? 5f : 3f) * (GrabTarget.Position - VertexPositions[i]).ClipMagnitude(2, 5);
                    }
                }
                ComputeVertexDistances(nearestVertex);
                Profiler.Exit("MaybeGrabTargetOrg");
            }
        }

        ushort[] markQueue;
        void ComputeVertexDistances(ushort start)
        {
            int qIn = 0;
            int qOut = 0;
            markQueue[qIn++] = start;
            ushort maxDistance = 0;

            distanceFromGrabPoint.Initialize();
            distanceFromGrabPoint[start] = 1;
            while (qOut<qIn) {
                ushort v = markQueue[qOut++];
                ushort d = (ushort)(distanceFromGrabPoint[v]+1);
                if (d > maxDistance)
                    maxDistance = d;
                foreach (var neighbor in symmetricalEdges[v])
                    if (distanceFromGrabPoint[neighbor.vertex] == 0)
                    {
                        distanceFromGrabPoint[neighbor.vertex] = d;
                        markQueue[qIn++] = neighbor.vertex;
                    }
            }
        }

        /// <summary>
        /// Switch target to next Orb.
        /// </summary>
        void NextGrabTarget()
        {
            int index;
            if (GrabTarget == null)
                index = -1;
            else
                index = Game.Components.IndexOf(GrabTarget);

            index++;
            while (!(Game.Components[index % Game.Components.Count] is Orb))
                index++;
            GrabTarget = (Orb)Game.Components[index % Game.Components.Count];
        }

        static bool IsTouchingObstacle(IGameComponent c)
        {
            Obstacle o = c as Obstacle;
            return o != null && o.TouchingAmoeba;
        }

        void UpdateFooting()
        {
            Footing = null;
            float height = float.PositiveInfinity;

            // Find the lowest object we're touching
            foreach (var o in touchingNow)
            {
                if (o.Position.Y < height)
                {
                    Footing = o;
                    height = o.Position.Y;
                }
            }
        }
        public void AutoTarget()
        {
            if (Footing != null)
            {
                if (Footing == Game.EndOrb)
                    Game.NextLevel();
                else
                {
                    // Find the next Orb after it.
                    //for (c++; c < Game.Components.Count && !(Game.Components[c] is Orb); c++) ;
                    //if (c < Game.Components.Count)
                    //    grabTarget = (Orb)Game.Components[c];

                    float dSquared = float.PositiveInfinity;
                    Orb target = null;
                    foreach (var comp in Game.Components)
                    {
                        Orb s = comp as Orb;
                        if (s != null && s.Position.Y > Footing.Position.Y)
                        {
                            float dSq = (Position - s.Position).LengthSquared();
                            if (dSq < dSquared)
                            {
                                target = s;
                                dSquared = dSq;
                            }
                        }
                    }

                    if (target != null)
                        GrabTarget = target;
                }
            }
            // Else we're floating in the air, so can't target

        }

        void MaybeCrawlToFirstOrb()
        {
            if (Footing != null && Footing is GroundPlane && GrabTarget != null)
            {
                Vector3 toOrb = GrabTarget.Position - Position;
                float distToOrb = toOrb.Length();

                if (distToOrb > 20)
                {
                    toOrb *= 0.01f / distToOrb;
                    for (ushort i = 0; i < VertexPositions.Length; i++)
                        if (VertexPositions[i].Y > 0)
                            accelerations[i] += toOrb;
                        else
                            accelerations[i] = Vector3.Zero;
                }
            }
        }
        #endregion



        #region Collision detection and handling
        List<Obstacle> touchedPreviously = new List<Obstacle>();
        List<Obstacle> touchingNow = new List<Obstacle>();

        /// <summary>
        /// Check for collisions between character and each of the Obstacles.
        /// </summary>
        void 
            ProcessCollisions()
        {
            Profiler.Enter("ProcessCollisions");
            UpdateBoundingBox();

            TouchingObject = false;

            // Swap touchingNow and touchedPreviously
            List<Obstacle> swap = touchingNow;
            touchingNow = touchedPreviously;
            touchedPreviously = swap;
            touchingNow.Clear();

#if BSPTrees
            Game.GameObjectTree.DoAllIntersectingObjects(BoundingBox, delegate(GameObject c)
#else
            foreach (var c in Game.Components)
#endif
            {
                Obstacle o = c as Obstacle;
                if (o != null)
                {
                    o.TestCollisions(this);
                    if (o.TouchingAmoeba)
                        // Add it to the list of things touched this frame.
                        touchingNow.Add(o);

                    // Cross it off of the list of things touched last frame
                    // so we don't clear its TouchingAmoeba flag below
                    touchedPreviously.Remove(o);
                }
            }
#if BSPTrees
            );
#endif

            // clear touched fields of any remaining touched objects from last time
            foreach (var old in touchedPreviously)
                old.TouchingAmoeba = false;


            //foreach (var c in Game.Components)
            //{
            //    Obstacle o = c as Obstacle;
            //    if (o != null)
            //    {
            //        if (o.BoundingBox.Intersects(boundingBox))
            //            o.TestCollisions(this);
            //        else
            //            o.TouchingAmoeba = false;
            //    }
            //}
            Profiler.Exit("ProcessCollisions");
        }


        public override BoundingBox BoundingBox
        {
            get
            {
                return boundingBox;
            }
        }
        BoundingBox boundingBox;

        void UpdateBoundingBox()
        {
            Vector3 min = VertexPositions[0];
            Vector3 max = VertexPositions[0];
            for (int i = 1; i < VertexPositions.Length; i++)
            {
                Vector3.Max(ref max, ref VertexPositions[i], out max);
                Vector3.Min(ref min, ref VertexPositions[i], out min);
            }
            boundingBox = new BoundingBox(min, max);
        }
        #endregion



        #region Vertex, triangle, and vertex buffer management
        /// <summary>
        /// Computes the normal vector of a vertex as the average of the normals
        /// of the triangles to which it is adjacent.
        /// </summary>
        Vector3 NormalOfVertex(ushort i)
        {
            Vector3 n = Vector3.Zero;
            List<ushort> adj = adjacentTriangles[i];
            foreach (ushort tri in adj)
                //n += triangleNormals[tri];
                // Think of the code as being the line above.  The line
                // below is an optimized version because this is a performance
                // critical routine.
                Vector3.Add(ref n, ref triangleNormals[tri], out n);
            float lenSq = n.LengthSquared();
            if (lenSq > 0.0001f)
                Vector3.Multiply(ref n, (float)(1 / Math.Sqrt(lenSq)), out n);
            return n;
        }

        //Vector3 OrientedAreaOfVertex(ushort i)
        //{
        //    Vector3 n = Vector3.Zero;
        //    List<ushort> adj = adjacentTriangles[i];
        //    foreach (ushort tri in adj)
        //        //n += triangleNormalAreas[tri];
        //        // Think of the code as being the line above.  The line
        //        // below is an optimized version because this is a performance
        //        // critical routine.
        //        Vector3.Add(ref n, ref triangleNormalAreas[tri], out n);
        //    Vector3.Multiply(ref n, 1f / adj.Count, out n);  //n *= (1f / adj.Count);
        //    return n;
        //}

        /// <summary>
        /// The normal of a triangle formed by v1, v2, v3.
        /// NOTE: Technically, twice the area.
        /// </summary>
        static Vector3 TriangleNormalArea(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return Vector3.Cross(v1 - v3, v1 - v2);
        }

        /// <summary>
        /// Recompute triangleNormals[]
        /// </summary>
        void UpdateNormals()
        {
            Profiler.Enter("UpdateNormals");
            int v = 0;
            float newVolume = 0;
            for (int i = 0; i < triangleCount; i++)
            {
                Vector3 n = TriangleNormalArea(VertexPositions[triangleList[v]], VertexPositions[triangleList[v + 1]], VertexPositions[triangleList[v + 2]]);
                //triangleNormalAreas[i] = n;
                // To be more accurate, we should use the average of the three vertices,
                // but I doubt it's worth it in practice.
                newVolume += n.X * VertexPositions[triangleList[v]].X;
                n.Normalize();
                triangleNormals[i] = n;
                v += 3;
            }
            this.volume = newVolume;

            for (ushort i = 0; i < vertexNormals.Length; i++)
                vertexNormals[i] = NormalOfVertex(i);
            Profiler.Exit("UpdateNormals");
        }
        /// <summary>
        /// Update all positions and normals in vertex buffer for graphics card.
        /// </summary>
        void UpdateVertexBufferData()
        {
            for (ushort i = 0; i < vertexBufferData.Length; i++)
                vertexBufferData[i] = new VertexPositionNormalTexture(VertexPositions[i], vertexNormals[i], new Vector2(0, 0));
        }
        #endregion



        #region Drawing
        /// <summary>
        /// Redraw the mesh.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Profiler.Enter("Amoeba.Draw");
            UpdateVertexBufferData();
            effect.View = Game.ViewMatrix;
            //float delta = 4f*(targetVolume - volume) / targetVolume;
            //effect.DiffuseColor = new Vector3(Math.Max(0,Math.Min(1,delta)), 0, Math.Max(0,Math.Min(1,1-delta)));
            effect.DiffuseColor = Color.Gray.ToVector3();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Profiler.Enter("DrawUserIndexedPrimitives");
                Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertexBufferData, 0, vertexBufferData.Length,
                    indexBufferData, 0, indexBufferData.Length / 3);
                Profiler.Exit("DrawUserIndexedPrimitives");
            }
            base.Draw(gameTime);
            Profiler.Exit("Amoeba.Draw");
        }
        #endregion



        #region Building the mesh
        ///
        /// This code initializes the mesh.
        /// We start out with the mesh as a sphere.
        /// 

        /// <summary>
        /// Creates a SimpleMesh containing a sphere.
        /// </summary>
        void BuildMesh()
        {
            int level = 4;

            ushort XPLUS = AllocateVertex(1, 0, 0);	// X
            ushort XMIN = AllocateVertex(-1, 0, 0);	// -X
            ushort YPLUS = AllocateVertex(0, 1, 0);	//  Y
            ushort YMIN = AllocateVertex(0, -1, 0);	// -Y
            ushort ZPLUS = AllocateVertex(0, 0, 1);	//  Z
            ushort ZMIN = AllocateVertex(0, 0, -1);	// -Z

            // Vertices of a unit octahedron
            Triangulate(level, XPLUS, ZPLUS, YPLUS);
            Triangulate(level, YPLUS, ZPLUS, XMIN);
            Triangulate(level, XMIN, ZPLUS, YMIN);
            Triangulate(level, YMIN, ZPLUS, XPLUS);
            Triangulate(level, XPLUS, YPLUS, ZMIN);
            Triangulate(level, YPLUS, XMIN, ZMIN);
            Triangulate(level, XMIN, YMIN, ZMIN);
            Triangulate(level, YMIN, XPLUS, ZMIN);

            // Get rid of the hash table so the GC doesn't have to keep promoting it.
            midpoints = null;
        }

        void Triangulate(int level, ushort p1, ushort p2, ushort p3)
        {
            if (level < 1)
            {
                AddEdgeSpring(p1, p2);
                AddEdgeSpring(p2, p3);
                AddEdgeSpring(p3, p1);
            }

            if (level == 0)
            {
                AddTriangle(p1, p2, p3);
            }
            else
            {
                System.Diagnostics.Debug.Assert(MidpointVertex(p1, p2) == MidpointVertex(p1, p2));
                ushort m12 = MidpointVertex(p1, p2);
                ushort m23 = MidpointVertex(p2, p3);
                ushort m31 = MidpointVertex(p3, p1);

                Triangulate(level - 1, p1, m12, m31);
                Triangulate(level - 1, m12, p2, m23);
                Triangulate(level - 1, m31, m23, p3);
                Triangulate(level - 1, m12, m23, m31);
            }
        }


        List<Vector3> buildVertices = new List<Vector3>();
        List<List<Edge>> buildEdges = new List<List<Edge>>();
        List<List<Edge>> buildSymmetricalEdges = new List<List<Edge>>();

        List<List<ushort>> adjacentTriangles = new List<List<ushort>>();

        List<ushort> triangleList = new List<ushort>();

        /// <summary>
        /// Represents an edge of the mesh.
        /// </summary>
        struct Edge
        {
            /// <summary>
            /// Vertex the edge is joined to.
            /// </summary>
            public ushort vertex;
            /// <summary>
            /// Original length of the edge when the mesh was originally formed.
            /// </summary>
            public float length;

            /// <summary>
            /// Represents an edge of length l from some vertex to vertex v.
            /// </summary>
            public Edge(ushort v, float l)
            {
                vertex = v;
                length = l;
            }
        }

        /// <summary>
        /// Forcibly add a vertex, normalized to length RADIUS, to the VERTICES list, and return its index in the list.
        /// Does not check for duplications, since we then have to worry about
        /// round-off error in the coordinates.  Instead we will hash on indices
        /// (see MidpointVertex).
        /// </summary>
        ushort AllocateVertex(Vector3 v)
        {
            ushort index = (ushort)buildVertices.Count;
            
            buildVertices.Add(v);
            adjacentTriangles.Add(new List<ushort>());
            buildEdges.Add(new List<Edge>());
            buildSymmetricalEdges.Add(new List<Edge>());
            return index;
        }

        ushort AllocateNormalizedVertex(Vector3 v)
        {
            v.Normalize();
            return AllocateVertex(v * radius);
        }

        ushort AllocateVertex(float x, float y, float z)
        {
            return AllocateNormalizedVertex(new Vector3(x, y, z));
        }

        Dictionary<IndexPair, ushort> midpoints = new Dictionary<IndexPair, ushort>();
        /// <summary>
        /// Returns an index in VERTICES list for the midpoint of v1 and v2, as
        /// specified by their vertices.  If the midpoint is already in the table,
        /// its index is returned rather than the midpoint being duplicated.
        /// </summary>
        ushort MidpointVertex(ushort v1, ushort v2)
        {
            IndexPair key = new IndexPair(v1, v2);
            ushort index;

            if (!midpoints.TryGetValue(key, out index))
            {
                index = AllocateNormalizedVertex((buildVertices[v1] + buildVertices[v2]) * 0.5f);
                midpoints[key] = index;
            }
            return index;
        }

        void AddTriangle(ushort v1, ushort v2, ushort v3)
        {
            triangleList.Add(v1);
            triangleList.Add(v2);
            triangleList.Add(v3);
            adjacentTriangles[v1].Add(triangleCount);
            adjacentTriangles[v2].Add(triangleCount);
            adjacentTriangles[v3].Add(triangleCount);
            triangleCount++;
        }

        void AddEdgeSpring(ushort v1, ushort v2)
        {
            if (v2 < v1)
            {
                ushort t = v1;
                v1 = v2;
                v2 = t;
            }
            Edge e = new Edge(v2, (buildVertices[v1] - buildVertices[v2]).Length());
            buildEdges[v1].Add(e);
            buildSymmetricalEdges[v1].Add(e);
            buildSymmetricalEdges[v2].Add(new Edge(v1, e.length));
        }

#if StiffeningSprings
        void AddStiffeningSprings()
        {
            for (ushort v = 1; v < buildVertices.Count; v++)
            {
                for (int j = 0; j < 5; j++)
                {
                    ushort e = v;
                    for (int i = 0; i < 3; i++)
                    {
                        var edges = buildEdges[e];
                        if (edges.Count == 0)
                            break;
                        else
                            e = edges[random.Next(0, edges.Count)].vertex;
                    }
                    if (v!=e)
                        AddEdgeSpring(v, e);
                }
            }
        }
#endif

        /// <summary>
        /// Used as keys for the hash table of vertex buffer indicies.
        /// We use this to make sure we don't make the same vertex twice.
        /// </summary>
        struct IndexPair
        {
            public ushort a, b;
            public IndexPair(ushort a, ushort b)
            {
                if (a < b)
                {
                    this.a = a;
                    this.b = b;
                }
                else
                {
                    this.a = b;
                    this.b = a;
                }
            }

            public override int GetHashCode()
            {
                return a.GetHashCode() ^ b.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is IndexPair)
                {
                    IndexPair p = (IndexPair)obj;
                    return a == p.a && b == p.b;
                }
                else
                    return false;
            }

            static public bool operator ==(IndexPair a, IndexPair b)
            {
                return a.a == b.a && b.b == a.b;
            }

            static public bool operator !=(IndexPair a, IndexPair b)
            {
                return !(a == b);
            }
        }

        #endregion
    }
}