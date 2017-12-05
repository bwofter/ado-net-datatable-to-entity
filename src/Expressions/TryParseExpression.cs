namespace BWofter.Converters.Expressions
{
    using System;
    using System.Linq.Expressions;

    public sealed class TryParseExpression : DataExpression
    {
        public Expression InParameter { get; }
        public Expression OutParameter { get; }
        public override Type Type => boolType;
        public Type TargetType { get; }
        public TryParseExpression(Expression inParameter, Expression outParameter, Type type)
        {
            InParameter = inParameter ?? throw new ArgumentNullException(nameof(inParameter));
            OutParameter = outParameter ?? throw new ArgumentNullException(nameof(outParameter));
            TargetType = type ?? throw new ArgumentNullException(nameof(type));
        }
        public override Expression Reduce() =>
            Call(TargetType.GetMethod("TryParse", new[] { stringType, TargetType.MakeByRefType() }), InParameter, OutParameter);
    }
}
