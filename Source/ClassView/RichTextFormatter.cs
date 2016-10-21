namespace Reflector
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Text;
	using Reflector.CodeModel;

	internal class RichTextFormatter : IFormatter
	{
		private StringWriter formatter = new StringWriter();
		private bool allowProperties = false;
		private int indent = 0;
		private ArrayList colors = new ArrayList();
		private bool newLine = false;

		public override string ToString()
		{
			StringWriter writer = new StringWriter();

			writer.Write("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033");
			writer.Write("{\\colortbl ");
			
			foreach (int color in this.colors)
			{
				writer.Write("\\red");
				writer.Write(((color >> 16) & 0xff).ToString());
				writer.Write("\\green");
				writer.Write(((color >> 8) & 0xff).ToString());
				writer.Write("\\blue");
				writer.Write((color & 0xff).ToString());
				writer.Write(";");
			}
			 
			writer.Write("}");
			writer.Write(formatter.ToString());			
			writer.Write("}");
			
			return writer.ToString();
		}
	
		public RichTextFormatter()
		{
			this.colors.Add(0x000000);	
		}
	
		public void Write(string text)
		{
			this.ApplyIndent();
			this.WriteQuote(text);
		}

		public void WriteDeclaration(string text)
		{
			this.Write(text);	
		}

		public void WriteDeclaration(string text, object target)
		{
			this.Write(text);	
		}

		public void WriteKeyword(string text)
		{
			this.WriteColor(text, (int) 0x0000FF);
		}

		public void WriteComment(string text)
		{
			this.WriteColor(text, (int) 0x008000);	
		}

		public void WriteLiteral(string text)
		{
			this.WriteColor(text, (int) 0x800000);	
		}

		public void WriteIndent()
		{
			this.indent++;
		}
				
		public void WriteLine()
		{
			this.formatter.Write("\\par");
			this.newLine = true;
		}

		public void WriteOutdent()
		{
			this.indent--;
		}

		public void WriteReference(string text, string toolTip, Object reference)
		{
			this.ApplyIndent();
			// this.formatter.Write("\\b ");
			this.WriteColor(text, 0x2b9191);
			// this.formatter.Write("\\b0 ");
			
		}

		public void WriteProperty(string propertyName, string propertyValue)
		{
			if (this.allowProperties)
			{
				throw new NotSupportedException();
			}
		}
		
		public void WriteQuote(string text)
		{
			text = text.Replace("\\", "\\\\");
			text = text.Replace("{", "\\{");	
			text = text.Replace("}", "\\}");
			this.formatter.Write(text);	
		}

		public bool AllowProperties
		{
			set 
			{ 
				this.allowProperties = value; 
			}
			
			get 
			{ 
				return this.allowProperties;
			}
		}

		private void WriteColor(string text, int color)
		{
			this.ApplyIndent();

			int index = this.colors.IndexOf(color);
			if (index == -1)
			{
				index = this.colors.Count;
				this.colors.Add(color);
			}
			
			this.formatter.Write("\\cf" + index.ToString() + " ");
			this.WriteQuote(text);
			this.formatter.Write("\\cf0 ");
	

		}

		private void ApplyIndent()
		{
			if (this.newLine)
			{
				for (int i = 0; i < this.indent; i++)
				{
					this.WriteQuote("    ");
				}
				
				this.newLine = false;
			}
		}
	}
}