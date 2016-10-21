namespace Reflector.ComLoader
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Runtime.InteropServices;
	using Reflector;

	[Serializable]
    internal class TypeLibraryConverter
    {
		public event TextEventHandler Message;

		private string typeLibraryLocation;
		private string assemblyLocation;
		private string assemblyNamespace = null;
		private Version assemblyVersion = null;
		private bool unsafeInterfaces = false;
		private bool safeArrayAsSystemArray = false;
		private bool primaryInteropAssembly = false;
		private byte[] publicKey = null;
		private StrongNameKeyPair keyPair = null;

		public string TypeLibraryLocation
		{
			get 
			{ 
				return this.typeLibraryLocation; 
			}
			
			set 
			{ 
				this.typeLibraryLocation = value;
				
				if ((this.assemblyLocation == null) || (this.assemblyLocation.Length == 0))
				{
					this.UpdateAssemblyLocation();
				}
			}
		}

		public string AssemblyLocation
		{
			get { return this.assemblyLocation; }
			set { this.assemblyLocation = value; }
		}

		public string AssemblyNamespace
		{
			get { return this.assemblyNamespace; }
			set { this.assemblyNamespace = value; }
		}

		public Version AssemblyVersion
		{
			get { return this.assemblyVersion; }
			set { this.assemblyVersion = value; }
		}

		public bool UnsafeInterfaces
		{
			get { return this.unsafeInterfaces; }
			set { this.unsafeInterfaces = value; }
		}

		public bool SafeArrayAsSystemArray
		{
			get { return this.safeArrayAsSystemArray; }
			set { this.safeArrayAsSystemArray = value; }
		}

		public bool PrimaryInteropAssembly
		{
			get { return this.primaryInteropAssembly; }
			set { this.primaryInteropAssembly = value; }
		}

		public byte[] PublicKey
		{
			get { return this.publicKey; }
			set { this.publicKey = value; }
		}

		public StrongNameKeyPair KeyPair
		{
			get { return this.keyPair; }
			set { this.keyPair = value; }
		}

		public void Start()
		{
			if (!File.Exists(assemblyLocation))
			{
				AppDomain appDomain = null;
				try
				{
					appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
					appDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomain_UnhandledException);
					TypeLibraryConverterDomain domain = (TypeLibraryConverterDomain) appDomain.CreateInstanceFromAndUnwrap(typeof(TypeLibraryConverterDomain).Assembly.Location, typeof(TypeLibraryConverterDomain).FullName);
					domain.Message += new TextEventHandler(this.Domain_Message);
					domain.Convert(this.typeLibraryLocation, this.assemblyLocation, this.assemblyNamespace, this.assemblyVersion, this.TypeLibImporterFlags, this.publicKey, this.keyPair);
				}
				finally
				{
					if (appDomain != null)
					{
						AppDomain.Unload(appDomain);
					}
				}
			}
		}

		private void UpdateAssemblyLocation()
		{
			string fileName = null;

			NativeMethods.ITypeLib typeLib = null;
			try
			{
				NativeMethods.LoadTypeLibEx(this.typeLibraryLocation, NativeMethods.RegKind.RegKind_None, out typeLib);
				if (typeLib != null)
				{
					fileName = GetTypeLibName(typeLib);
				}
			}
			catch (COMException)
			{
			}
			finally
			{
				if (typeLib != null)
				{
					Marshal.ReleaseComObject(typeLib);
				}
			}

			if ((fileName == null) || (fileName.Length == 0))
			{
				fileName = Path.GetFileNameWithoutExtension(this.typeLibraryLocation);
				fileName = fileName.Trim().Replace(" ", "");
			}

			this.AssemblyLocation = fileName + ".dll";
		}

		private static string GetTypeLibName(NativeMethods.ITypeLib typeLib)
		{
			string strName = null;
			string strDocString = null;
			int dwHelpContext = 0;
			string strHelpFile = null;
			typeLib.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
			return strName;
		}

		private TypeLibImporterFlags TypeLibImporterFlags
		{
			get
			{
				TypeLibImporterFlags flags = 0;
				
				if (this.PrimaryInteropAssembly)
				{
					flags |= TypeLibImporterFlags.PrimaryInteropAssembly;
				}
	
				if (this.SafeArrayAsSystemArray)
				{
					flags |= TypeLibImporterFlags.SafeArrayAsSystemArray;
				}

				if (this.UnsafeInterfaces)
				{
					flags |= TypeLibImporterFlags.UnsafeInterfaces;
				}

				return flags;
			}
		}

		private void Domain_Message(object sender, TextEventArgs e)
		{
 			if (this.Message != null)
			{
				this.Message(this, e);
			}
		}

		private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			throw e.ExceptionObject as Exception;
		}

		[Serializable]
		internal class TypeLibraryConverterDomain : MarshalByRefObject, ITypeLibImporterNotifySink
		{
			public event TextEventHandler Message;

			private string assemblyPath;

			public void Convert(string typeLibraryLocation, string assemblyLocation, string assemblyNamespace, Version assemblyVersion, TypeLibImporterFlags flags, byte[] publicKey, StrongNameKeyPair keyPair)
			{
				string assemblyFile = Path.GetFileName(assemblyLocation);
				this.assemblyPath = Path.GetDirectoryName(assemblyLocation);

				NativeMethods.ITypeLib typeLib = null;
				try
				{
					if (!Directory.Exists(assemblyPath))
					{
						Directory.CreateDirectory(assemblyPath);
					}

					this.RaiseMessage(string.Format("Loading '{0}'.", typeLibraryLocation));

					NativeMethods.LoadTypeLibEx(typeLibraryLocation, NativeMethods.RegKind.RegKind_None, out typeLib);
					if (typeLib != null)
					{
						this.ConvertTypeLib(typeLib, assemblyPath, assemblyFile, assemblyNamespace, assemblyVersion, flags, publicKey, keyPair);
					}
				}
				catch (Exception exception)
				{
					this.RaiseError(exception.Message);
				}
				finally
				{
					if (typeLib != null)
					{
						Marshal.ReleaseComObject(typeLib);
					}
				}

				this.RaiseMessage("Done.");
			}

			private Assembly ConvertTypeLib(NativeMethods.ITypeLib typeLib, string assemblyPath, string assemblyFile, string assemblyNamespace, Version assemblyVersion, TypeLibImporterFlags flags, byte[] publicKey, StrongNameKeyPair keyPair)
			{
				try
				{
					this.RaiseMessage(string.Format("Converting '{0}'.", GetTypeLibName(typeLib)));

					TypeLibConverter converter = new TypeLibConverter();
					AssemblyBuilder assemblyBuilder = converter.ConvertTypeLibToAssembly(typeLib, Path.Combine(assemblyPath, assemblyFile), flags, this, publicKey, keyPair, assemblyNamespace, assemblyVersion);

					this.RaiseMessage(string.Format("Saving '{0}'.", assemblyFile));

					string currentDirectory = Environment.CurrentDirectory;
					Environment.CurrentDirectory = assemblyPath;
					assemblyBuilder.Save(assemblyFile);
					Environment.CurrentDirectory = currentDirectory;

					return assemblyBuilder;
				}
				catch (COMException exception)
				{
					this.RaiseError(exception.Message);
				}

				return null;
			}

			public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
			{
				this.RaiseMessage(eventMsg);
			}

			public Assembly ResolveRef(object value)
			{
				NativeMethods.ITypeLib typeLib = value as NativeMethods.ITypeLib;
				if (typeLib != null)
				{
					string typeLibName = GetTypeLibName(typeLib);

					string assemblyFile = "Interop." + typeLibName + ".dll";
					if (File.Exists(Path.Combine(assemblyPath, assemblyFile)))
					{
						this.RaiseError(string.Format(CultureInfo.InvariantCulture, "Cannot automatically resolve type library '{0}'.", typeLibName));
						this.RaiseError(string.Format(CultureInfo.InvariantCulture, "File '{0}' already exists.", assemblyFile));
					}
					else
					{
						Assembly assembly = this.ConvertTypeLib(typeLib, this.assemblyPath, assemblyFile, null, null, 0, null, null);
						return assembly;
					}
				}
				else
				{
					this.RaiseError("Unknown reference resolve request.");
				}
				
				return null;
			}

			private void RaiseMessage(string value)
			{
				TextEventArgs e = new TextEventArgs(value);
				this.OnMessage(e);
			}

			private void RaiseError(string value)
			{
				TextEventArgs e = new TextEventArgs("ERROR: " + value);
				this.OnMessage(e);
			}

			private void OnMessage(TextEventArgs e)
			{
				if (this.Message != null)
				{
					this.Message(this, e);
				}
			}
		}
	}

	[Serializable]
	internal delegate void TextEventHandler(object sender, TextEventArgs e);

	[Serializable]
	internal class TextEventArgs : EventArgs
	{
		private string text;

		public TextEventArgs(string text)
		{
			this.text = text;
		}

		public string Text
		{
			get { return this.text; }
		}
	}
}
