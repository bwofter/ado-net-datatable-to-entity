
using System;
using System.Linq.Expressions;

namespace BWofter.Converters.Expressions
{
    public sealed class DataTypeExpression : DataExpression
    {
        public override Type Type => typeType;
        public new Expression DataColumn { get; }

        public DataTypeExpression(Expression dataColumn) =>
            DataColumn = dataColumn ?? throw new ArgumentNullException(nameof(dataColumn));

        public override Expression Reduce()
        {
            if (dataColumnType.IsAssignableFrom(DataColumn.Type))
            {
                return Property(DataColumn, nameof(System.Data.DataColumn.DataType));
            }
            else
            {
                throw new InvalidOperationException($"");
            }
        }
    }
}
