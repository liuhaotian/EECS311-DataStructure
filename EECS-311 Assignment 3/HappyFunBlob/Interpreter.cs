using System;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace HappyFunBlob
{
    /// <summary>
    /// Contains procedures for implementing scripting language
    /// </summary>
    public static class Interpreter
    {
        /// <summary>
        /// Changes the value of the specified field or property.
        /// </summary>
        /// <param name="obj">object to change the value of the field of</param>
        /// <param name="memberName">name of the field or property to change the value of</param>
        /// <param name="newValue">new value to assign to that field</param>
        public static object SetMemberValue(this object obj, string memberName, object newValue)
        {
            Type type = obj.GetType();
            EventInfo e = type.GetEvent(memberName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (e == null)
                type.InvokeMember(memberName,
                                   BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Instance
                                        | BindingFlags.Public | BindingFlags.NonPublic,
                                   null, obj, new object[] { newValue });
            else
                e.AddEventHandler(obj, (System.Delegate)newValue);

            return newValue;
        }

        /// <summary>
        /// Returns the value of the specified field or property
        /// </summary>
        /// <param name="obj">object to read the field value from</param>
        /// <param name="memberName">name of the field or property to get the value of</param>
        /// <returns></returns>
        public static object GetMemberValue(this object obj, string memberName)
        {
            if (obj is Type)
                return ((Type)obj).InvokeMember(memberName,
                                                  BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.Static
                                                        | BindingFlags.Public | BindingFlags.NonPublic,
                                                  null, obj, null);
            else
                return obj.GetType().InvokeMember(memberName,
                                              BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.Instance
                                                    | BindingFlags.Public | BindingFlags.NonPublic,
                                              null, obj, null);
        }

        /// <summary>
        /// Calls the specified method with the specified arguments
        /// </summary>
        /// <param name="obj">object on which to call the method</param>
        /// <param name="methodName">name of the method to call</param>
        /// <param name="args">arguments to pass to the method</param>
        /// <returns></returns>
        public static object CallMethod(this object obj, string methodName, params object[] args)
        {
            return obj.GetType().InvokeMember(methodName,
                                              BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.Instance
                                                    | BindingFlags.Public | BindingFlags.NonPublic,
                                              null, obj, args);
        }

        public static GameObject.GameEventHandler MakeHandler(string code)
        {
            SyntaxTree parsed = SyntaxTree.Parse(code);
            return delegate()
            {
                parsed.Run(Environment);
            };
        }

        /// <summary>
        /// The dictionary holding the values of all the variables in the scripting language
        /// </summary>
        public static ListDictionary Environment { get; set; }

        /// <summary>
        /// Performs a numeric operation such as + or * that can be performed on different types
        /// of objects (ints, floats, Vectors), given the arguments and the name of the operation,
        /// e.g. "+"
        /// </summary>
        /// <param name="name">String giving the name of the operator, such as "+"</param>
        /// <param name="arguments">arguments to apply operation to</param>
        /// <returns></returns>
        public static object GenericOperator(string name, object[] arguments)
        {
            // This is ugly code because it has to do a lot of case analysis.
            // It also uses a bunch of stuff called reflection that's outside
            // the scope of the class.

            // Start by turning all scalars (numbers) into floats
            // unless all the arguments happen to be ints.  This
            // simplifies the case analysis we have to and the
            // loss of double precision doesn't hurt us in this
            // application
            bool convertToFloat = false;
            foreach (var a in arguments)
                convertToFloat |= (a is float) | (a is double) | (a is Vector3) | (a is Matrix) | (a is Vector2);
            if (convertToFloat)
                for (int i = 0; i < arguments.Length; i++)
                {
                    object a = arguments[i];
                    if (a is int | a is double | a is float)
                        arguments[i] = Convert.ToSingle(a);
                }
            // Now we either have all ints, all floats, or a mix of vectors and floats

            // Special case unary negation, e.g. -a   (as opposed to a-b)
            if ((name == "-" && arguments.Length == 1) || name=="negate")
            {
                object o = arguments[0];
                if (o is int)
                    return -(int)o;
                else if (o is float)
                    return -(float)o;
                else
                    return o.GetType().InvokeMember("op_UnaryNegation",
                                                      BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.Static
                                                            | BindingFlags.Public | BindingFlags.NonPublic,
                                                      null, null, arguments);
            }
            // Anything else should have two arguments
            else if (arguments.Length == 2)
            {
                // Handle the comparison operators
                switch (name)
                {
                    // For equality, we use the built-in equality operator on the Object class
                    case "==":
                        return arguments[0].Equals(arguments[1]);

                    case "!=":
                        return !arguments[0].Equals(arguments[1]);

                    // For < and > we convert to double-precisions and then compare.
                    case "<":
                        return Convert.ToSingle(arguments[0]) < Convert.ToSingle(arguments[1]);

                    case "<=":
                        return Convert.ToSingle(arguments[0]) <= Convert.ToSingle(arguments[1]);

                    case ">":
                        return Convert.ToSingle(arguments[0]) > Convert.ToSingle(arguments[1]);

                    case ">=":
                        return Convert.ToSingle(arguments[0]) >= Convert.ToSingle(arguments[1]);

                    // If we get here, it's +, -, *, /
                    // Do case analysis first on the operator, and then on the argument types
                    default:
                        string methodName;
                        switch (name)
                        {
                            case "+":
                                // We only have three cases: all ints, all floats, or mixed float/vector/matrix
                                if (arguments[0] is int)
                                    return ((int)arguments[0]) + ((int)arguments[1]);
                                else if (arguments[0] is float && arguments[1] is float)
                                    return ((float)arguments[0]) + ((float)arguments[1]);
                                else
                                    // Punt and fall through to the call out to reflection, below.
                                    methodName = "op_Addition";
                                break;

                            case "-":
                                if (arguments[0] is int)
                                    return ((int)arguments[0]) - ((int)arguments[1]);
                                else if (arguments[0] is float && arguments[1] is float)
                                    return ((float)arguments[0]) - ((float)arguments[1]);
                                else
                                    methodName = "op_Subtraction";
                                break;

                            case "*":
                                if (arguments[0] is int)
                                    return ((int)arguments[0]) * ((int)arguments[1]);
                                else if (arguments[0] is float && arguments[1] is float)
                                    return ((float)arguments[0]) * ((float)arguments[1]);
                                else
                                    methodName = "op_Multiply";
                                break;

                            case "/":
                                if (arguments[0] is int)
                                    return ((int)arguments[0]) / ((int)arguments[1]);
                                else if (arguments[0] is float && arguments[1] is float)
                                    return ((float)arguments[0]) / ((float)arguments[1]);
                                else
                                    methodName = "op_Division";
                                break;

                            default:
                                throw new ArgumentException("Unknown arithmetic operator " + name);
                        }

                        // If we got this far, it's arithmetic on vectors and possibly a float.
                        if (arguments[0] is float && !(arguments[1] is float))
                        {
                            // We're doing scalar multiplication, so make sure
                            // the scalar is the second argument.  It's already been
                            // converted into a float.
                            object t = arguments[0];
                            arguments[0] = arguments[1];
                            arguments[1] = t;
                        }
                        // Call out to System.Reflection to invoke the method by name.
                        return arguments[0].GetType().InvokeMember(methodName,
                                                                      BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.Static
                                                                            | BindingFlags.Public | BindingFlags.NonPublic,
                                                                      null, null, arguments);
                }
            }
            else
                throw new ArgumentException("Wrong number of arguments to arithmetic operator " + name);
        }
    }
}
