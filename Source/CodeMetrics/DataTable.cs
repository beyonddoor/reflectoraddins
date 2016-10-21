namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections.Generic;

	public class DataTable
	{
		private IList<DataColumn> columns = new List<DataColumn>();
		private IList<DataRow> rows = new List<DataRow>();

		public DataRow NewRow()
		{
			return new DataRow(this);
		}

		public IList<DataColumn> Columns
		{
			get { return this.columns; }
		}

		public IList<DataRow> Rows
		{
			get { return this.rows; }
		}
	}

	public class DataColumn
	{
		private string columnName;
		private Type dataType;

		public DataColumn(string columnName, Type dataType)
		{
			this.columnName = columnName;
			this.dataType = dataType;
		}

		public string ColumnName
		{
			get { return this.columnName; }
		}

		public Type DataType
		{
			get { return this.dataType; }
		}
	}

	public class DataRow
	{
		private DataTable table;
		private object[] columns;

		internal DataRow(DataTable table)
		{
			this.table = table;
			this.columns = new object[table.Columns.Count];
		}

		public object this[DataColumn column]
		{
			get
			{
				int index = this.table.Columns.IndexOf(column);
				if (index != -1)
				{
					return this.columns[index];
				}

				return null;
			}
		}

		public object this[int index]
		{
			get { return this.columns[index]; }
			set { this.columns[index] = value; }
		}
	}
}