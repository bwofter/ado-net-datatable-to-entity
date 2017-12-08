namespace BWofter.Converters.Tests.DataTableConverterTests
{
    using BWofter.Converters.Data;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    [TestClass]
    public sealed class ErrorTest
    {
        private class Convertible
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public DateTime Column3 { get; set; }
        }
        private static readonly Lazy<DataTable> testTable = new Lazy<DataTable>(() =>
        {
            DataTable table = new DataTable();
            table.Columns.Add("Column1", typeof(int));
            table.Columns.Add("Column2", typeof(string));
            table.Columns.Add("Column3", typeof(DateTime?));
            table.Rows.Add(10, "I am row", null);
            return table;
        });

        [TestMethod]
        public void NullDataTable()
        {
            DataTable nullDt = null;
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(nullDt));
        }
        [TestMethod]
        public void NullColumnToPropertyMap()
        {
            //TODO: Figure out why this test fails even though it should be succeeding - This exception is thrown immediately and
            //manual testing confirms it is being thrown. Like seriously, what the hell is this even doing?
            DataTable dt = new DataTable();
            Dictionary<DataColumn, string> nullCtP = null;
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(dt, nullCtP));
        }
        [TestMethod]
        public void EmptyDataRowCollection()
        {
            DataTable dt = new DataTable();
            Assert.ThrowsException<ArgumentException>(() => DataTableConverter<Convertible>.ToEntities(dt.Rows));
        }
        [TestMethod]
        public void NullDataRowCollection()
        {
            DataRowCollection drc = null;
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(drc));
        }
        [TestMethod]
        public void DataRowCollectionNullColumnToPropertyMap()
        {
            //TODO: Figure out why this test fails even though it should be succeeding - This exception is thrown immediately and
            //manual testing confirms it is being thrown. Like seriously, what the hell is this even doing?
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(testTable.Value.Rows, null));
        }
        [TestMethod]
        public void NullIEnumerableDataRow()
        {
            IEnumerable<DataRow> idr = null;
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(idr));
        }
        [TestMethod]
        public void IEnumerableDataRowNullColumnToPropertyMap()
        {
            //TODO: Figure out why this test fails even though it should be succeeding - This exception is thrown immediately and
            //manual testing confirms it is being thrown. Like seriously, what the hell is this even doing?
            IEnumerable<DataRow> idr = testTable.Value.Rows.Cast<DataRow>();
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(idr, null));
        }
        [TestMethod]
        public void NullArrayDataRow()
        {
            DataRow[] adr = null;
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(adr));
        }
        [TestMethod]
        public void ArrayDataRowNullColumnToPropertyMap()
        {
            //TODO: Figure out why this test fails even though it should be succeeding - This exception is thrown immediately and
            //manual testing confirms it is being thrown. Like seriously, what the hell is this even doing?
            DataRow[] adr = testTable.Value.Rows.Cast<DataRow>().ToArray();
            Assert.ThrowsException<ArgumentNullException>(() => DataTableConverter<Convertible>.ToEntities(adr, null));
        }
    }
}
