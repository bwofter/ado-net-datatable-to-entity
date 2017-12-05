namespace BWofter.Converters.Expressions
{
    using System;
    using System.Linq.Expressions;
    public sealed class DataFieldIsNullExpression : DataExpression
    {
        public Expression DataRow { get; }
        public new Expression DataColumn { get; }
        public override Type Type => boolType;
        public DataFieldIsNullExpression(Expression dataRow, Expression dataColumn)
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
            return Call(DataRow, nameof(System.Data.DataRow.IsNull), Type.EmptyTypes, DataColumn);
        }
    }
}
