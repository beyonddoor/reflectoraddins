namespace Reflector.CodeMetrics
{
	using System;
	using System.Reflection.Emit;
	using Reflector.CodeModel;

	public sealed class MethodCodeMetric : CodeMetric
    {
        private CyclomaticComplexityAlgorithm cyclo = new CyclomaticComplexityAlgorithm();

		public MethodCodeMetric() : base("Method", CodeMetricLevel.Method)
        { 
		}

        protected override void AddColumns()
        {
			this.AddColumn("CodeSize", typeof(int));
			this.AddColumn("CyclomaticComplexity", typeof(int));
            this.AddColumn("Instructions", typeof(int));
            this.AddColumn("Locals", typeof(int));
            this.AddColumn("MaxStack", typeof(int));
            this.AddColumn("ExceptionHandlers", typeof(int));
            this.AddColumn("Throw", typeof(int));
            this.AddColumn("NewObj", typeof(int));
            this.AddColumn("Ret", typeof(int));
            this.AddColumn("CastClass", typeof(int));
        }

        protected override void InternalCompute(IAssembly[] assemblies)
        {
            int stepCount = this.GetStepCount(assemblies);
            int count = 0;

			for (int i = 0; i < assemblies.Length; ++i)
            {
				IAssembly assembly = assemblies[i];
				if (!this.IsAbortPending())
				{
					foreach (IModule module in assembly.Modules)
					{
						if (!this.IsAbortPending())
						{
							foreach (ITypeDeclaration type in module.Types)
							{
								this.OnProgress(new ComputationProgressEventArgs(count++, stepCount));
								foreach (IMethodDeclaration method in type.Methods)
								{
									if (!this.IsAbortPending())
									{
										this.ComputeMethod(method);
									}
								}
							}
						}
					}
				}
            }
        }

        private int GetStepCount(IAssembly[] assemblies)
        {
            int count = 0;

			for (int i = 0; i < assemblies.Length; ++i)
            {
				IAssembly assembly = assemblies[i];
                foreach (IModule module in assembly.Modules)
                {
                    count += module.Types.Count;
                }
            }
            return count;
        }

        private void ComputeMethod(IMethodDeclaration value)
        {
			IMethodBody methodBody = value.Body as IMethodBody;
			if (methodBody != null)
			{
				int cyclo = this.cyclo.Compute(methodBody);
				int codeSize = Helper.GetMethodBodySize(value);
				this.AddRow(value, codeSize, cyclo, methodBody.Instructions.Count, methodBody.LocalVariables.Count, methodBody.MaxStack, methodBody.ExceptionHandlers.Count, CountOperand(methodBody, OpCodes.Throw.Value), CountOperand(methodBody, OpCodes.Newobj.Value), CountOperand(methodBody, OpCodes.Ret.Value), CountOperand(methodBody, OpCodes.Castclass.Value));
			}
        }

        private int CountOperand(IMethodBody methodBody, int code)
        {
            int count = 0;

			foreach (IInstruction instruction in methodBody.Instructions)
            {
				if (instruction.Code == code)
				{
					count++;
				}
            }

            return count;
        }
    }
}
