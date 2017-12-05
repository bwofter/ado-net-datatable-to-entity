namespace BWofter.Converters.Expressions
{
    using System;
    using System.Linq.Expressions;

    public sealed class TypeIsAssignExpression : DataExpression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        public override Type Type => boolType;
        public Type TargetType { get; }
        public TypeIsAssignExpression(Expression left, Expression right, Type type)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            TargetType = type ?? throw new ArgumentNullException(nameof(type));
        }
        public override Expression Reduce() =>
             NotEqual(Assign(Right, TypeAs(Left, TargetType)), Constant(null, TargetType));
    }
}
