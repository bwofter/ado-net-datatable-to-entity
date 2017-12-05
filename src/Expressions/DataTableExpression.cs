namespace BWofter.Converters.Expressions
{
    using System;
    using System.Linq.Expressions;

    public sealed class DataTableExpression : DataExpression
    {
        public override Type Type => dataTableType;
        public Expression DataMember { get; }
        public Expression TableName { get; }
        public Expression TableNamespace { get; }

        internal DataTableExpression(Expression dataMember, Expression tableName = null, Expression tableNamespace = null)
        {
            DataMember = dataMember ?? throw new ArgumentNullException(nameof(dataMember));
            TableName = tableName;
            TableNamespace = tableNamespace;
        }

        public override Expression Reduce()
        {
            if (dataTableType.IsAssignableFrom(DataMember.Type))
            {
                return DataMember;
            }
            else if (dataRowType.IsAssignableFrom(DataMember.Type))
            {
                return Property(DataMember, nameof(System.Data.DataRow.Table));
            }
            else if (dataColumnType.IsAssignableFrom(DataMember.Type))
            {
                return Property(DataMember, nameof(System.Data.DataColumn.Table));
            }
            else if (dataSetType.IsAssignableFrom(DataMember.Type))
            {
                MemberExpression tables = Property(DataMember, "Tables");
                if ((stringType.IsAssignableFrom(TableName.Type) || intType.IsAssignableFrom(TableName.Type)) && TableNamespace == null)
                {
                    return Property(tables, "Item", TableName);
                }
                else if (stringType.IsAssignableFrom(TableName.Type) && stringType.IsAssignableFrom(TableNamespace.Type))
                {
                    return Property(tables, "Item", TableName, TableNamespace);
                }
                else
                {
                    throw new InvalidOperationException($"");
                }
            }
            else
            {
                throw new InvalidOperationException($"");
            }
        }
    }
}
