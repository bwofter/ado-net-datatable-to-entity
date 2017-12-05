namespace BWofter.Converters.Expressions
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public sealed class ChangeTypeExpression : DataExpression
    {
        public Expression Operand { get; }
        public override Type Type { get; }
        private static readonly MethodInfo changeType = convertType.GetMethod(nameof(System.Convert.ChangeType), new[] { objectType, typeType });
        public ChangeTypeExpression(Expression operand, Type type)
        {
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
            Type = type ?? throw new ArgumentNullException(nameof(operand));
        }
        public override Expression Reduce()
        {
            if (convertType.GetMethod($"To{Type.Name}", new[] { Operand.Type }) is MethodInfo methodInfo)
            {
                return Call(methodInfo, Operand);
            }
            else
            {
                return Convert(Call(changeType, Operand, Constant(Type)), Type);
            }
        }
    }
}
