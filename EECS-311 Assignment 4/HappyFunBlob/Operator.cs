using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyFunBlob
{

    public class Operator
    {
        static Operator()
        {
            DefineOperator("=", 0);
            DefineOperator("+", 10);
            DefineOperator("-", 10);
            DefineOperator("*", 11);
            DefineOperator("/", 11);
            DefineOperator("negate", 50);
        }

        public string Name { get; private set; }
        public int Precedence { get; private set; }

        Operator(string token, int precedence)
        {
            Name = token;
            Precedence = precedence;
        }

        static Dictionary<string, Operator> operators = new Dictionary<string, Operator>();

        public static void DefineOperator(string token, int precedence)
        {
            operators[token] = new Operator(token, precedence);
        }

        public static Operator Lookup(string token)
        {
            return operators[token];
        }

        public static bool IsOperator(string token)
        {
            return operators.ContainsKey(token);
        }
    }
}
