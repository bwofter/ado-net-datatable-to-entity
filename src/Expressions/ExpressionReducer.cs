namespace BWofter.Converters.Expressions
{
    using System.Linq.Expressions;
    public sealed class ExpressionReducer : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node != null)
                while (node.CanReduce)
                    node = node.Reduce();
            return base.Visit(node);
        }
    }
}
