using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace HappyFunBlob
{
    /// <summary>
    /// This is the class of the data object that represents the game as a whole.
    /// It contains fields and properties for the graphics device (the interface to
    /// video card, the game's "components" (all the things that get displayed or
    /// updated in the course of the game), and various other things, like the
    /// transformation matrices representing the current viewpoint.
    /// </summary>
    public class HappyFunBlobGame : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Used for setting up the graphics device (i.e. the video card)
        /// This is *not* the graphics device itself.
        /// </summary>
        GraphicsDeviceManager graphicsDeviceManager;

        /// <summary>
        /// The number of update cycles that have been performed.
        /// This is different from the GameTime parameter which is
        /// passed into the update and draw calls, because they
        /// continue advancing when the game is paused.
        /// </summary>
        public long FrameCount { get; private set; }

        /// <summary>
        /// Used to draw sprites (2D images) and text on the display.
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Font to use when drawing on the screen
        /// </summary>
        public SpriteFont Font { get; private set; }

        #region Camera control fields
        /// <summary>
        /// Distance of the virtual camera from whatever object it's tracking
        /// (usually either the amoeba or the orb it's resting on)
        /// </summary>
        float cameraDistance = 75;

        /// <summary>
        /// The forward direction of the camera.
        /// </summary>
        Vector3 cameraDirection = Vector3.Forward;

        /// <summary>
        /// The camera's tilt up or down
        /// </summary>
        float cameraPitch = 0;

        /// <summary>
        /// True if this is the opening sequence of the game
        /// (causes the camera to zoom in from a long shot to
        /// a medium shot.
        /// </summary>
        bool openingSequence = true;

        /// <summary>
        /// Position of whatever orb or block we're tracking
        /// </summary>
        public Vector3 fixedTargetPosition;
        /// <summary>
        /// Camera position is a weighted sum of fixedTargetPosition
        /// and the centroid of the amoeba.  This is the weight
        /// applied to the fixed target.  When the amoeba attaches
        /// to a target, we do a smooth pan from the amoeba to the
        /// target by changing this weight.
        /// </summary>
        public float fixedTargetFraction = 0;

        /// <summary>
        /// Maps world-centered coordinates to camera-centered (but 
        /// still 3D) coordinates.  Varies based on player position,
        /// fixedTargetPosition, fixedTargetFraction, cameraDistance,
        /// cameraPitch, and cameraDirection.
        /// </summary>
        public Matrix ViewMatrix { get; private set; }

        /// <summary>
        /// Maps camera-centered 3D coordinates, obtained using
        /// ViewMatrix to 2D image plane coordinates.  Fixed at
        /// the start of the game.
        /// </summary>
        public Matrix ProjectionMatrix { get; private set; }

        public BoundingFrustum ViewFrustum { get; private set; }

        #endregion

        /// <summary>
        /// The "player character", an amoeba-like creature.
        /// </summary>
        public Amoeba Amoeba { get; private set; }

        /// <summary>
        /// The "ground", i.e. the thing at the bottom the player falls on,
        /// and more importantly, can't pass through.
        /// </summary>
        public GroundPlane GroundPlane { get; private set; }

        public BSPTree GameObjectTree { get; set; }

        #region Level system
        /// <summary>
        /// Array of the different game levels, 0 the easiest, 1, harder, etc.
        /// </summary>
        IList<Level> allLevels;

        /// <summary>
        /// The level we're currently on.
        /// </summary>
        int currentLevelNumber = -1;

        /// <summary>
        /// The last Orb on the current level
        /// </summary>
        public Orb EndOrb { get; set; }

        /// <summary>
        /// Starting orb on the current level
        /// This isn't necessarily the orb you start
        /// at, but is where the player starts over
        /// when they land on the ground plane.
        /// </summary>
        public Orb StartOrb { get; set; }
        #endregion

        /// <summary>
        /// Constructor for the game objecct
        /// </summary>
        public HappyFunBlobGame()
        {
            //Profiler.EnableProfiling();
            //Profiler.Enter("Startup");
            //Profiler.Enter("Game constructor");
            // The GraphicsDeviceManager is used to connect up to
            // and configure the GraphicsDevice, which interfaces
            // the game to the video card.
            graphicsDeviceManager = new GraphicsDeviceManager(this);
#if !DEBUG
            // Run full screen for release mode, but not debug mode
            // since that can make it hard to get to the debugger
            // if you hit an exception or breakpoint.
            graphicsDeviceManager.IsFullScreen = true;
#endif
            // Sets up a callback to set the windows size to the screen size, once
            // the graphics device manager knows what screen we're displaying on.
            graphicsDeviceManager.PreparingDeviceSettings += delegate(object sender, PreparingDeviceSettingsEventArgs e)
                                                        {
                                                            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                                                            e.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat = displayMode.Format;
                                                            e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = displayMode.Width;
                                                            e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = displayMode.Height;
                                                        };
            // Tells the game where to find media assets like fonts and textures.
            Content.RootDirectory = "Content";
            // Turns on anti-aliasing to smooth out jaggies on sharp edges.
            graphicsDeviceManager.PreferMultiSampling = true;
            //Profiler.Exit("Game constructor");
        }

        public BasicEffect BasicEffect { get; private set; }

        /// <summary>
        /// This gets called by XNA after the graphics device is set up.
        /// We do all the initialization here that we can't do in the constructor
        /// because it requires access to the graphics device.
        /// </summary>
        protected override void Initialize()
        {
            //Profiler.Enter("Initialize");
            // Tell XNA to call update every 60th of a second, even if it can
            // go faster, or has to skip drawing the screen to keep up.  We
            // need this because the physics integration will go unstable if we
            // start changing the time step around.
            IsFixedTimeStep = true;

            // Configure the antialiasing hardware.
            //GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.FourSamples;
            //GraphicsDevice.RenderState.MultiSampleAntiAlias = true;

            // Set up the projection matrix to simulate a lens with a 45 degree field of view.
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                                                            MathHelper.ToRadians(45),
                                                            (float)GraphicsDevice.Viewport.Width /
                                                            (float)GraphicsDevice.Viewport.Height,
                                                            1.0f, 500.0f);

            // Creating basic effect objects turns out to be slow, as compared to cloning them
            // So we create one here, that then gets cloned by orbs, etc.
            BasicEffect = new BasicEffect(GraphicsDevice);
            BasicEffect.Projection = ProjectionMatrix;
            BasicEffect.EnableDefaultLighting();
            BasicEffect.PreferPerPixelLighting = true;


            // Create the game objects that are commmon to all game levels.
            Components.Add(new Profiler(this));                     // Tracks execution performance of different parts of the code
            Components.Add(Amoeba = new Amoeba(this, 10));          // The player
            Components.Add(GroundPlane = new GroundPlane(this));    // The ground
            Components.Add(new Help(this));                         // Simple help and tutorial system
            Components.Add(new ParticleSystem(this, 2000, 500));

            //Profiler.Enter("base.Initialize");
            base.Initialize();
            //Profiler.Exit("base.Initialize");
            
            // Load all the level files in at once
            //Profiler.Enter("ReadLevels");
            allLevels = Level.ReadLevels();
            //Profiler.Exit("ReadLevels");
            //Profiler.Enter("NextLevel");
            // Start the first level
            NextLevel();
            //Profiler.Exit("NextLevel");
            //Profiler.Enter("AutoTarget");
            // Point the player at the first target
            Amoeba.AutoTarget();
            //Profiler.Exit("AutoTarget");
            //Profiler.Exit("Initialize");
        }

        /// <summary>
        /// Switches game to the next level.
        /// This requires blowing away all the orbs and blocks
        /// of the previous level and creating the objects for
        /// the new level, as well as resetting the position
        /// and shape of the amoeba.
        /// </summary>
        public void NextLevel()
        {
            if (currentLevelNumber<allLevels.Count-1)
                allLevels[++currentLevelNumber].Install(this);
        }

        public void Restart()
        {
            currentLevelNumber = 0;
            allLevels[currentLevelNumber].Install(this);
        }

        /// <summary>
        /// Reads media assets in from disk.
        /// </summary>
        protected override void LoadContent()
        {
            //Profiler.Enter("LoadContent");            
            // Create a new SpriteBatch, which can be used to draw textures.
            // I would have put this in Initialize, but XNA puts it here.  I'm
            // sure there's a good reason for it.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            // Load the font in from disk.
            Font = Content.Load<SpriteFont>("MainFont");
            //Profiler.Exit("LoadContent");
        }

        KeyboardState previousKeyState;
        KeyboardState currentKeyState;

        /// <summary>
        /// True if key is currently held down.
        /// </summary>
        public bool IsKeyDown(Keys key)
        {
            return currentKeyState.IsKeyDown(key);
        }

        /// <summary>
        /// True if key has just been pressed.
        /// Will not return true again until key
        /// is released and repressed.
        /// </summary>
        public bool IsJustPressed(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

        /// <summary>
        /// If true, calls to the Update() methods of game objects are suspended.
        /// </summary>
        bool paused = false;

        /// <summary>
        /// Update the states of the objects in the game.  Also handles keyboard
        /// input and camera update.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //Profiler.MaybeExit("Startup");
            Profiler.EnableProfiling();
            Profiler.Enter("Update");
            previousKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();
            GamePadState p = GamePad.GetState(PlayerIndex.One);
            UpdateCamera();

            bool singleStep = false;

            if (IsKeyDown(Keys.Escape) || IsKeyDown(Keys.Q) || IsKeyDown(Keys.X) || p.IsButtonDown(Buttons.B))
                Exit();

            if (p.IsButtonDown(Buttons.Y))
                Restart();

            if (IsJustPressed(Keys.Delete))
                paused = !paused;
            if (IsJustPressed(Keys.F10))
                singleStep = true;

            if (!paused || singleStep)
            {
                FrameCount++;
                // Update everything else
                base.Update(gameTime);
            }
            Profiler.Exit("Update");
        }

        /// <summary>
        /// Update the position of the camera based on amoeba position
        /// and player input.
        /// </summary>
        private void UpdateCamera()
        {
            GamePadState p = GamePad.GetState(PlayerIndex.One);
            // Slowly zoom in if at the start of the game.
            if (openingSequence)
            {
                if (cameraDistance >= 150)
                    openingSequence = false;
                else
                    cameraDistance += 0.5f;
            }

            // Pan control
            if (IsKeyDown(Keys.Left))
                cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateRotationY(-0.02f));
            else if (IsKeyDown(Keys.Right))
                cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateRotationY(0.02f));
            float thumbx = p.ThumbSticks.Left.X;
            if (thumbx!=0)
                cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateRotationY(thumbx * 0.05f));

            if (!IsKeyDown(Keys.LeftControl) && !IsKeyDown(Keys.RightControl))
            {
                // Tilt control
                if (IsKeyDown(Keys.Up))
                    cameraPitch -= 0.01f;
                else if (IsKeyDown(Keys.Down))
                    cameraPitch += 0.01f;
            }
            else
            {
                // Truck forward/backward
                if (IsKeyDown(Keys.Up))
                    cameraDistance = Math.Max(10, cameraDistance - 0.5f);
                else if (IsKeyDown(Keys.Down))
                    cameraDistance += 0.5f;
            }

            float thumby = p.ThumbSticks.Left.Y;
            if (thumby != 0)
                cameraPitch -= thumby * 0.02f;

            cameraPitch = (float)Math.Min(Math.PI / 3, Math.Max(-Math.PI / 3, cameraPitch));

            // Desired direction from which to shoot the target
            Vector3 cameraOffset = Vector3.Transform(cameraDirection, Matrix.CreateFromAxisAngle(Vector3.Cross(Vector3.Up, cameraDirection), cameraPitch));

            // Now Update the camera target

            // Figure out if there's a fixed object the amoeba is resting on
            // that we should target the camera on.  If we can, this lets us keep
            // the camera stable while the amoeba is swirling around it, rather than
            // tracking the centroid of the amoeba, which is jiggling around.
            bool foundFixedTarget = false;
            if (Amoeba.Footing != null && !(Amoeba.Footing is GroundPlane))
            {
                if (fixedTargetPosition == Vector3.Zero)
                    fixedTargetPosition = Amoeba.Footing.Position;
                else
                    fixedTargetPosition = 0.96f * fixedTargetPosition + (1 - 0.96f) * Amoeba.Footing.Position;
                foundFixedTarget = true;
            }
            else if (fixedTargetFraction < 0.01f)
                fixedTargetPosition = Vector3.Zero;

            // Now update the weighting between the fixed target and the
            // camera.  This lets us smoothly move from amoeba to orb
            // on contact, and smoothly move back to the amoeba on dismount.
            if (foundFixedTarget)
                // Found a fixed target, so raise the fixed target weight, causing
                // the camera to pan toward it
                fixedTargetFraction = 1 - (1 - fixedTargetFraction) * 0.94f;
            else
                // No target, so drop the weight, so camera pans away from it
                fixedTargetFraction = 0.95f * fixedTargetFraction;

            // Update current fixation point.
            Vector3 cameraTarget = (1 - fixedTargetFraction) * Amoeba.Position + fixedTargetFraction * fixedTargetPosition;

            // Determine camera location based on fixation point and offset.
            Vector3 cameraLoc = cameraTarget + cameraDistance * cameraOffset;

            // Don't let the camera get too close to the ground plane.
            if (cameraLoc.Y < 0.5f)
            {
                cameraPitch = 0;
                cameraLoc.Y = 0.5f;
            }

            // And finally update the view matrix
            ViewMatrix = Matrix.CreateLookAt(cameraLoc, cameraTarget + (cameraDistance * 0.1f) * Vector3.UnitY, Vector3.Up);
        }

        float messageX;
        float messageY;
        string messageText;
        float messageExpirationTime = 0;

        /// <summary>
        /// Displays a string in the specified location for the specified period of time
        /// </summary>
        public void Display(float x, float y, float expirationTime, string message)
        {
            messageX = x;
            messageY = y;
            messageText = message;
            messageExpirationTime = expirationTime;
        }

        /// <summary>
        /// Redraw the screen.  This also calls the draw methods
        /// of the individual components so they can draw themselves.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            Profiler.Enter("Draw");
            // Make the screen black
            GraphicsDevice.Clear(new Color(0, 0, 0));
            // Reset rendering controls that will have gotten messed up
            // by the SpriteBatch operations on the previous frame.
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.RenderState.DepthBufferEnable = true;

            // Update the bounding frustrum that tracks what objects are in view.
            ViewFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);

            // Draw the GameObjects (e.g. amoeba, orbs)
            foreach (var c in Components)
            {
                GameObject o = c as GameObject;
                if (o != null) // && o.InView)
                    o.Draw(gameTime);
            }

            // Draw everything else (e.g. heads-up displays) on top.
            foreach (var c in Components)
            {
                IDrawable d = c as IDrawable;
                if (d != null && !(d is GameObject))
                    d.Draw(gameTime);
            }

            // Tell the user if they won
            SpriteBatch.Begin();
            if (Amoeba.Footing == EndOrb)
                SpriteBatch.DrawString(Font, (currentLevelNumber<(allLevels.Count-1))?"Level Complete!\n\n\n\nPress TAB to begin next level":"You Win!", new Vector2(GraphicsDevice.Viewport.Width/2-40, GraphicsDevice.Viewport.Height/2+100), Color.White);
            // Display message from script, if any.
            if (messageText != null)
            {
                SpriteBatch.DrawString(Font, messageText, new Vector2(messageX, messageY), Color.White);
                if (messageExpirationTime != 0)
                {
                    messageExpirationTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (messageExpirationTime <= 0)
                        messageText = null;
                }
            }
            //SpriteBatch.DrawString(Font, string.Format("Footing={0}, FTP={1}", Amoeba.Footing, fixedTargetPosition), new Vector2(0, 300), Color.White);
            SpriteBatch.End();
            Profiler.Exit("Draw");
        }
    }
}
