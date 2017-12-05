
namespace BWofter.Converters.Expressions
{
    using System;
    using System.Linq.Expressions;
    public sealed class DataColumnExpression : DataExpression
    {
        public override Type Type => dataColumnType;
        public new Expression DataTable { get; }
        public Expression ColumnName { get; }

        internal DataColumnExpression(Expression dataTable, Expression columnName)
        {
            DataTable = dataTable ?? throw new ArgumentNullException(nameof(dataTable));
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
        }

        public override Expression Reduce()
        {
            if (!dataTableType.IsAssignableFrom(DataTable.Type))
            {
                throw new InvalidOperationException($"{nameof(DataTable)} should return a value that inherits from {dataTableType.FullName}, {DataTable.Type.FullName} given instead.");
            }
            if (!intType.IsAssignableFrom(ColumnName.Type) && !stringType.IsAssignableFrom(ColumnName.Type))
            {
                throw new InvalidOperationException($"{nameof(ColumnName)} should return a value that is either {intType.FullName} or {stringType.FullName}, {ColumnName.Type.FullName} given instead.");
            }
            return Property(Property(DataTable, nameof(System.Data.DataTable.Columns)), "Item", ColumnName);
        }
    }
}
