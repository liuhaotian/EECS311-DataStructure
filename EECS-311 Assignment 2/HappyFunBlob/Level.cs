using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace HappyFunBlob
{
    /// <summary>
    /// Represents a level of the game
    /// Contains a series of "items", each of which has the information needed to create
    /// an Obstacle (e.g. Orb).  Calling Install() removes all current Obstacles from
    /// game and replaces them with the Obstacles specified in this level.
    /// </summary>
    public class Level
    {
        Item[] items;

        /// <summary>
        /// Called when the level is started.
        /// </summary>
        public event GameObject.GameEventHandler StartLevel;

        /// <summary>
        /// Creates a level consisting of hte specified items.
        /// </summary>
        Level(params Item[] items)
        {
            this.items = items;
        }

        public static IList<Level> ReadLevels()
        {
            List<Level> levels = new List<Level>();

            for (short i = 0; File.Exists(LevelFileName(i)); i++)
                levels.Add(ReadLevelFile(i));

            return levels;
        }

        public static Level ReadLevelFile(short levelNumber)
        {
            string[] lines = System.IO.File.ReadAllLines(LevelFileName(levelNumber));
            List<Item> items = new List<Item>();
            short lineNumber = 0;

            foreach (var line in lines) {
                string[] fields = line.Split(new char[] {'\t'}, StringSplitOptions.None);

                lineNumber++;

                if (fields[0].StartsWith("#"))
                    // This is a header
                    continue;

                int lastElt;
                for (lastElt = fields.Length - 1; lastElt >= 0 && string.IsNullOrEmpty(fields[lastElt]); lastElt--) ;

                if (lastElt < 5)
                    throw new LevelFileException(levelNumber, lineNumber, (short)(lastElt + 1), "Object is missing fields; must include at least type and position");
                // Remove quotation marks around fields, if any (these are often added by excel)
                for (int i = 0; i <= lastElt; i++)
                    fields[i] = fields[i].Trim('\"');
                string[] args = new string[lastElt + 1 - 6];
                for (int i = 0; i < args.Length; i++)
                    args[i] = fields[i + 6];

                if (string.IsNullOrEmpty(fields[0]) && string.IsNullOrEmpty(fields[1]) && string.IsNullOrEmpty(fields[2]) &&
                    string.IsNullOrEmpty(fields[3]) && string.IsNullOrEmpty(fields[4]) && string.IsNullOrEmpty(fields[5]))
                {
                    // It's a continuation line
                    if (items.Count == 0)
                        throw new LevelFileException(levelNumber, lineNumber, 1, "Level file appears to start with a continuation line. There must be an item description above this line.");
                    else
                        items[items.Count - 1].AddOptions(lineNumber, args);
                }
                else
                {
                    // It's a new item
                    Type type = Type.GetType("HappyFunBlob." + fields[1]);
                    if (type == null || (type != typeof(Amoeba) && type != typeof(HappyFunBlobGame) && !type.IsSubclassOf(typeof(Obstacle))))
                        throw new LevelFileException(levelNumber, lineNumber, 2, "Unknown item type " + fields[1]);
                    Vector3 position = new Vector3(float.Parse(fields[3]), float.Parse(fields[4]), float.Parse(fields[5]));
                    fields[2] = fields[2].Trim();
                    if (!String.IsNullOrEmpty(fields[2]))
                    {
                        Item r = items.Find(i => (i.Name == fields[2]));
                        if (r == null)
                            throw new LevelFileException(levelNumber, lineNumber, 3, "Can't find item named " + fields[2]);
                        else
                            position += r.Position;
                    }
                    items.Add(new Item(levelNumber, fields[0], type, position, lineNumber, args));
                }
            }

            Item start = items.Find(i => (i.Name == "start" || i.Name=="Start"));
            if (start==null)
                throw new LevelFileException(levelNumber, "Level contains no start Orb");
            else if (start.Type.IsSubclassOf(typeof(Orb)))
                throw new LevelFileException(levelNumber, start.lineNumber, 2, "Level start is not a Orb");

            Item end = items.Find(i => (i.Name == "end" || i.Name == "End"));
            if (end == null)
                throw new LevelFileException(levelNumber, "Level contains no end Orb");
            else if (end.Type.IsSubclassOf(typeof(Orb)))
                throw new LevelFileException(levelNumber, end.lineNumber, 2, "Level end is not a Orb");

            return new Level(items.ToArray());
        }

        static object Parse(string s)
        {
            object o = ParseFloat(s);
            if (o != null)
                return o;
            o = ParseBool(s);
            if (o != null)
                return o;
            o = ParseVector(s);
            if (o != null)
                return o;
            else
                return s;
        }

        static float? ParseFloat(string s)
        {
            float? result;
            try
            {
                result = float.Parse(s);
            }
            catch (System.FormatException)
            {
                result = null;
            }

            return result;
        }

        static bool? ParseBool(string s)
        {
            switch (s) {
                case "true":
                    return true;

                case "false":
                    return false;

                default:
                    return null;
            }
        }

        static Vector3? ParseVector(string s)
        {
            if (s.StartsWith("(") && s.EndsWith(")"))
            {
                string[] components = s.Substring(1, s.Length - 2).Split(' ', ',');
                if (components.Length == 3)
                {
                    float? x = ParseFloat(components[0]);
                    float? y = ParseFloat(components[1]);
                    float? z = ParseFloat(components[2]);

                    if (x != null && y != null && z != null)
                        return new Vector3(x.Value, y.Value, z.Value);
                }
            }
            return null;
        }

        static string LevelFileName(int levelNumber)
        {
            return string.Format("Content/Level{0}.txt", levelNumber);
        }

        /// <summary>
        /// Replace the game's current items with the items from this level.
        /// </summary>
        public void Install(HappyFunBlobGame game)
        {
            //Profiler.Enter("Level.Install");
            List<GameObject> levelObjects = new List<GameObject>();
            RemovePreviousLevel(game);
            // Make a new environment for the scripting code
            Interpreter.Environment = new ListDictionary();
            Interpreter.Environment.Store("amoeba", game.Amoeba);
            Interpreter.Environment.Store("game", game);
            Interpreter.Environment.Store("color", typeof(Microsoft.Xna.Framework.Color));
            //Profiler.Enter("Create objects");
            foreach (var item in items)
            {
                IGameComponent i = item.Create(game);
                Orb o = i as Orb;
                if (i != null)
                {
                    game.Components.Add(i);
                    //if (i is Obstacle && ((Obstacle)i).OscillationAmplitude==0)
                        levelObjects.Add((GameObject)i);
                }
                if (item.Name == "start" || item.Name == "Start")
                {
                    Debug.Assert(o != null, "Start of level must be an Orb"); 
                    game.StartOrb = o;
                }
                else if (item.Name == "end" || item.Name == "End")
                {
                    Debug.Assert(o != null, "End of level must be an Orb");
                    game.EndOrb = o;
                }
            }

            // Adding the ground plane makes it to hard to find good splits
            levelObjects.Add(game.GroundPlane);
            //Profiler.Exit("Create objects");
#if BSPTrees
            game.GameObjectTree = BSPTree.BuildTree(levelObjects);
#endif

            // Run any even handlers for the start of the level
            if (StartLevel != null)
                StartLevel();

            game.fixedTargetPosition = game.StartOrb.Position;
            //Profiler.Exit("Level.Install");
        }

        /// <summary>
        /// Remove the game's current components.
        /// </summary>
        static void RemovePreviousLevel(HappyFunBlobGame g)
        {
            //Profiler.Enter("Level.RemovePreviousLevel");
            for (int i = g.Components.Count - 1; i >= 0; i--)
            {
                IGameComponent c = g.Components[i];
                if (c is Obstacle && !(c is GroundPlane))
                {
                    g.Components.RemoveAt(i);
                    IDisposable d = c as IDisposable;
                    if (d != null)
                    {
                        d.Dispose();
                    }
                }
            }
            //Profiler.Exit("Level.RemovePreviousLevel");
        }

        class Item
        {
            public Type Type { get; private set; }
            public Vector3 Position { get; private set; }
            List<Option> Options { get; set; }
            public string Name { get; private set; }
            public short lineNumber;
            public short levelNumber;

            public Item(short level, string name, Type type, Vector3 position, short lineNumber, params string[] optionArgs)
            {
                this.levelNumber = level;
                this.Name = name;
                this.lineNumber = lineNumber;
                Type = type;
                Position = position;
                Options = new List<Option>();
                AddOptions(lineNumber, optionArgs);
            }

            public void AddOptions(short lineNumber, string[] optionArgs)
            {
                if (optionArgs.Length % 2 != 0)
                    throw new LevelFileException(levelNumber, lineNumber, (short)(6 + optionArgs.Length), "No value specified for field " + optionArgs[optionArgs.Length - 1]);
                int argpos = 0;
                for (short i = 0; i < optionArgs.Length / 2; i++)
                    Options.Add(new Option(optionArgs[argpos++], Parse(optionArgs[argpos++]), lineNumber, (short)(i + 7)));
                Validate();
            }

            public Obstacle Create(HappyFunBlobGame game)
            {
                switch (Type.Name)
                {
                    case "Amoeba":
                        game.Amoeba.ResetPosition(Position);
                        foreach (var o in Options)
                            // Use the reflection system to reach in and change fields by Name
                            Type.InvokeMember(o.name,
                                BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public,
                                null, game.Amoeba, new object[] { o.value });
                        return null;

                    case "HappyFunBlobGame":
                        foreach (var o in Options)
                            // Use the reflection system to reach in and change fields by Name
                            Type.InvokeMember(o.name,
                                BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public,
                                null, game, new object[] { o.value });
                        return null;

                    case "Level":
                        foreach (var o in Options)
                            // Use the reflection system to reach in and change fields by Name
                            Type.InvokeMember(o.name,
                                BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public,
                                null, this, new object[] { o.value });
                        return null;

                    default:
                        return (Obstacle)CreateInstance(Type, game, Position,
                            new Obstacle.Initializer(delegate(Obstacle component)
                                {
                                    foreach (var o in Options)
                                        component.SetMemberValue(o.name, o.value);
                                    // Add to dictionary if named
                                    if (!string.IsNullOrEmpty(Name))
                                        Interpreter.Environment.Store(Name, component);
                                }));
                }
            }

            object CreateInstance(Type type, params object[] args)
            {
                //Profiler.Enter("CreateInstance");
                object result = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, args);
                //Profiler.Exit("CreateInstance");
                return result;
            }

            void Validate()
            {
                foreach (var o in Options)
                {
                    MemberInfo[] mems = Type.GetMember(o.name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (mems == null || mems.Length == 0)
                        throw new LevelFileException(levelNumber, o.lineNumber, o.columnNumber, "Unknown field name: " + o.name);
                    else if (mems.Length > 1)
                        throw new LevelFileException(levelNumber, o.lineNumber, o.columnNumber, "Ambiguous field name:" + o.name);

                    MemberInfo m = mems[0];
                    if (m is FieldInfo)
                    {
                        FieldInfo f = (FieldInfo)m;
                        if (!f.FieldType.IsInstanceOfType(o.value))
                            throw new LevelFileException(levelNumber, o.lineNumber, (short)(o.columnNumber + 1), string.Format("Field {0} is wrong type; should be {1}.", o.name, TypeName(f.FieldType)));
                    }
                    else if (m is PropertyInfo)
                    {
                        PropertyInfo p = (PropertyInfo)m;
                        if (!p.PropertyType.IsInstanceOfType(o.value))
                            throw new LevelFileException(levelNumber, o.lineNumber, (short)(o.columnNumber + 1), string.Format("Property {0} is wrong type; should be {1}.", o.name, TypeName(p.PropertyType)));
                    }
                    else if (m is EventInfo)
                    {
                        EventInfo e = (EventInfo)m;
                        if (!(o.value is string))
                        {
                            if (!(o.value is Delegate))
                                throw new LevelFileException(levelNumber, o.lineNumber, o.columnNumber, "Handler for event " + o.name + " should be executable code.");
                        }
                        else
                            o.value = Interpreter.MakeHandler((string)o.value);
                    }
                    else
                        throw new LevelFileException(levelNumber, o.lineNumber, o.columnNumber, "Unknown field name: " + o.name);
                }
            }

            static string TypeName(Type t)
            {
                if (t == typeof(float))
                    return "number";
                else
                    return t.Name;
            }

            class Option
            {
                public string name;
                public object value;
                public short lineNumber;
                public short columnNumber;

                public Option(string name, object value, short line, short column) {
                    this.name = name;
                    this.value = value;
                    lineNumber = line;
                    columnNumber = column;
                }
            }
        }
    }

    public class LevelFileException : Exception
    {
        static string CellNameFromLineAndColumn(short line, short column)
        {
            return string.Format("{0}{1}", (char)('A' + column - 1), line);
        }

        public LevelFileException(int level, short line, short column, string message)
            : base("Level "+level+", cell "+CellNameFromLineAndColumn(line, column)+": "+message)
        { }

        public LevelFileException(int level, string message)
            : base("Level " + level + ": " + message)
        { }

    }
}
