namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Threading;
	using Reflector.CodeModel;

	public abstract class CodeMetric
	{
		public event ComputationProgressEventHandler Progress;

		private string name;
		private CodeMetricLevel level;

		private int abortPending = 0;
		private DataTable result;
		private DataColumn nameColumn;
		private Hashtable rowItems = new Hashtable();

		public CodeMetric(string name, CodeMetricLevel level)
		{
			this.name = name;
			this.level = level;
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string DisplayName
		{
			get
			{
				return this.Name + " Metrics";
			}
		}

		public CodeMetricLevel Level
		{
			get
			{
				return this.level;
			}
		}

		public bool IsAbortPending()
		{
			return this.abortPending > 0;
		}

		public DataTable Result
		{
			get
			{
				return this.result;
			}
		}

		protected virtual void OnProgress(ComputationProgressEventArgs args)
		{
			if (this.Progress != null)
			{
				this.Progress(this, args);
			}
		}

		public void Analyze(IAssembly[] assemblies)
		{
			this.result = new DataTable();

			this.nameColumn = new DataColumn("Name", typeof(string));
			this.result.Columns.Add(this.nameColumn);

			this.rowItems.Clear();

			this.AddColumns();

			try
			{
				this.InternalCompute(assemblies);
			}
			finally
			{
				this.Reset();
			}
		}

		protected abstract void InternalCompute(IAssembly[] assemblies);

		protected abstract void AddColumns();

		protected void AddColumn(string name)
		{
			this.AddColumn(name, typeof(int));
		}

		protected void AddColumn(string name, Type type)
		{
			DataColumn column = new DataColumn(name, type);
			this.result.Columns.Add(column);
		}

		public virtual void Abort()
		{
			Interlocked.Increment(ref this.abortPending);
		}

		public void Reset()
		{
			this.abortPending = 0;
		}

		public object FindItem(DataRow row)
		{
			if (row == null)
			{
				throw new ArgumentNullException("row");
			}

			return this.rowItems[row];
		}

		protected void AddRow(object item, params object[] items)
		{
			DataRow row = this.result.NewRow();
			row[0] = item.ToString();

			this.rowItems.Add(row, item);

			for (int i = 0; i < items.Length; i++)
			{
				row[i + 1] = items[i];
			}

			this.result.Rows.Add(row);
		}

		public override string ToString()
		{
			return this.DisplayName;
		}
	}
}
