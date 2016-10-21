namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.IO;
	using System.Text;
	using System.Windows.Forms;
	using Reflector.CodeModel;
	using Microsoft.Research.CommunityTechnologies.Treemap;

	internal sealed class CodeMetricView : Control
	{
		private static object syncRoot = new Object();

		private static volatile string searchString = "";
		private TextBox searchBox;
		private CodeMetric codeMetric;
		private Splitter splitter;
		private Control padding;
		private CodeMetricListView listView;
		private CodeMetricTreeMap treeMap;

		private IAssemblyBrowser assemblyBrowser;

		public CodeMetricView(CodeMetric codeMetric, IServiceProvider serviceProvider) : base(codeMetric.DisplayName)
		{
			this.TabStop = false;
			this.Dock = DockStyle.Fill;

			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));

			this.codeMetric = codeMetric;

			this.searchBox = new TextBox();
			this.searchBox.TabIndex = 1;
			this.searchBox.Dock = DockStyle.Top;
			this.searchBox.TextChanged += new EventHandler(this.SearchBox_TextChanged);

			this.listView = new CodeMetricListView(codeMetric);
			this.listView.TabIndex = 2;

			this.splitter = new Splitter();
			this.splitter.Dock = DockStyle.Bottom;

			switch (codeMetric.Level)
			{
				case CodeMetricLevel.Type:
				case CodeMetricLevel.Module:
					this.treeMap = new CodeMetricTreeMap(codeMetric);
					this.treeMap.TabStop = false;
					this.treeMap.Dock = DockStyle.Bottom;
					this.treeMap.Height = 200;
					break;
			}

			this.padding = new Control();
			this.padding.TabStop = false;
			this.padding.Size = new Size(1, 1);
			this.padding.Dock = DockStyle.Top;
		}

		public void Activate()
		{
			this.listView.Focus();
		}

		public void Copy()
		{
			this.listView.Copy();
		}

		private static string SearchString
		{
			get
			{
				lock (syncRoot)
				{
					return searchString;
				}
			}
			set
			{
				lock (syncRoot)
				{
					searchString = value;
				}
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (this.Parent != null)
			{
				this.SuspendLayout();
				this.Controls.Add(this.listView);
				this.Controls.Add(this.padding);
				this.Controls.Add(this.searchBox);
				this.Controls.Add(this.splitter);

				if (this.treeMap != null)
				{
					this.Controls.Add(this.treeMap);
				}

				this.ResumeLayout();

				this.listView.ItemActivate += new EventHandler(this.ListView_ItemActivate);
				this.listView.ActiveItemChanged += new EventHandler(this.ListView_ActiveItemChanged);
				this.listView.ColumnClick += new ColumnClickEventHandler(this.ListView_ColumnClick);

				if (this.treeMap != null)
				{
					this.treeMap.ActiveItemChanged += new EventHandler(this.TreeMap_ActiveItemChanged);
				}
			}
			else
			{
				this.listView.ItemActivate -= new EventHandler(this.ListView_ItemActivate);
				this.listView.ActiveItemChanged -= new EventHandler(this.ListView_ActiveItemChanged);
				this.listView.ColumnClick -= new ColumnClickEventHandler(this.ListView_ColumnClick);

				if (this.treeMap != null)
				{
					this.treeMap.ActiveItemChanged -= new EventHandler(this.TreeMap_ActiveItemChanged);
				}

				this.Controls.Clear();
			}
		}

		private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (this.treeMap != null)
			{
				if (ModifierKeys == Keys.Shift)
				{
					this.treeMap.SetColorColumn(e.Column);
				}
				else
				{
					this.treeMap.SetSizeColumn(e.Column);
				}
			}
		}

		private void ListView_ItemActivate(object sender, EventArgs e)
		{
			object target = this.listView.ActiveItem;
			if (target != null)
			{
				this.assemblyBrowser.ActiveItem = target;
			}
		}

		private void ListView_ActiveItemChanged(object sender, EventArgs e)
		{
			if (this.treeMap != null)
			{
				object target = this.listView.ActiveItem;
				if (target != null)
				{
					this.treeMap.ActiveItem = target;
				}
			}
		}

		private void TreeMap_ActiveItemChanged(object sender, EventArgs e)
		{
			object target = this.treeMap.ActiveItem;
			if (target != null)
			{
				this.listView.ActiveItem = target;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (this.Visible)
			{
				string search = SearchString;
				this.searchBox.Text = search;
				if (search != null && search.Length > 0)
				{
					this.SearchItem(search);
				}
			}
		}

		private void SearchBox_TextChanged(object sender, EventArgs e)
		{
			SearchString = this.searchBox.Text.ToLower();
			this.SearchItem(SearchString);
		}

		private void SearchItem(string search)
		{
			this.listView.SearchItem(search);
		}

		private class CodeMetricListView : ListView
		{
			public event EventHandler ActiveItemChanged;

			private CodeMetric codeMetric;

			public CodeMetricListView(CodeMetric codeMetric)
			{
				this.codeMetric = codeMetric;

				this.Dock = DockStyle.Fill;
				this.View = View.Details;
				this.FullRowSelect = true;
				this.HeaderStyle = ColumnHeaderStyle.Clickable;
				this.HideSelection = false;
				this.SmallImageList = BrowserResource.ImageList;

				DataTable dataTable = this.codeMetric.Result;

				int columns = dataTable.Columns.Count;

				int index = 0;
				foreach (DataColumn column in dataTable.Columns)
				{
					ColumnHeader columnHeader = new ColumnHeader();
					columnHeader.Text = column.ColumnName;
					columnHeader.Width = (index == 0) ? 250 : 50;

					this.Columns.Add(columnHeader);
					index++;
				}

				foreach (DataRow row in dataTable.Rows)
				{
					ListViewItem item = new ListViewItem();
					item.Tag = row;
					object target = this.codeMetric.FindItem(row);

					if (target != null)
					{
						IMethodDeclaration methodDeclaration = target as IMethodDeclaration;
						if (methodDeclaration != null)
						{
							item.ImageIndex = IconHelper.GetImageIndex(methodDeclaration);
							item.ForeColor = Color.FromArgb(IconHelper.GetColorDeclaringType(methodDeclaration));
						}

						ITypeDeclaration typeDeclaration = target as ITypeDeclaration;
						if (typeDeclaration != null)
						{
							item.ImageIndex = IconHelper.GetImageIndex(typeDeclaration);
							item.ForeColor = Color.FromArgb(IconHelper.GetColor(typeDeclaration));
						}

						IModule module = target as IModule;
						if (module != null)
						{
							item.ImageIndex = BrowserResource.Module;
						}
					}

					item.Text = (string)row[0];

					for (int i = 1; i < columns; i++)
					{
						ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
						subItem.Text = row[i].ToString();
						item.SubItems.Add(subItem);
					}

					this.Items.Add(item);

					index++;
				}
			}

			public void Copy()
			{
				ICollection items = this.SelectedItems;
				if (items.Count == 0)
				{
					items = this.Items;
				}

				using (StringWriter writer = new StringWriter())
				{
					DataTable table = this.codeMetric.Result;
					int count = table.Columns.Count;

					for (int i = 0; i < count; i++)
					{
						DataColumn column = table.Columns[i];
						if (i != 0)
						{
							writer.Write(", ");
						}

						writer.Write(column.ColumnName);
					}

					writer.WriteLine();

					foreach (ListViewItem item in items)
					{
						DataRow row = (DataRow)item.Tag;
						for (int i = 0; i < count; i++)
						{
							if (i != 0)
							{
								writer.Write(", ");
							}

							writer.Write(row[i].ToString());
						}

						writer.WriteLine();
					}

					Clipboard.SetDataObject(writer.ToString(), true);
				}
			}

			public void SearchItem(string value)
			{
				this.SelectedItems.Clear();

				bool selectFirst = true;

				if ((value != null) && (value.Length > 0))
				{
					for (int i = 0; i < this.Items.Count; i++)
					{
						ListViewItem item = (ListViewItem)this.Items[i];
						if (item.Text.ToLower().IndexOf(value) != -1)
						{
							item.Selected = true;

							if (selectFirst)
							{
								this.EnsureVisible(i);
								selectFirst = false;
							}
						}
					}
				}
			}

			protected override void OnParentChanged(EventArgs e)
			{
				base.OnParentChanged(e);

				for (int i = 0; i < this.Items.Count; i++)
				{
					this.Items[i].Focused = false;
					this.Items[i].Selected = false;
				}

				if (this.Parent != null)
				{
					if (this.Items.Count > 0)
					{
						this.Items[0].Focused = true;
						this.Items[0].Selected = true;
					}
				}
			}

			protected override void OnSelectedIndexChanged(EventArgs e)
			{
				base.OnSelectedIndexChanged(e);

				object target = this.ActiveItem;
				if (target != null)
				{
					this.OnActiveItemChanged(EventArgs.Empty);
				}
			}

			public object ActiveItem
			{
				get
				{
					if (this.SelectedItems.Count == 1)
					{
						ListViewItem item = (ListViewItem)this.SelectedItems[0];
						DataRow row = (DataRow)item.Tag;
						object target = this.codeMetric.FindItem(row);
						if (target != null)
						{
							return target;
						}
					}

					return null;
				}

				set
				{
					if (this.ActiveItem != value)
					{
						this.SelectedItems.Clear();

						if (value != null)
						{
							for (int i = 0; i < this.Items.Count; i++)
							{
								ListViewItem item = this.Items[i];
								DataRow row = (DataRow)item.Tag;
								object target = this.codeMetric.FindItem(row);
								if (target != null)
								{
									if (target == value)
									{
										item.Selected = true;
										item.Focused = true;
										this.EnsureVisible(i);
										return;
									}
								}
							}
						}
					}
				}
			}

			protected override void OnColumnClick(ColumnClickEventArgs e)
			{
				base.OnColumnClick(e);

				if (this.ListViewItemSorter == null)
				{
					this.ListViewItemSorter = new Comparer(e.Column);
				}
				else
				{
					Comparer comparer = (Comparer)this.ListViewItemSorter;
					comparer.Column = e.Column;

					this.ListViewItemSorter = null;
					this.ListViewItemSorter = comparer;
				}

				this.Sorting = SortOrder.Ascending;
			}

			protected virtual void OnActiveItemChanged(EventArgs e)
			{
				if (this.ActiveItemChanged != null)
				{
					this.ActiveItemChanged(this, e);
				}
			}

			private class Comparer : IComparer
			{
				private int column = 0;
				private int factor = -1;

				public Comparer(int column)
				{
					this.column = column;
				}

				public int Column
				{
					get
					{
						return this.column;
					}

					set
					{
						if (this.column == value)
						{
							this.factor *= -1;
						}
						else
						{
							this.factor = -1;
						}

						this.column = value;
					}
				}

				public int Compare(Object a, Object b)
				{
					ListViewItem item1 = (ListViewItem)a;
					ListViewItem item2 = (ListViewItem)b;
					DataRow row1 = (DataRow)item1.Tag;
					DataRow row2 = (DataRow)item2.Tag;
					IComparable comparable1 = row1[this.column] as IComparable;
					IComparable comparable2 = row2[this.column] as IComparable;
					return this.factor * comparable1.CompareTo(comparable2);
				}
			}
		}

		private class CodeMetricTreeMap : Control
		{
			public event EventHandler ActiveItemChanged;

			private CodeMetric codeMetric;
			private TreemapControl treeMap;

			private int sizeColumn = 1;
			private int colorColumn = 2;

			public CodeMetricTreeMap(CodeMetric codeMetric)
			{
				this.codeMetric = codeMetric;

				this.SuspendLayout();

				this.treeMap = new TreemapControl();
				this.treeMap.Dock = DockStyle.Fill;
				this.treeMap.MinColor = Color.Red;
				this.treeMap.MaxColor = Color.Green;
				this.treeMap.DiscreteNegativeColors = 50;
				this.treeMap.DiscretePositiveColors = 50;
				this.treeMap.PaddingPx = 1;
				this.treeMap.PenWidthPx = 1;
				this.treeMap.SelectedNodeChanged += new TreemapControl.NodeEventHandler(this.TreeMap_SelectedNodeChanged);
				this.Controls.Add(this.treeMap);

				this.ResumeLayout();

				this.Populate();
			}

			public void SetSizeColumn(int index)
			{
				if (index != 0)
				{
					this.sizeColumn = index;
					this.Populate();
				}
			}

			public void SetColorColumn(int index)
			{
				if (index != 0)
				{
					this.colorColumn = index;
					this.Populate();
				}
			}

			private void TreeMap_SelectedNodeChanged(object sender, NodeEventArgs nodeEventArgs)
			{
				object target = this.ActiveItem;
				if (target != null)
				{
					this.OnActiveItemChanged(EventArgs.Empty);
				}
			}

			public object ActiveItem
			{
				get
				{
					Node node = this.treeMap.SelectedNode;
					if (node != null)
					{
						return node.Tag;
					}

					return null;
				}

				set
				{
					if ((this.treeMap.SelectedNode == null) || (this.treeMap.SelectedNode.Tag != value))
					{
						foreach (Node node in this.treeMap.Nodes)
						{
							Node selectedNode = this.FindNode(node, value);
							if (selectedNode != null)
							{
								this.treeMap.SelectNode(selectedNode);
								return;
							}
						}
					}
				}
			}

			protected virtual void OnActiveItemChanged(EventArgs e)
			{
				if (this.ActiveItemChanged != null)
				{
					this.ActiveItemChanged(this, e);
				}
			}

			private void Populate()
			{
				this.SuspendLayout();

				this.treeMap.Nodes.Clear();

				float minColorMetric = -0.0001f;
				float maxColorMetric = 0;

				IList<DataRow> rows = this.codeMetric.Result.Rows;

				if (this.codeMetric.Level == CodeMetricLevel.Type)
				{
					NodeBuilder builder = new NodeBuilder(treeMap, this.sizeColumn, this.colorColumn);
					for (int i = 0; i < rows.Count; ++i)
					{
						// if (i > 100) break;
						DataRow row = rows[i];
						object item = this.codeMetric.FindItem(row);
						builder.VisitItem(item, row);

						float colorMetric = Math.Max(0, ConvertToSingle(row[this.colorColumn]));

						if (colorMetric < minColorMetric)
						{
							minColorMetric = colorMetric;
							this.treeMap.MinColorMetric = minColorMetric;
						}

						if (colorMetric > maxColorMetric)
						{
							maxColorMetric = colorMetric;
							this.treeMap.MaxColorMetric = maxColorMetric;
						}
					}
				}

				if (this.codeMetric.Level == CodeMetricLevel.Module)
				{
					for (int i = 0; i < rows.Count; i++)
					{
						DataRow row = rows[i];

						float sizeMetric = ConvertToSingle(row[this.sizeColumn]);
						float colorMetric = ConvertToSingle(row[this.colorColumn]);

						this.treeMap.Nodes.Add((string)row[0], sizeMetric, colorMetric, this.codeMetric.FindItem(row));

						if (colorMetric < minColorMetric)
						{
							minColorMetric = colorMetric;
							this.treeMap.MinColorMetric = minColorMetric;
						}

						if (colorMetric > maxColorMetric)
						{
							maxColorMetric = colorMetric;
							this.treeMap.MaxColorMetric = maxColorMetric;
						}
					}
				}

				this.ResumeLayout();
			}

			private static float ConvertToSingle(object value)
			{
				if (value is int)
				{
					return (float)(int)value;
				}

				if (value is float)
				{
					return (float)value;
				}

				if (value is double)
				{
					return (float)(double)value;
				}

				return 0;
			}

			internal sealed class NodeBuilder
			{
				private readonly TreemapControl treemap;
				private readonly int sizeColumn;
				private readonly int colorColumn;
				private readonly Hashtable nodes = new Hashtable();

				public NodeBuilder(TreemapControl treemap,
					int sizeColumn,
					int colorColumn)
				{
					this.treemap = treemap;
					this.sizeColumn = sizeColumn;
					this.colorColumn = colorColumn;
				}

				public float GetSizeMetric(DataRow row)
				{
					float value = ConvertToSingle(row[this.sizeColumn]);
					return Math.Max(0, value);
				}

				public float GetColorMetric(DataRow row)
				{
					float value = ConvertToSingle(row[this.colorColumn]);
					return Math.Max(0, value);
				}

				public Node VisitItem(object item, DataRow row)
				{
					ITypeReference typeReference = item as ITypeReference;
					if (typeReference != null)
						return this.VisitTypeReference(typeReference, row);

					IMethodDeclaration methodDeclaration = item as IMethodDeclaration;
					if (methodDeclaration != null)
						return this.VisitMethodDeclaration(methodDeclaration, row);

					return null;
				}

				public Node VisitNamespace(string ns)
				{
					Node node = this.nodes[ns] as Node;
					if (node == null)
					{
						node = this.treemap.Nodes.Add(ns, 0, 0);
						node.ToolTip = ns;
						this.nodes.Add(ns, node);
					}
					return node;
				}

				public Node VisitTypeReference(ITypeReference value, DataRow row)
				{
					if (value == null) return null;

					Node typeNode = this.nodes[value] as Node;
					Node nsNode = this.VisitNamespace(value.Namespace);
					if (typeNode == null)
					{
						typeNode = nsNode.Nodes.Add(value.Name, 0, this.GetColorMetric(row), value);
						typeNode.ToolTip = Helper.GetNameWithResolutionScope(value);
						this.nodes.Add(value, typeNode);
					}

					float sizeMetric = this.GetSizeMetric(row);
					typeNode.SizeMetric += sizeMetric;
					if (nsNode != null)
						nsNode.SizeMetric += sizeMetric;

					return typeNode;
				}

				public Node VisitMethodDeclaration(IMethodDeclaration value, DataRow row)
				{
					Node typeNode = this.VisitTypeReference(value.DeclaringType as ITypeReference, row);
					return typeNode;
				}
			}

			private Node FindNode(object value)
			{
				foreach (Node node in this.treeMap.Nodes)
				{
					Node findNode = this.FindNode(node, value);
					if (findNode != null)
					{
						return findNode;
					}
				}

				return null;
			}

			private Node FindNode(Node node, object value)
			{
				if (node.Tag == value)
				{
					return node;
				}

				foreach (Node subNode in node.Nodes)
				{
					Node selectedNode = this.FindNode(subNode, value);
					if (selectedNode != null)
					{
						return selectedNode;
					}
				}

				return null;
			}
		}
	}
}
