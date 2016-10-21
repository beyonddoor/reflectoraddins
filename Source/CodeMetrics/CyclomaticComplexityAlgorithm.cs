namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using Reflector.CodeModel;

	internal sealed class CyclomaticComplexityAlgorithm
    {
        private FlowToCodeConverter flowConverter = new FlowToCodeConverter();

        public int Compute(IMethodBody body)
        {
            int cyclo = 0;
            foreach (IInstruction instruction in body.Instructions)
            {
                System.Reflection.Emit.FlowControl flow =
                    this.flowConverter.Convert(instruction.Code);
                if (flow == System.Reflection.Emit.FlowControl.Cond_Branch)
                    cyclo++;
            }

            return cyclo + 1;
        }

        public int Compute(ITypeDeclaration type)
        {
            int cyclo = 0;
            foreach (IMethodDeclaration method in type.Methods)
            {
                IMethodBody body = method.Body as IMethodBody;
                if (body == null)
                    return -1;
                cyclo += Compute(body);
            }
            return cyclo;
        }

		private sealed class FlowToCodeConverter
		{
			private Hashtable codeFlows = new Hashtable();
			public FlowToCodeConverter()
			{
				foreach (System.Reflection.FieldInfo fi in typeof(System.Reflection.Emit.OpCodes).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
				{
					System.Reflection.Emit.OpCode code = (System.Reflection.Emit.OpCode)fi.GetValue(null);
					this.codeFlows[(int)code.Value] = code.FlowControl;
				}
			}

			public System.Reflection.Emit.FlowControl Convert(int code)
			{
				Object o = this.codeFlows[code];
				if (o == null)
				{
					return System.Reflection.Emit.FlowControl.Meta;
				}

				//	throw new Exception(String.Format("code.Value {0} not found",code.Value));

				return (System.Reflection.Emit.FlowControl)o;
			}
		}
    }
}
