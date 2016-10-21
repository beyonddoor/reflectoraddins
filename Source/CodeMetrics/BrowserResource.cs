namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Windows.Forms;
	
	internal sealed class BrowserResource
	{
		private static ImageList browserImageList;

		static BrowserResource()
		{
			string resourceName = "Reflector.CodeMetrics.Browser16.png";
			Bitmap browser = new Bitmap(typeof(BrowserResource).Assembly.GetManifestResourceStream(resourceName));

			browserImageList = new ImageList();
			browserImageList.ImageSize = new Size(browser.Height, browser.Height);
			browserImageList.Images.AddStrip(browser);
			browserImageList.ColorDepth = ColorDepth.Depth32Bit;
			browserImageList.TransparentColor = Color.FromArgb(255, 0, 128, 0);
		}

		public static ImageList ImageList
		{
			get
			{
				return browserImageList;
			}
		}

		public const int None = 0;

		public const int Namespace = None + 1;
		public const int Class = Namespace + 6;
		public const int Interface = Class + 6;
		public const int Structure = Interface + 6;
		public const int Enumeration = Structure + 6;
		public const int Delegate = Enumeration + 6;
		public const int Primitive = Delegate + 6;
		public const int Generic = Primitive + 6;
		public const int Constructor = Generic + 6;
		public const int Method = Constructor + 12;
		public const int Field = Method + 24;
		public const int EnumerationElement = Field + 12;
		public const int Property = EnumerationElement + 6;
		public const int PropertyRead = Property + 12;
		public const int PropertyWrite = PropertyRead + 12;
		public const int Event = PropertyWrite + 12;

		public const int Assembly = Event + 12;
		public const int AssemblyReference = Assembly + 1;
		public const int Module = AssemblyReference + 1;
		public const int ModuleReference = Module + 1;
		public const int References = ModuleReference + 1;

		public const int Folder = References + 1;
		public const int ByteArrayResource = Folder + 2;
		public const int Resource = ByteArrayResource + 1;
		public const int ImageResource = Resource + 1;
		public const int TextResource = ImageResource + 1;
		public const int StringResource = TextResource + 1;

		public const int BaseTypes = StringResource + 1;
		public const int DerivedTypes = BaseTypes + 1;

		public const int Information = DerivedTypes + 1;
		public const int Error = Information + 1;
		public const int Wait = Information + 2;

		public const int Equality = Information + 3;
	}
}
