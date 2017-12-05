namespace BWofter.Converters.Data
{
    using BWofter.Converters.Expressions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    /// <summary><para>A static class used to convert data tables into entities.</para></summary>
    /// <typeparam name="TEntity"><para>The entity type to convert to.</para></typeparam>
    public static class DataTableConverter<TEntity> where TEntity : class, new()
    {
        private static readonly Type type = typeof(TEntity);
        private static readonly ConcurrentDictionary<ICollection<string>, Func<DataRow, TEntity>> converters =
            new ConcurrentDictionary<ICollection<string>, Func<DataRow, TEntity>>(StringCollectionHelper.GetInstance());
        /// <summary><para>Iterates over the <see cref="DataRow"/> values in the <paramref name="dataTable"/>, mapping their fields
        /// to the entity type.</para></summary>
        /// <param name="dataTable"><para>The <see cref="DataTable"/> to map to the entity type.</para></param>
        /// <returns><para>A yielded instance of the entity type.</para></returns>
        public static IEnumerable<TEntity> ToEntities(DataTable dataTable) =>
            ToEntities(dataTable, new Dictionary<DataColumn, string>(dataTable.Columns.Cast<DataColumn>().Select(GetDataColumnKeyValuePair)));
        /// <summary><para>Iterates over the <see cref="DataRow"/> values in the <paramref name="dataTable"/>, mapping their fields
        /// to the entity type.</para></summary>
        /// <param name="dataTable"><para>The <see cref="DataTable"/> to map to the entity type.</para></param>
        /// <param name="columnToMemberMap"><para>The <see cref="Dictionary{TKey, TValue}"/> to map the <see cref="DataColumn"/> values to properties.</para></param>
        /// <returns><para>A yielded instance of the entity type.</para></returns>
        public static IEnumerable<TEntity> ToEntities(DataTable dataTable, IDictionary<DataColumn, string> columnToMemberMap)
        {
            Func<DataRow, TEntity> converter = GetConverter(columnToMemberMap);
            foreach (DataRow row in dataTable.Rows)
            {
                yield return converter(row);
            }
        }

        private static Func<DataRow, TEntity> GetConverter(IDictionary<DataColumn, string> columnToMemberMap)
        {
            List<string> columnNames = columnToMemberMap.Select(k => k.Key.ColumnName).ToList();
            if (!converters.TryGetValue(columnNames, out Func<DataRow, TEntity> value))
            {
                NewExpression instantiate = Expression.New(type);
                ParameterExpression dataRow = Expression.Parameter(typeof(DataRow), "dataRow");
                //Declare a dictionary to prevent creating more than 1 instance of a temporary parser type.
                ConcurrentDictionary<Type, ParameterExpression> parameters = new ConcurrentDictionary<Type, ParameterExpression>();
                List<MemberBinding> memberBindings = new List<MemberBinding>();
                //Iterate over the column map and generate the expressions needed.
                foreach (KeyValuePair<DataColumn, string> columnToMember in columnToMemberMap)
                {
                    if (TryGetMemberInfo(columnToMember.Value, columnToMember.Key.Table.CaseSensitive, out MemberInfo memberInfo))
                    {
                        //Get a member expression. This is used for type resolution.
                        Type memberType = Expression.MakeMemberAccess(instantiate, memberInfo).Type,
                            realType = Nullable.GetUnderlyingType(memberType) ?? memberType;
                        //Get the data column expression used to access the data row field.
                        DataColumnExpression dataColumn = DataExpression.DataColumn(DataExpression.DataTable(dataRow),
                            columnToMember.Key.ColumnName);
                        //Get the is null expression used to determine if the data field is null.
                        DataFieldIsNullExpression callDataFieldIsNull = DataExpression.DataFieldIsNull(
                            dataRow, dataColumn);
                        ConditionalExpression dataFieldIsNull = Expression.Condition(callDataFieldIsNull.Reduce(),
                            Expression.Default(memberType), Expression.Default(memberType));
                        //Get the is assignable from expression used to determine if the data column can be converted to the member type.
                        IsAssignableFromExpression callIsAssignableFrom = DataExpression.IsAssignableFrom(
                            DataExpression.DataType(dataColumn), memberType);
                        //Get the conditional expression used to process the data column type conversion.
                        ConditionalExpression isAssignableFrom = Expression.Condition(
                            callIsAssignableFrom.Reduce(), Expression.Convert(DataExpression.DataField(dataRow, dataColumn), memberType),
                            Expression.Default(memberType));
                        if (realType.GetMethods().Any(m => m.Name == "TryParse"))
                        {
                            //Get the is string parameter expression used for try parsing.
                            ParameterExpression stringLocal = parameters.GetOrAdd(typeof(string), Expression.Variable(typeof(string))),
                                outLocal = parameters.GetOrAdd(realType, Expression.Variable(realType));
                            //Get the type is assign and try parse expressions used for try parsing.
                            TypeIsAssignExpression typeIsAssign = DataExpression.TypeIsAssign(
                                DataExpression.DataField(dataRow, dataColumn), stringLocal);
                            TryParseExpression callTryParse = DataExpression.TryParse(stringLocal, outLocal);
                            //Get the conditional expression used to process the try parse conversion.
                            ConditionalExpression tryParse = Expression.Condition(
                                Expression.AndAlso(typeIsAssign, callTryParse),
                                Expression.Convert(outLocal, memberType), Expression.Default(memberType));
                            //Update is assignable from with the try parse method.
                            isAssignableFrom = isAssignableFrom.Update(isAssignableFrom.Test, isAssignableFrom.IfTrue, tryParse);
                        }
                        if (typeof(IConvertible).IsAssignableFrom(realType))
                        {
                            //Get the change type expression.
                            ChangeTypeExpression callChangeType = DataExpression.ChangeType(
                                DataExpression.DataField(dataRow, dataColumn), realType);
                            //Get the try catch expression for the change type.
                            UnaryExpression changeType = Expression.Convert(callChangeType, memberType);
                            //Determine if try parse is set. If so, update it and is assignable from. Otherwise, add change type to is assignable from.
                            if (isAssignableFrom.IfFalse is ConditionalExpression tryParse)
                            {
                                tryParse = tryParse.Update(tryParse.Test, tryParse.IfTrue, changeType);
                                isAssignableFrom = isAssignableFrom.Update(isAssignableFrom.Test, isAssignableFrom.IfTrue, tryParse);
                            }
                            else
                            {
                                isAssignableFrom = isAssignableFrom.Update(isAssignableFrom.Test, isAssignableFrom.IfTrue, changeType);
                            }
                        }
                        dataFieldIsNull = dataFieldIsNull.Update(dataFieldIsNull.Test, dataFieldIsNull.IfTrue, isAssignableFrom);
                        //Silently ignore conversion errors with the try catch expression. This is mostly for testing purposes and might be removed
                        //in the future.
                        memberBindings.Add(Expression.Bind(memberInfo, Expression.TryCatch(dataFieldIsNull,
                            Expression.Catch(typeof(Exception), Expression.Default(memberType)))));
                    }
                }
                //Get the converter lambda expression, creating the initialization block with its parameters.
                Expression<Func<DataRow, TEntity>> converter = Expression.Lambda<Func<DataRow, TEntity>>(
                    Expression.Block(parameters.Values, Expression.MemberInit(instantiate, memberBindings)), dataRow);
                //Use expression reducer to reduce the conversion lambda expression, then compile and return the delegate.
                converter = (Expression<Func<DataRow, TEntity>>)new ExpressionReducer().Visit(converter);
                value = converters.GetOrAdd(new HashSet<string>(columnNames), converter.Compile());
            }
            return value;
        }
        private static bool TryGetMemberInfo(string memberName, bool caseSensitive, out MemberInfo memberInfo)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            if (!caseSensitive)
            {
                flags |= BindingFlags.IgnoreCase;
            }
            if (type.GetProperty(memberName, flags) is PropertyInfo propertyInfo)
            {
                memberInfo = propertyInfo;
            }
            else if (type.GetField(memberName, flags) is FieldInfo fieldInfo)
            {
                memberInfo = fieldInfo;
            }
            else
            {
                memberInfo = null;
            }
            return memberInfo != null;
        }
        private static KeyValuePair<DataColumn, string> GetDataColumnKeyValuePair(DataColumn dataColumn) =>
            new KeyValuePair<DataColumn, string>(dataColumn, dataColumn.ColumnName);

        private class StringCollectionHelper : IEqualityComparer<ICollection<string>>
        {
            private static readonly Lazy<StringCollectionHelper> instance = new Lazy<StringCollectionHelper>(true);
            public static StringCollectionHelper GetInstance() => instance.Value;
            public bool Equals(ICollection<string> x, ICollection<string> y)
            {
                if (x == y)
                {
                    return true;
                }
                else if (x == null || y == null)
                {
                    return false;
                }
                else if (x.Count != y.Count)
                {
                    return false;
                }
                else if (x is HashSet<string>)
                {
                    foreach (string s in y)
                    {
                        if (!x.Contains(s))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    foreach (string s in x)
                    {
                        if (!x.Contains(s))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            public int GetHashCode(ICollection<string> obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                int hashCode = 2;
                unchecked
                {
                    foreach (string s in obj)
                    {
                        hashCode <<= (obj?.GetHashCode() ?? 10) * 230182;
                    }
                }
                return hashCode;
            }
        }
    }
}
