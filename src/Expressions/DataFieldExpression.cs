using System;
using System.Linq.Expressions;

namespace BWofter.Converters.Expressions
{
    public sealed class DataFieldExpression : DataExpression
    {
        public Expression DataRow { get; }
        public new Expression DataColumn { get; }
        public override Type Type => objectType;
        public DataFieldExpression(Expression dataRow, Expression dataColumn)
        {
            DataRow = dataRow ?? throw new ArgumentNullException(nameof(dataRow));
            DataColumn = dataColumn ?? throw new ArgumentNullException(nameof(dataColumn));
        }
        public override Expression Reduce()
        {
            if (!dataRowType.IsAssignableFrom(DataRow.Type))
            {
                throw new InvalidOperationException($"");
            }
            if (!dataColumnType.IsAssignableFrom(DataColumn.Type))
            {
                throw new InvalidOperationException($"");
            }
            return Property(DataRow, "Item", DataColumn);
        }
    }
}
