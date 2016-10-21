
	using System;
	using System.Collections;
	using System.IO;
	using System.Globalization;
	using Reflector;
	using Reflector.CodeModel;

	// This example shows how to use the .NET Reflector Code Model to load an assembly and enumerate its types, methods and IL code.

	internal class CodeModelExample
	{
		public static void Main()
		{
			IServiceProvider serviceProvider = new ApplicationManager(null);

			IAssemblyManager assemblyManager = (IAssemblyManager) serviceProvider.GetService(typeof(IAssemblyManager));
			
			IAssembly assembly = assemblyManager.LoadFile(typeof(CodeModelExample).Module.FullyQualifiedName);

			Console.WriteLine("Assembly: " + assembly.ToString());

			foreach (IModule module in assembly.Modules)
			{
				Console.WriteLine("Module: " + module.Name);
					
				foreach (ITypeDeclaration typeDeclaration in module.Types)
				{
					Console.WriteLine("Type: " + typeDeclaration.Namespace + "." + typeDeclaration.Name);
					
					foreach (IMethodDeclaration methodDeclaration in typeDeclaration.Methods)
					{
						Console.WriteLine("Method: " + methodDeclaration);	
						
						IMethodBody methodBody = methodDeclaration.Body as IMethodBody;
						if (methodBody != null)
						{
							foreach (IInstruction instruction in methodBody.Instructions)
							{
								Console.Write("L" + instruction.Offset.ToString("X4", CultureInfo.InvariantCulture));
								Console.Write(": ");
								Console.Write(InstructionTable.GetInstructionName(instruction.Code));

								if (instruction.Value != null)
								{
									Console.Write(" ");

									if (instruction.Value is string)
									{
										Console.Write("\"");
									}

									Console.Write(instruction.Value.ToString());

									if (instruction.Value is string)
									{
										Console.Write("\"");
									}
								}


								Console.WriteLine();
							}	
						}
					}
				}
			}
			
			assemblyManager.Unload(assembly);
		}
	}
