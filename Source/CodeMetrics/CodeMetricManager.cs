namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using Reflector.CodeModel;

	public sealed class CodeMetricManager
	{
		public event EventHandler BeginRun;
		public event EventHandler EndRun;
		public event CodeMetricEventHandler BeginRunMetric;
		public event CodeMetricEventHandler EndRunMetric;

		private Exception errorException = null;
		private bool abortPending = false;

        private CodeMetricCollection codeMetrics = new CodeMetricCollection();
		private ArrayList assemblies = new ArrayList();

        public void Register(CodeMetric codeMetric)
        {
			this.codeMetrics.Add(codeMetric);
        }

        public CodeMetricCollection CodeMetrics
        {
            get 
			{ 
				return this.codeMetrics; 
			}
        }

		public void AddAssembly(IAssembly value)
		{
			if (!this.assemblies.Contains(value))
			{
				this.assemblies.Add(value);
			}
		}

		public void RemoveAssembly(IAssembly value)
		{
			if (this.assemblies.Contains(value))
			{
				this.assemblies.Remove(value);
			}
		}

		public IAssembly[] Assemblies
		{
			get
			{
				IAssembly[] array = new IAssembly[this.assemblies.Count];
				this.assemblies.CopyTo(array, 0);
				return array;
			}
		}

		public Exception ErrorException
		{
			get { return this.errorException; }
		}

		public void Analyze()
		{
			this.abortPending = false;

			try
			{
				this.OnBeginRun(EventArgs.Empty);
				foreach (CodeMetric codeMetric in this.CodeMetrics)
				{
					if (!codeMetric.IsAbortPending())
					{
						CodeMetricEventArgs e = new CodeMetricEventArgs(codeMetric);
						this.OnBeginRunMetric(e);
						codeMetric.Analyze(this.Assemblies);
						this.OnEndRunMetric(e);
					}
				}
			}
			catch (Exception exception)
			{
				this.errorException = exception;
			}
			finally
			{
				this.OnEndRun(EventArgs.Empty);
			}
		}

		public void Abort()
		{
			this.abortPending = true;

			foreach (CodeMetric codeMetric in this.CodeMetrics)
			{
				codeMetric.Abort();
			}
		}

		public bool IsAbortPending()
		{
			return this.abortPending;
		}

		public void Save(TextWriter textWriter)
		{
			XmlTextWriter writer = new XmlTextWriter(textWriter);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartElement("Report");

			foreach (CodeMetric codeMetric in this.CodeMetrics)
			{
				if (codeMetric.Result != null)
				{
					writer.WriteStartElement("Metric");
					{
						writer.WriteAttributeString("Name", codeMetric.Name);
						foreach (DataRow row in codeMetric.Result.Rows)
						{
							writer.WriteStartElement(codeMetric.Name);

							IList<DataColumn> columns = codeMetric.Result.Columns;
							for (int i = 0; i < columns.Count; i++)
							{
								DataColumn column = columns[i];
								string name = column.ColumnName;
								object value = row[column];

								if (i == 0)
								{
									switch (codeMetric.Level)
									{
										case CodeMetricLevel.Type:
											value = Helper.GetNameWithResolutionScope((ITypeReference) codeMetric.FindItem(row));
											break;

										case CodeMetricLevel.Method:
											value = Helper.GetNameWithDeclaringType((IMethodReference) codeMetric.FindItem(row));
											break;
									}
								}

								writer.WriteAttributeString(name, value.ToString());
							}

							writer.WriteEndElement();
						}
					}
					writer.WriteEndElement();
				}
			}

			writer.WriteEndElement();
		}

		private void OnBeginRun(EventArgs e)
		{
			this.errorException = null;

			if (this.BeginRun != null)
			{
				this.BeginRun(this, e);
			}
		}

		private void OnEndRun(EventArgs e)
		{
			foreach (CodeMetric codeMetric in this.CodeMetrics)
			{
				codeMetric.Reset();
			}

			if (this.EndRun != null)
			{
				this.EndRun(this, e);
			}
		}

		private void OnBeginRunMetric(CodeMetricEventArgs e)
		{
			if (this.BeginRunMetric != null)
			{
				this.BeginRunMetric(this, e);
			}
		}

		private void OnEndRunMetric(CodeMetricEventArgs e)
		{
			if (this.EndRunMetric != null)
			{
				this.EndRunMetric(this, e);
			}
		}
	}
}
