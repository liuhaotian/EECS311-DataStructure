using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HappyFunBlob
{
    /// <summary>
    /// Simple help system
    /// </summary>
    class Help : DrawableGameComponent
    {
        HappyFunBlobGame game;

        bool displayHelp = false;

        public Help(HappyFunBlobGame g)
            : base(g)
        {
            game = g;
            DrawOrder = 9999;
        }

        string helpString;

        KeyboardState previousState;
        public override void Update(GameTime gameTime)
        {
            KeyboardState k = Keyboard.GetState();
            if (k.IsKeyDown(Keys.F1) && previousState.IsKeyUp(Keys.F1))
                displayHelp = !displayHelp;
            previousState = k;

            helpString = null;
            if (k.IsKeyUp(Keys.Space) && (game.Amoeba.Footing == game.Amoeba.GrabTarget
                || (game.Amoeba.GrabTarget != null && (game.Amoeba.GrabTarget.Position - game.Amoeba.Position).LengthSquared() > 50 *50)))
                helpString = "Press TAB to select the next orb to grab";
            else if (game.Amoeba.Footing != null && game.Amoeba.GrabTarget != null)
            {
                if (game.Amoeba.GrabTarget.TouchingAmoeba)
                    helpString = "Release when you think you have a good hold on it";
                else
                    helpString = "Hold spacebar down to grab for orb";
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            game.SpriteBatch.Begin();
            if (gameTime.TotalGameTime.TotalSeconds < 15 && !displayHelp)
                game.SpriteBatch.DrawString(game.Font, "Press F1 key for help", new Vector2(50, 400), Color.White);

            if (displayHelp)
            {
                game.SpriteBatch.DrawString(game.Font, "TAB=lock onto next orb           Space=Grab         Arrows=change viewpoint    Esc=Exit   F1=toggle help", new Vector2(50, 50), Color.White);
                if (helpString != null)
                    game.SpriteBatch.DrawString(game.Font, helpString, new Vector2(50, 400), Color.White);
            }
            game.SpriteBatch.End();
            base.Draw(gameTime);
        }

    }
}
