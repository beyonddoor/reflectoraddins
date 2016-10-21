namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using System.Globalization;
	using System.IO;
	using Reflector.CodeModel;

	internal class CommandLineProcessor
	{
		private IWindowManager windowManager;
		private IAssemblyManager assemblyManager;
		private IAssemblyCache assemblyCache;

		public CommandLineProcessor(IWindowManager windowManager, IAssemblyManager assemblyManager, IAssemblyCache assemblyCache)
		{
			this.windowManager = windowManager;
			this.assemblyManager = assemblyManager;
			this.assemblyCache = assemblyCache;
		}

		public bool CommandLineMode
		{
			get
			{
				string[] run = this.GetArguments("Run");
				if ((run != null) && (Array.IndexOf(run, this.GetType().Assembly.GetName().Name) != -1))
				{
					string outputPath = this.GetArgument("OutputPath");
					if ((outputPath != null) && (outputPath.Length > 0))
					{
						return true;
					}
				}

				return false;
			}
		}

		public void Run()
		{
			this.windowManager.Windows["CodeMetricWindow"].Visible = true;
			System.Windows.Forms.Application.DoEvents();

			CodeMetricWindow codeMetricWindow = (CodeMetricWindow)this.windowManager.Windows["CodeMetricWindow"].Content;
			codeMetricWindow.ClearAssemblyList();

			string[] assemblyFiles = this.GetArguments("Assembly");
			foreach (string assemblyFile in assemblyFiles)
			{
				IAssembly assembly = this.assemblyManager.LoadFile(assemblyFile);
				if (assembly != null)
				{
					codeMetricWindow.CodeMetricManager.AddAssembly(assembly);
				}
			}

			codeMetricWindow.StartAnalysis();

			while (codeMetricWindow.IsAnalyzing())
			{
				System.Windows.Forms.Application.DoEvents();
			}

			string outputPath = this.GetArgument("OutputPath");
			if ((outputPath != null) && (outputPath.Length > 0))
			{
				using (TextWriter textWriter = File.CreateText(outputPath))
				{
					codeMetricWindow.CodeMetricManager.Save(textWriter);
				}
			}

			this.windowManager.Windows["CodeMetricWindow"].Visible = false;
			System.Windows.Forms.Application.DoEvents();
		}

		private string GetArgument(string name)
		{
			string[] arguments = this.GetArguments(name);
			if (arguments != null)
			{
				if (arguments.Length != 1)
				{
					throw new InvalidOperationException();
				}

				return arguments[0];
			}

			return null;
		}

		private string[] GetArguments(string name)
		{
			name = name.ToLower(CultureInfo.InvariantCulture);

			ArrayList list = new ArrayList(0);

			string[] arguments = Environment.GetCommandLineArgs();
			for (int i = 1; i < arguments.Length; i++)
			{
				string argument = arguments[i];
				string argumentName = string.Empty;
				string argumentValue = string.Empty;

				if ((argument[0] != '/') && (argument[0] != '-'))
				{
					argumentValue = argument;
				}
				else
				{
					int index = argument.IndexOf(':');

					if (index == -1)
					{
						// "-option" without value
						argumentName = argument.Substring(1).ToLower(CultureInfo.InvariantCulture);

						// Turn '-?' into '-help'
						if (argumentName == "?")
						{
							argumentName = "help";
						}
					}
					else
					{
						// "-option:value"
						argumentName = argument.Substring(1, index - 1).ToLower(CultureInfo.InvariantCulture);
						argumentValue = argument.Substring(index + 1);
					}
				}

				// Add value
				if ((argumentName.Length != 0) && (name.StartsWith(argumentName)))
				{

					list.Add(argumentValue);
				}

				if ((argumentName.Length == 0) && (name.Length == 0))
				{
					list.Add(argumentValue);
				}
			}

			if (list.Count != 0)
			{
				string[] array = new string[list.Count];
				list.CopyTo(array, 0);
				return array;
			}

			return null;
		}
	}
}
