using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HappyFunBlob
{
    /// <summary>
    /// Displays frame rate on screen for debugging purposes
    /// Adapted from code on Shawn Hargreaves' blog.
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        HappyFunBlobGame game;

        /// <summary>
        /// Displays frame rate on screen for debugging purposes
        /// Adapted from code on Shawn Hargreaves' blog.
        /// </summary>
        public FrameRateCounter(HappyFunBlobGame game)
            : base(game)
        {
            this.game = game;
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            game.SpriteBatch.Begin();

            game.SpriteBatch.DrawString(game.Font, fps, new Vector2(33, 33), Color.Black);
            game.SpriteBatch.DrawString(game.Font, fps, new Vector2(32, 32), Color.White);
            
            game.SpriteBatch.End();
        }
    }
}
