using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HappyFunBlob
{
    /// <summary>
    /// This is the parent class of all the things the player can
    /// collide with, both Orbs and Blocks.
    /// </summary>
    public abstract class Obstacle : GameObject
    {
        /// <summary>
        /// Fills in position and initializes fields.
        /// </summary>
        protected Obstacle(HappyFunBlobGame game, Vector3 position)
            : base(game)
        {
            Position = PreviousPosition = position;
            StartingPosition = position;
            OscillationDirection = Vector3.Up;
            OscillationAmplitude = 0;
            OscillationPeriod = 5;
        }

        /// <summary>
        /// The color of the object.
        /// </summary>
        public Vector3 Color { get; set; }

        /// <summary>
        /// Position of the object in the previous frame.
        /// </summary>
        public Vector3 PreviousPosition { get; protected set; }

        /// <summary>
        /// Original position of the object, as specifiedi n the level file.
        /// </summary>
        public Vector3 StartingPosition { get; private set; }

        /// <summary>
        /// Direction the object should oscillate in
        /// </summary>
        public Vector3 OscillationDirection { get; set; }

        /// <summary>
        /// Number of seconds between cycles of the object's oscillation.
        /// </summary>
        public float OscillationPeriod { get; set; }

        /// <summary>
        /// Half-distance the object travels during oscillation.
        /// If this is zero, the object doesn't oscillate.
        /// </summary>
        public float OscillationAmplitude { get; set; }

        /// <summary>
        /// A procedure that can be called to initialize the optional fields
        /// of the object that are specified in the level file.
        /// </summary>
        /// <param name="o">The object to be initialized</param>
        public delegate void Initializer(Obstacle objectToBeInitialized);

        bool touchingAmoeba;

        /// <summary>
        /// True if this object is presently in contact with the player
        /// </summary>
        public bool TouchingAmoeba
        {
            get
            {
                return touchingAmoeba;
            }

            set
            {
                if (value != touchingAmoeba)
                {
                    if (value)
                    {
                        if (Touched != null)
                            Touched();
                    }
                    else
                    {
                        if (Untouched != null)
                            Untouched();
                    }
                }
                touchingAmoeba = value;
            }
        }

        /// <summary>
        /// Signaled when the amoeba first touches this object
        /// </summary>
        public event GameEventHandler Touched;

        /// <summary>
        /// Signaled when the amoeba stops touching this object.
        /// </summary>
        public event GameEventHandler Untouched;

        /// <summary>
        /// Signaled when the amoeba begins to rest on this object
        /// </summary>
        public event GameEventHandler Mounted;

        /// <summary>
        /// Signaled when the amoeba stops resting on this object
        /// </summary>
        public event GameEventHandler Dismounted;

        /// <summary>
        /// Called when the amoeba begins to rest on this object
        /// </summary>
        public virtual void Mount()
        {
            if (Mounted != null)
                Mounted();
        }

        /// <summary>
        /// Called when the amoeba stops resting on this object
        /// </summary>
        public virtual void Dismount()
        {
            if (Dismounted != null)
                Dismounted();
        }

        /// <summary>
        /// Tests the vertices of the amoeba's mesh for intersection
        /// with this objects and corrects their positions when intersecting.
        /// </summary>
        /// <param name="a"></param>
        public abstract void TestCollisions(Amoeba player);

        /// <summary>
        /// A callback for updating the object.
        /// </summary>
        public delegate void Updater(GameTime time);

        /// <summary>
        /// Signaled when this object is updated.
        /// </summary>
        public event Updater Updaters;

        /// <summary>
        /// Update the object's position and call Updaters.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            PreviousPosition = Position;
            Position = StartingPosition + OscillationDirection * (OscillationAmplitude * (float)Math.Sin((2/60f)*Math.PI*Game.FrameCount/OscillationPeriod));
            if (Updaters != null)
                Updaters(gameTime);

            base.Update(gameTime);
        }
    }
}
