using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HappyFunBlob
{
    class Profiler : DrawableGameComponent
    {
        public Profiler(HappyFunBlobGame g)
            : base(g)
        {
            game = g;
            DrawOrder = 99999;
            LoadContent();
        }

        static bool enabled;

        [Conditional("LIVEPROFILE"), DebuggerStepThrough()]
        static public void EnableProfiling()
        {
            enabled = true;
        }

        [Conditional("LIVEPROFILE"), DebuggerStepThrough()]
        static public void DisableProfiling()
        {
            enabled = false;
        }

        static Profiler()
        {
            topLevel = new ProfiledCodeSection("TopLevel");
            sectionStack.Push(topLevel);
        }

        Texture2D bargraphbar;
        HappyFunBlobGame game;
        const int displayPosition = 100;
        bool displayEnabled = false;
        KeyboardState previous;
        static ProfiledCodeSection topLevel;

        public override void Update(GameTime gameTime)
        {
            KeyboardState k = Keyboard.GetState();

            if (k.IsKeyDown(Keys.R))
                ProfiledCodeSection.ResetAllMaxTimes();

            if (k.IsKeyDown(Keys.P) && previous.IsKeyUp(Keys.P))
                displayEnabled = !displayEnabled;

            previous = k;

            base.Update(gameTime);
        }

        /// <summary>
        /// Calling string.Format allocates a lot of memory, causing more frequent GCs
        /// So when we do formatting to the screen, we use a statically allocated
        /// StringBuilder object.
        /// </summary>
        StringBuilder buffer = new StringBuilder();
        StringBuilder QuickFormat(string format, object arg1, object arg2)
        {
            buffer.Length = 0;
            buffer.AppendFormat(format, arg1, arg2);
            return buffer;
        }

        int displayHeight;

        double lastDrawTime;

        public override void Draw(GameTime gameTime)
        {
            if (displayEnabled)
            {
                game.SpriteBatch.Begin();
                game.SpriteBatch.DrawString(game.Font,
                                            string.Format("{0:###} frames/second, GC count = {1}",
                                                            1 / Math.Max(0.0001, gameTime.TotalGameTime.TotalSeconds - lastDrawTime),
                                                            GC.CollectionCount(0)),
                                            new Vector2(20, 20), Color.White);
                game.SpriteBatch.DrawString(game.Font, "Method", new Vector2(displayPosition, 50), Color.White);
                game.SpriteBatch.DrawString(game.Font, "avg ms (max)", new Vector2(displayPosition + 350, 50), Color.White);
                displayHeight = 70;
                foreach (var s in topLevel.children)
                    DrawSection(s, 0);
                //for (int i = 0; i < ProfiledCodeSection.allSections.Count; i++)
                //{
                //    int y = 20 * i + 70;
                //    ProfiledCodeSection s = ProfiledCodeSection.allSections[i];
                //    game.SpriteBatch.DrawString(game.Font, s.Name, new Vector2(displayPosition, y), Color.White);
                //    game.SpriteBatch.DrawString(game.Font, QuickFormat("{0:0.00}  ({1:0.00})", s.AverageExecutionTime, s.MaxExecutionTime),
                //        new Vector2(displayPosition + 250, y), Color.White);
                //    int reading = (int)(200 * s.LastSample);
                //    game.SpriteBatch.Draw(bargraphbar,
                //        new Rectangle(displayPosition + 400, y + 3, reading, 17),
                //        new Rectangle(0, 0, reading, 1),
                //        Color.White);
                //}
                game.SpriteBatch.End();
            }
            base.Draw(gameTime);
            lastDrawTime = gameTime.TotalGameTime.TotalSeconds;
        }

        void DrawSection(ProfiledCodeSection s, int indentLevel)
        {
            int y = displayHeight;
            game.SpriteBatch.DrawString(game.Font, s.Name, new Vector2(displayPosition+indentLevel, y), Color.White);
            game.SpriteBatch.DrawString(game.Font, QuickFormat("{0:0.00}  ({1:0.00})", s.AverageExecutionTime, s.MaxExecutionTime),
                new Vector2(displayPosition + 350, y), Color.White);
            int reading = (int)(200 * s.LastSample);
            game.SpriteBatch.Draw(bargraphbar,
                new Rectangle(displayPosition + 500, y + 3, reading, 17),
                new Rectangle(0, 0, reading, 1),
                Color.White);
            displayHeight += 20;
            foreach (var child in s.children)
                DrawSection(child, indentLevel+30);
        }

        protected override void LoadContent()
        {
            bargraphbar = game.Content.Load<Texture2D>("bargraphbar");
        }

        static Dictionary<string, ProfiledCodeSection> sections = new Dictionary<string, ProfiledCodeSection>();
        static Stack<ProfiledCodeSection> sectionStack = new Stack<ProfiledCodeSection>();

        [Conditional("LIVEPROFILE"), DebuggerStepThrough()]
        public static void Enter(string name)
        {
            if (!enabled)
                return;

            ProfiledCodeSection s;
            if (!sections.TryGetValue(name, out s))
            {
                s = new ProfiledCodeSection(name);
                sections.Add(name, s);
            }
            s.Start();
            if (s.parent == null)
                s.SetParent(sectionStack.Peek());
            else if (s.parent != sectionStack.Peek() && s.parent != topLevel)
                s.SetParent(topLevel);

            sectionStack.Push(s);
        }

        [Conditional("LIVEPROFILE"), DebuggerStepThrough()]
        public static void Exit(string name)
        {
            if (!enabled)
                return;

            ProfiledCodeSection s = sectionStack.Pop();
            Debug.Assert(s.Name == name, "Profiler exiting a section with a different name than the name last entered.  This probably means there's a missing Profiler.Enter or Profiler.Exit");
            s.End();
        }

        [Conditional("LIVEPROFILE"), DebuggerStepThrough()]
        public static void MaybeExit(string name)
        {
            if (!enabled)
                return;

            if (sectionStack.Peek().Name == name)
            {
                ProfiledCodeSection s = sectionStack.Pop();
                s.End();
            }
        }

        class ProfiledCodeSection
        {
            public ProfiledCodeSection(string name)
            {
                this.name = name;
                allSections.Add(this);
            }

            public static List<ProfiledCodeSection> allSections = new List<ProfiledCodeSection>();

            public string Name
            {
                get { return name; }
            }
            string name;

            Stopwatch timer = new Stopwatch();
            ulong calls = 0;
            TimeSpan currentStartTime;
            int currentGCCount;

            float maxTime = 0;

            public ProfiledCodeSection parent;
            public List<ProfiledCodeSection> children = new List<ProfiledCodeSection>();

            public void SetParent(ProfiledCodeSection p)
            {
                if (parent != null)
                    parent.children.Remove(this);
                parent = p;
                parent.children.Add(this);
            }

            public float LastSample { get; private set; }

            public static void ResetAllMaxTimes()
            {
                foreach (var sec in allSections)
                    sec.ResetMax();
            }

            static int CollectionCount()
            {
                return GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2) + GC.CollectionCount(3);
            }

            public void Start()
            {
#if DEBUG
                if (timer.IsRunning)
                    throw new ProfilingException("Profiled code section " + name + " entered, but not exited.");
#endif
                currentStartTime = timer.Elapsed;
                currentGCCount = CollectionCount();
                timer.Start();
            }

            public void End()
            {
#if DEBUG
                if (!timer.IsRunning)
                    throw new ProfilingException("Profiled code section " + name + " exited without being entered.");
#endif
                timer.Stop();
                calls++;
                LastSample = (float)(timer.Elapsed - currentStartTime).TotalMilliseconds;
                if (LastSample > maxTime && CollectionCount() == currentGCCount)
                    maxTime = LastSample;
            }

            public float AverageExecutionTime
            {
                get
                {
                    return (float)(timer.Elapsed.TotalMilliseconds / calls);
                }
            }

            public float MaxExecutionTime
            {
                get
                {
                    return maxTime;
                }
            }

            public void ResetMax()
            {
                maxTime = 0;
            }
        }
    }

    public class ProfilingException : Exception
    {
        public ProfilingException(string message)
            : base(message)
        { }
    }
}
