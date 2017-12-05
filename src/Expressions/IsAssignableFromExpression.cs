using System;
using System.Linq.Expressions;

namespace BWofter.Converters.Expressions
{
    public sealed class IsAssignableFromExpression : DataExpression
    {
        public override Type Type => boolType;
        public Expression TargetType { get; }
        public Expression OtherType { get; }
        internal IsAssignableFromExpression(Expression target, Expression other)
        {
            TargetType = target ?? throw new ArgumentNullException(nameof(target));
            OtherType = other ?? throw new ArgumentNullException(nameof(other));
        }
        public IsAssignableFromExpression Update(Expression target, Expression other) =>
            new IsAssignableFromExpression(target, other);
        public override Expression Reduce()
        {
            Expression target,
                other;
            if (typeType.IsAssignableFrom(TargetType.Type) || typeInfoType.IsAssignableFrom(TargetType.Type))
            {
                target = TargetType;
            }
            else
            {
                target = Call(TargetType, nameof(object.GetType), Type.EmptyTypes);
            }
            if (typeType.IsAssignableFrom(OtherType.Type) || typeInfoType.IsAssignableFrom(OtherType.Type))
            {
                other = OtherType;
            }
            else
            {
                other = Call(OtherType, nameof(object.GetType), Type.EmptyTypes);
            }
            return Call(TargetType, nameof(Type.IsAssignableFrom), Type.EmptyTypes, other);
        }
    }
}
