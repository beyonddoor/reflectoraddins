namespace Reflector.CodeMetrics
{
	using System;
	using System.IO;
	using Reflector.CodeModel;

	public sealed class ModuleCodeMetric : CodeMetric
	{
		private int stepCount;
		private int currentCount;

		public ModuleCodeMetric() : base("Module", CodeMetricLevel.Module)
		{
		}

		protected override void AddColumns()
		{
			this.AddColumn("Size");
			this.AddColumn("Types");
			this.AddColumn("Abstracts");
			this.AddColumn("EfferentCouplings");
			this.AddColumn("AfferentCouplings");
			this.AddColumn("Abstractness", typeof(double));
			this.AddColumn("Instability", typeof(double));
			this.AddColumn("Distance", typeof(double));
		}

		protected override void InternalCompute(IAssembly[] assemblies)
		{
			this.GetStepCount(assemblies);

			this.currentCount = 0;

			for (int i = 0; i < assemblies.Length; ++i)
			{
				IAssembly assembly = assemblies[i];
				if (!this.IsAbortPending())
				{
					foreach (IModule module in assembly.Modules)
					{
						if (!this.IsAbortPending())
						{
							this.ComputeModule(module);
						}
					}
				}
			}
		}

		private void GetStepCount(IAssembly[] assemblies)
		{
			this.stepCount = 0;

			for (int i = 0; i < assemblies.Length; ++i)
			{
				IAssembly assembly = assemblies[i];
				foreach (IModule module in assembly.Modules)
				{
					this.stepCount += module.Types.Count;
				}
			}
		}

		private void ComputeModule(IModule module)
		{
			int abstracts = 0;
			int eCoupling = 0;
			int aCoupling = 0;

			foreach (IType type in module.Types)
			{
				if (!this.IsAbortPending())
				{
					this.OnProgress(new ComputationProgressEventArgs(currentCount++, this.stepCount));

					ITypeReference typeReference = type as ITypeReference;
					if (typeReference != null)
					{
						ITypeDeclaration typeDeclaration = typeReference.Resolve();
						if (typeDeclaration != null)
						{
							if (typeDeclaration.Abstract || typeDeclaration.Interface)
							{
								abstracts++;
							}

							if (IsEfferent(module, typeDeclaration))
							{
								eCoupling++;
							}

							if (IsAfferent(module, typeDeclaration))
							{
								aCoupling++;
							}
						}
					}
				}
			}

			double abstractness = abstracts / (double)module.Types.Count;
			double instability = 0;
			if (eCoupling + aCoupling > 0)
			{
				instability = eCoupling / (double)(eCoupling + aCoupling);
			}

			double distance = Math.Abs((abstractness + instability - 1) / 2.0);

			string location = Environment.ExpandEnvironmentVariables(module.Location);
			int fileSize = (File.Exists(location)) ? (int)new FileInfo(location).Length : 0;

			this.AddRow(module, fileSize, module.Types.Count, abstracts, eCoupling, aCoupling, abstractness, instability, distance);
		}

		private bool IsEfferent(IModule module, ITypeDeclaration typeDeclaration)
		{
			foreach (IMethodDeclaration methodDeclaration in typeDeclaration.Methods)
			{
				foreach (IParameterDeclaration parameterDeclaration in methodDeclaration.Parameters)
				{
					IModule parameterModule = GetModule(parameterDeclaration.ParameterType as ITypeReference);
					if ((parameterModule != null) && (parameterModule == module))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsAfferent(IModule module, ITypeDeclaration typeDeclaration)
		{
			foreach (IMethodDeclaration methodDeclaration in typeDeclaration.Methods)
			{
				foreach (IParameterDeclaration parameterDeclaration in methodDeclaration.Parameters)
				{
					IModule parameterModule = GetModule(parameterDeclaration.ParameterType as ITypeReference);
					if ((parameterModule != null) && (parameterModule != module))
					{
						return true;
					}
				}
			}
			return false;
		}

		private IModule GetModule(ITypeReference typeReference)
		{
			if (typeReference != null)
			{
				if (typeReference.GenericType != null)
				{
					typeReference = typeReference.GenericType;
				}

				ITypeDeclaration typeDeclaration = typeReference.Resolve();
				if (typeDeclaration != null)
				{
					do
					{
						IModule module = typeDeclaration.Owner as IModule;
						if (module != null)
						{
							return module;
						}

						typeDeclaration = typeDeclaration.Owner as ITypeDeclaration;
					}
					while (typeDeclaration != null);
				}
			}
			return null;
		}
	}
}
