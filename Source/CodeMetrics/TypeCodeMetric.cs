namespace Reflector.CodeMetrics
{
	using System;
	using Reflector.CodeModel;

	public sealed class TypeCodeMetric : CodeMetric
	{
		private CyclomaticComplexityAlgorithm cyclomaticComplexityAlgorithm = new CyclomaticComplexityAlgorithm();
		private int stepCount;

		public TypeCodeMetric() : base("Type", CodeMetricLevel.Type)
		{
		}

		protected override void AddColumns()
		{
			this.AddColumn("CodeSize");
			this.AddColumn("WeightedCyclomaticComplexity");
			this.AddColumn("Fields");
			this.AddColumn("Methods");
			this.AddColumn("Properties");
			this.AddColumn("Events");
			this.AddColumn("NestedTypes");
			this.AddColumn("Attributes");
			this.AddColumn("DepthInTree");
		}

		protected override void InternalCompute(IAssembly[] assemblies)
		{
			this.GetStepCount(assemblies);
			int count = 0;

			for (int i = 0; i < assemblies.Length; i++)
			{
				IAssembly assembly = assemblies[i];
				foreach (IModule module in assembly.Modules)
				{
					foreach (ITypeDeclaration type in module.Types)
					{
						if (!this.IsAbortPending())
						{
							this.OnProgress(new ComputationProgressEventArgs(count++, stepCount));
							this.ComputeType(type);
						}
					}
				}
			}
		}

		private void GetStepCount(IAssembly[] assemblies)
		{
			this.stepCount = 0;

			for (int i = 0; i < assemblies.Length; i++)
			{
				IAssembly assembly = assemblies[i];
				foreach (IModule module in assembly.Modules)
				{
					this.stepCount += module.Types.Count;
				}
			}
		}

		private void ComputeType(ITypeDeclaration type)
		{
			int depth = ComputeDepth(type);
			int cyclo = this.cyclomaticComplexityAlgorithm.Compute(type);
			int codeSize = ComputeCodeSize(type);
			this.AddRow(type, codeSize, cyclo, type.Fields.Count, type.Methods.Count, type.Properties.Count, type.Events.Count, type.NestedTypes.Count, type.Attributes.Count, depth);
		}

		private int ComputeCodeSize(ITypeDeclaration value)
		{
			int size = 0;

			foreach (IMethodDeclaration methodDeclaration in value.Methods)
			{
				size += Helper.GetMethodBodySize(methodDeclaration);
			}

			foreach (ITypeDeclaration nestedTypes in value.NestedTypes)
			{
				size = size + this.ComputeCodeSize(nestedTypes);
			}

			return size;
		}

		private int ComputeDepth(ITypeDeclaration type)
		{
			int depth = 0;
			ITypeDeclaration current = type;
			while (current.BaseType != null)
			{
				depth++;

				// Fix #411: This can be null if base type cannot be resolved.

				ITypeReference baseType = current.BaseType;
				if (baseType == null)
				{
					return -1;
				}

				current = baseType.Resolve();
				if (current == null)
				{
					return -1;
				}
			}

			return depth;
		}
	}
}
