namespace Reflector
{
	using System;
	using System.Collections;
	using System.Drawing;
	using System.IO;
	using System.Reflection;
	using System.Windows.Forms;
	using Reflector.CodeModel;

	internal class CodeModelWindow : TreeView, IPackage
	{
		private IAssemblyBrowser assemblyBrowser;
		private IWindowManager windowManager;
		private ICommandBarManager commandBarManager;
		private ICommandBarSeparator separator;
		private ICommandBarButton showCodeModelViewButton;

		public CodeModelWindow()
		{
			this.Dock = DockStyle.Fill;

			this.ImageList = new ImageList();
			this.ImageList.Images.AddStrip(new Bitmap(this.GetType().Assembly.GetManifestResourceStream("Reflector.CodeModelViewer.Icon.png")));
			this.ImageList.ColorDepth = ColorDepth.Depth32Bit;
			this.ImageList.TransparentColor = Color.FromArgb(255, 0, 128, 0);
		}
		
		~CodeModelWindow()
		{
			this.AssemblyBrowser = null;	
		}

		// IPackage.Load
		public void Load(IServiceProvider serviceProvider)
		{	
			this.AssemblyBrowser = (IAssemblyBrowser) serviceProvider.GetService(typeof(IAssemblyBrowser));

			this.windowManager = (IWindowManager) serviceProvider.GetService(typeof(IWindowManager));
			this.windowManager.Windows.Add("CodeModelWindow", this, "Code Model Viewer");

			this.commandBarManager = (ICommandBarManager) serviceProvider.GetService(typeof(ICommandBarManager));
			this.separator = this.commandBarManager.CommandBars["Tools"].Items.AddSeparator();
			this.showCodeModelViewButton = this.commandBarManager.CommandBars["Tools"].Items.AddButton("Show Code Model View", new EventHandler(this.ShowCodeModelViewButton_Click));
		}

		// IPackage.Unload
		public void Unload()
		{
			this.windowManager.Windows.Remove("CodeModelWindow");
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.showCodeModelViewButton);
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.separator);
		}
		
		private void ShowCodeModelViewButton_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["CodeModelWindow"].Visible = true;
		}

		public IAssemblyBrowser AssemblyBrowser
		{
			get
			{
				return this.assemblyBrowser;				
			}
			
			set
			{
				if (this.assemblyBrowser != null)
				{
					this.assemblyBrowser.ActiveItemChanged -= new EventHandler(this.AssemblyBrowser_ActiveItemChanged);
				}
				
				this.assemblyBrowser = value;

				if (this.assemblyBrowser != null)
				{
					this.assemblyBrowser.ActiveItemChanged += new EventHandler(this.AssemblyBrowser_ActiveItemChanged);
				}
			}	
		}

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			base.OnBeforeExpand(e);

			BrowserNode node = (BrowserNode) e.Node;
			node.PerformExpandItem();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			
			if (this.Parent != null)
			{
				this.UpdateItem();	
			}
		}

		private void AssemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
		{
			this.UpdateItem();
		}

		private void UpdateItem()
		{
			this.Nodes.Clear();

			if (this.assemblyBrowser.ActiveItem != null)
			{
				ObjectNode node = new ObjectNode(this.assemblyBrowser.ActiveItem);
				this.Nodes.Add(node);	
			}
		}
		
		private class BrowserNode : TreeNode
		{
			internal virtual void PerformExpandItem()
			{
				this.Nodes.Clear();	
			}	
		}
		
		private class ObjectNode : BrowserNode
		{
			private object value;

			public ObjectNode(object value)
			{
				this.value = value;
				this.ImageIndex = this.SelectedImageIndex = 0;
				
				if (this.value == null)
				{
					this.Text = "(null)";	
				}
				else 
				{
					Type type = this.value.GetType();

					if ((type.IsPrimitive) || (value is string))
					{
						this.ImageIndex = this.SelectedImageIndex = 2;
						this.Text = "'" + value.ToString() + "'";	
					}
					else if (type.IsEnum) 
					{
						this.ImageIndex = this.SelectedImageIndex = 1;
						this.Text = type.Name + "." + value.ToString();	
					}
					else
					{
						this.Text = /* "[" + type.Name + "] " + */ "{" + value.ToString() + "}";
	
						if (this.GetProperties().Count > 0)
						{
							this.Nodes.Add(new TreeNode());	
						}
					}
				}
			}
			
			internal override void PerformExpandItem()
			{
				base.PerformExpandItem();

				foreach (PropertyInfo propertyInfo in this.GetProperties())
				{
					this.Nodes.Add(new PropertyNode(value, propertyInfo));
				}
			}
			
			private ICollection GetProperties()
			{
				ArrayList list = new ArrayList();

				Type type = this.value.GetType();
				if (type != null)
				{
					foreach (Type interfaceType in type.GetInterfaces())
					{
						PropertyInfo[] properties = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
						foreach (PropertyInfo propertyInfo in properties)
						{
							if (!list.Contains(propertyInfo))
							{
								list.Add(propertyInfo);	
							}
						}
					}
				}

				return list;				
			}
		}
		
		private class PropertyNode : BrowserNode
		{
			private object value;
			private PropertyInfo propertyInfo;

			public PropertyNode(object value, PropertyInfo propertyInfo)
			{
				this.value = value;
				this.propertyInfo = propertyInfo;

				this.Text = "." + this.propertyInfo.Name;
				
				object target = this.propertyInfo.GetValue(value, null);

				this.ImageIndex = this.SelectedImageIndex = 3;

				IEnumerable enumerable = target as IEnumerable;
				if (enumerable != null)
				{
					if ((target is string) || (enumerable.GetEnumerator().MoveNext()))
					{
						this.Nodes.Add(new TreeNode());	
					}
				}
				else
				{
					this.Nodes.Add(new TreeNode());	
				}
			}	

			internal override void PerformExpandItem()
			{
				base.PerformExpandItem();

				object target = this.propertyInfo.GetValue(value, null);
				if (target == null)
				{
					this.Nodes.Add(new ObjectNode(target));
				}
				else
				{
					Type type = target.GetType();
					IEnumerable enumerable = target as IEnumerable;
					
					if ((type.IsPrimitive) || (target is string))
					{
						this.Nodes.Add(new ObjectNode(target));
					}
					else if (enumerable != null)
					{
						int index = 0;
						foreach (object item in enumerable)
						{
							this.Nodes.Add(new ObjectNode(item));
	
							index++;
							if (index > 100)
							{
								TreeNode node = new TreeNode();
								node.Text = "...";
								this.Nodes.Add(node);	
								break;
							}
						}
					}
					else
					{
						this.Nodes.Add(new ObjectNode(target));
					}
				}
			}
		}
		
		private class LiteralNode : BrowserNode
		{
			public LiteralNode(object value)
			{
			}
		}
	}
}
