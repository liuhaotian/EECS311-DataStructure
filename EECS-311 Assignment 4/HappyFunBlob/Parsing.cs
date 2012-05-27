using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// Please note this is *NOT* the way to write a real parser.
/// The right way to do it is to use a program, called a parser generator,
/// to write it for you.  However, it produces incomprehensible code, and
/// this version is probably better for this class for pedagogical reasons.
namespace HappyFunBlob
{
    partial class SyntaxTree
    {
        /// <summary>
        /// Parse a string containing executable code and return its parse tree.
        /// </summary>
        public static SyntaxTree Parse(string text)
        {
            TextReader input = new StringReader(text);
            SyntaxTree t = ParseExpression(input);
            SwallowWhitespace(input);
            if (EndOfInput(input))
                return t;
            else
                throw new SyntaxError("Extra characters after end of expression: '" + input.ReadToEnd() + "'");
        }

        static SyntaxTree ParseExpression(TextReader input)
        {
            return ParseBinaryOperatorExpression(input, ParseSimpleExpression(input), 0);
        }

        #region 'Simple' expressions - no operators or unary (one-argument) operators
        static SyntaxTree ParseSimpleExpression(TextReader input)
        {
            SwallowWhitespace(input);
            if (EndOfInput(input))
                throw new SyntaxError("Premature end of expression");
            char c = (char)input.Peek();
            if (char.IsLetter(c))
                return ParseIdentifierOrMember(input);
            else if (char.IsDigit(c) || c == '.')
                return ParseNumber(input);
            else if (c == '-')
            {
                input.Read();
                return UnaryOperation(Operator.Lookup("negate"), ParseSimpleExpression(input));
            }
            else if (c == '(')
            {
                input.Read();
                SyntaxTree e = ParseExpression(input);
                SwallowWhitespace(input);
                char c2 = (char)input.Read();
                if (c2 != ')')
                    throw new SyntaxError("Expected ')' after parenthesized expression");
                else
                    return e;
            }
            else if (c == '"')
                return ParseString(input);
            else
                throw new SyntaxError("Syntax error beginning at '" + c + "'");
        }

        static SyntaxTree ParseNumber(TextReader input)
        {
            return new Constant(ReadNumber(input));
        }

        static SyntaxTree ParseString(TextReader input)
        {
            // Swallow quotes (quotes are all doubled by excel rather than escaped with \)
            input.Read();
            input.Read();
            List<char> chars = new List<char>();
            while (input.Peek() != (int)'"')
            {
                int c = input.Read();
                if (c < 0)
                    throw new SyntaxError("String missing close quote");
                chars.Add((char)c);
            }
            // Swallow close quotes - again, quotes are doubled.
            input.Read();
            input.Read();

            // Return a constant node with the text of the string.
            return new Constant(new string(chars.ToArray()));
        }

        static SyntaxTree ParseIdentifierOrMember(TextReader input)
        {
            SyntaxTree expression = ParseIdentifier(input);
            while (input.Peek() == '.')
            {
                input.Read();
                expression = ParseMemberExpression(input, expression);
            }

            return expression;
        }

        static SyntaxTree ParseMemberExpression(TextReader input, SyntaxTree expression)
        {
            string member = ReadIdentifier(input);
            if (input.Peek() == '(')
            {
                // Method call
                input.Read();
                SwallowWhitespace(input);

                List<SyntaxTree> args = new List<SyntaxTree>();
                while (input.Peek() != ')')
                {
                    args.Add(ParseExpression(input));

                    SwallowWhitespace(input);
                    int c = input.Peek();

                    if (c < 0)
                        throw new SyntaxError("Method call missing ')'");
                    else if (c == ',')
                        input.Read();
                    else if (c != ')')
                        throw new SyntaxError("Expected ',' at '" + (char)c + input.ReadToEnd() + "'");
                    else
                        SwallowWhitespace(input);
                }
                input.Read();   // Swallow the ')'
                return new MethodCall(expression, member, args.ToArray());
            }
            else
                // Field or property reference
                return new MemberReference(expression, member);
        }

        static SyntaxTree ParseIdentifier(TextReader input)
        {
            string id = ReadIdentifier(input);
            return new VariableReference(id);
        }

        static SyntaxTree UnaryOperation(Operator op, SyntaxTree argumentExpression)
        {
            Constant c = argumentExpression as Constant;
            if (op.Name == "negate" && c != null)
            {
                if (c.Value is float)
                    return new Constant(-(float)c.Value);
                else if (c.Value is int)
                    return new Constant(-(int)c.Value);
                else
                    throw new SyntaxError("Can't apply - to the value " + c.Value);
            }
            else
                return new OperatorExpression(op, argumentExpression);
        }
        #endregion

        #region Expressions containing binary (two-argument) operators
        static SyntaxTree ParseBinaryOperatorExpression(TextReader input, SyntaxTree leftExpression, int minPrecedence)
        {
            SwallowWhitespace(input);
            while (!EndOfInput(input) && LookingAtOperator(input) && NextOperatorPrecedence(input) >= minPrecedence)
            {
                Operator op = ReadOperator(input);
                SyntaxTree rightExpression = ParseSimpleExpression(input);
                int prec;
                SwallowWhitespace(input);
                while (!EndOfInput(input) && LookingAtOperator(input) && (prec = NextOperatorPrecedence(input)) > op.Precedence)
                {
                    rightExpression = ParseBinaryOperatorExpression(input, rightExpression, prec);
                    SwallowWhitespace(input);
                }
                leftExpression = BinaryOperation(op, leftExpression, rightExpression);
            }
            return leftExpression;
        }

        static SyntaxTree BinaryOperation(Operator op, SyntaxTree leftExpression, SyntaxTree rightExpression)
        {
            if (op.Name == "=")
                return AssignmentExpression(leftExpression, rightExpression);
            else
                return new OperatorExpression(op, leftExpression, rightExpression);
        }

        static SyntaxTree AssignmentExpression(SyntaxTree leftExpression, SyntaxTree rightExpression)
        {
            VariableReference v = leftExpression as VariableReference;
            MemberReference m = leftExpression as MemberReference;

            if (v != null)
                return new VariableAssignment(v.VariableName, rightExpression);
            else if (m != null)
                return new MemberAssignment(m.ObjectExpression, m.MemberName, rightExpression);
            else
                throw new SyntaxError("Assignments can only be made to variables and their members");
        }
        #endregion

        #region Operator parsing
        static int NextOperatorPrecedence(TextReader input)
        {
            return Operator.Lookup(PeekOperator(input)).Precedence;
        }

        static bool LookingAtOperator(TextReader input)
        {
            return Operator.IsOperator(PeekOperator(input));
        }

        static string PeekOperator(TextReader input)
        {
            SwallowWhitespace(input);
            return new string((char)input.Peek(), 1);
        }

        static Operator ReadOperator(TextReader input)
        {
            SwallowWhitespace(input);
            return Operator.Lookup(new string((char)input.Read(), 1));
        }
        #endregion

        #region Token processing
        static object ReadNumber(TextReader input)
        {
            SwallowWhitespace(input);
            if (input.Peek() == '-')
            {
                input.Read();
                return ReallyReadNumber(input, -1);
            }
            else
            {
                if (input.Peek() == '+')
                    input.Read();
                return ReallyReadNumber(input, 1);
            }
        }

        static object ReallyReadNumber(TextReader input, int sign)
        {
            string integerToken = ReadToken(input, char.IsDigit);
            if (input.Peek() == '.')
            {
                input.Read();
                return sign * float.Parse(integerToken + "." + ReadToken(input, char.IsDigit));
            }
            else
                return sign * int.Parse(integerToken);
        }

        static string ReadIdentifier(TextReader input)
        {
            return ReadToken(input, char.IsLetterOrDigit);
        }

        static string ReadToken(TextReader input, Predicate<char> acceptable)
        {
            StringBuilder b = new StringBuilder();

            SwallowWhitespace(input);
            while (acceptable((char)input.Peek()))
                b.Append((char)input.Read());
            return b.ToString();
        }

        static void SwallowWhitespace(TextReader input)
        {
            while (char.IsWhiteSpace((char)input.Peek()))
                input.Read();
        }

        static bool EndOfInput(TextReader input)
        {
            return input.Peek() < 0;
        }
        #endregion

        class SyntaxError : Exception
        {
            public SyntaxError()
                : base("Syntax error")
            {
            }

            public SyntaxError(string message)
                : base(message)
            {
            }
        }
    }
}
