using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyFunBlob
{
    public abstract partial class SyntaxTree
    {
        public SyntaxTree[] Children { get; protected set; }
        public abstract string Label { get; }

        public virtual void WriteScheme(StringBuilder b)
        {
            b.Append("(");
            b.Append(Label);
            foreach (var child in Children)
            {
                b.Append(" ");
                child.WriteScheme(b);
            }
            b.Append(")");
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            WriteScheme(b);
            return b.ToString();
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public abstract object Run(Dictionary dict);
    }

    /// <summary>
    /// A syntax tree with no children, e.g. a constant or variable reference.
    /// </summary>
    abstract class SyntaxTreeLeaf : SyntaxTree
    {
        static SyntaxTree[] noChildren = new SyntaxTree[0];
        protected SyntaxTreeLeaf()
        {
            Children = noChildren;
        }

        public override void WriteScheme(StringBuilder b)
        {
            b.Append(Label);
        }
    }

    class Constant : SyntaxTreeLeaf
    {
        public object Value { get; private set; }
        public Constant(object constantValue)
            : base()
        {
            Value = constantValue;
        }

        public override string Label
        {
            get { return Value.ToString(); }
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public override object Run(Dictionary dict)
        {
            //throw new NotImplementedException();
            return Value;
        }
    }

    class VariableReference : SyntaxTreeLeaf
    {
        public string VariableName { get; private set; }

        public VariableReference(string variableName)
            : base()
        {
            VariableName = variableName;
        }

        public override string Label
        {
            get { return VariableName; }
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public override object Run(Dictionary dict)
        {
            //throw new NotImplementedException();
            return dict.Lookup(VariableName);
        }
    }

    class VariableAssignment : SyntaxTreeLeaf
    {
        public string VariableName { get; private set; }
        public SyntaxTree ValueExpression { get; private set; }

        public VariableAssignment(string variableName, SyntaxTree value)
            : base()
        {
            VariableName = variableName;
            ValueExpression = value;
            Children = new SyntaxTree[] { value };
        }

        public override void WriteScheme(StringBuilder b)
        {
            b.Append("(set! ");
            b.Append(VariableName);
            b.Append(" ");
            ValueExpression.WriteScheme(b);
            b.Append(")");
        }

        public override string Label
        {
            get { return "set!"; }
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public override object Run(Dictionary dict)
        {
            //throw new NotImplementedException();
            dict.Store(VariableName, ValueExpression.Run(dict));
            return ValueExpression.Run(dict);
        }
    }

    class MemberReference : SyntaxTree
    {
        public SyntaxTree ObjectExpression { get; private set; }
        public string MemberName { get; private set; }

        public MemberReference(SyntaxTree oExpression, string member)
        {
            ObjectExpression = oExpression;
            MemberName = member;
            Children = new SyntaxTree[] { oExpression };
        }

        public override void WriteScheme(StringBuilder b)
        {
            ObjectExpression.WriteScheme(b);
            b.Append(".");
            b.Append(MemberName);
        }

        public override string Label
        {
            get { return "member"; }
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public override object Run(Dictionary dict)
        {
            //throw new NotImplementedException();
            return ObjectExpression.Run(dict).GetMemberValue(MemberName);
        }
    }

    class MemberAssignment : SyntaxTree
    {
        public SyntaxTree ObjectExpression { get; private set; }
        public string MemberName { get; private set; }
        public SyntaxTree ValueExpression { get; private set; }

        public MemberAssignment(SyntaxTree oExpression, string member, SyntaxTree value)
        {
            ObjectExpression = oExpression;
            MemberName = member;
            ValueExpression = value;
            Children = new SyntaxTree[] { oExpression, value };
        }

        public override void WriteScheme(StringBuilder b)
        {
            b.Append("(set-member! ");
            ObjectExpression.WriteScheme(b);
            b.Append(".");
            b.Append(MemberName);
            b.Append(" ");
            ValueExpression.WriteScheme(b);
            b.Append(")");
        }

        public override string Label
        {
            get { return "member"; }
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public override object Run(Dictionary dict)
        {
            //throw new NotImplementedException();
            ObjectExpression.Run(dict).SetMemberValue(MemberName, ValueExpression.Run(dict));
            return ValueExpression.Run(dict);
        }
    }

    class MethodCall : SyntaxTree
    {
        public SyntaxTree ObjectExpression { get; private set; }
        public string MethodName { get; private set; }
        public SyntaxTree[] Arguments { get; private set; }

        public MethodCall(SyntaxTree oExpression, string method, params SyntaxTree[] args)
        {
            ObjectExpression = oExpression;
            MethodName = method;
            Arguments = args;
            SyntaxTree[] children = new SyntaxTree[args.Length + 1];
            children[0] = oExpression;
            args.CopyTo(children, 1);
            Children = children;
        }

        public override void WriteScheme(StringBuilder b)
        {
            b.Append("(");
            ObjectExpression.WriteScheme(b);
            b.Append(".");
            b.Append(MethodName);
            foreach (var arg in Arguments)
            {
                b.Append(" ");
                arg.WriteScheme(b);
            }
            b.Append(")");

        }

        public override string Label
        {
            get { return "call"; }
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public override object Run(Dictionary dict)
        {
            //throw new NotImplementedException();
            object[] valveArguments = new object[Arguments.Length];
            int index = 0;
            foreach (var arg in Arguments)
            {
                valveArguments[index] = arg.Run(dict);
                index++;
            }
            return ObjectExpression.Run(dict).CallMethod(MethodName, valveArguments);
        }
    }

    class OperatorExpression : SyntaxTree
    {
        /// <summary>
        /// The operator, but this can't be called "operator" because that's a C# keyword.
        /// </summary>
        Operator operation;

        public OperatorExpression(Operator op, params SyntaxTree[] args)
        {
            operation = op;
            Children = args;
        }

        public override string Label
        {
            get { return operation.Name; }
        }

        /// <summary>
        /// Runs the expression and returns its value
        /// </summary>
        public override object Run(Dictionary dict)
        {
            object[] valveChildren = new object[Children.Length];
            int index = 0;
            foreach (var arg in Children)
            {
                valveChildren[index] = arg.Run(dict);
                index++;
            }
            //throw new NotImplementedException();
            return Interpreter.GenericOperator(Label, valveChildren);
        }
    }
}
