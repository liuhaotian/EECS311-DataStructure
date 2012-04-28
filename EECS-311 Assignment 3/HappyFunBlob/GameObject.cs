using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HappyFunBlob
{
    /// <summary>
    /// This is the parent class of the player and every object
    /// the player can interact with.
    /// </summary>
    public abstract class GameObject : DrawableGameComponent
    {
        /// <summary>
        /// Constructor.  Just fills in the game field.
        /// </summary>
        protected GameObject(HappyFunBlobGame theGame)
            : base(theGame)
        {
            Game = theGame;
        }

        /// <summary>
        /// The game this object is a component of.
        /// </summary>
        protected new HappyFunBlobGame Game { get; private set; }

        /// <summary>
        /// Position of the center or centroid of the objecct.
        /// </summary>
        public Vector3 Position { get; protected set; }

        /// <summary>
        /// The smallest, axis-aligned box that fully contains the object.
        /// </summary>
        public abstract BoundingBox BoundingBox
        {
            get;
        }

        /// <summary>
        /// If false, the object isn't in view of the camera.
        /// If true, it probably is visible, but it isn't guaranteed.
        /// </summary>
        public virtual bool InView
        {
            get
            {
                return Game.ViewFrustum.Contains(BoundingBox) != ContainmentType.Disjoint;
            }
        }

        /// <summary>
        /// Type of handlers for scriptable in-game events.
        /// </summary>
        public delegate void GameEventHandler();
    }
}
