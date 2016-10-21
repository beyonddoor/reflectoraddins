namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using System.Globalization;
	using System.IO;
	using Reflector.CodeModel;

	public sealed class Helper
	{
		private Helper()
		{
		}

		public static string GetName(ITypeReference value)
		{
			if (value != null)
			{
				ITypeCollection genericParameters = value.GenericArguments;
				if (genericParameters.Count > 0)
				{
					using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
					{
						for (int i = 0; i < genericParameters.Count; i++)
						{
							if (i != 0)
							{
								writer.Write(",");
							}

							IType genericParameter = genericParameters[i];
							if (genericParameter != null)
							{
								writer.Write(genericParameter.ToString());
							}
						}

						return value.Name + "<" + writer.ToString() + ">";
					}
				}

				return value.Name;
			}

			throw new NotSupportedException();
		}

		public static string GetNameWithResolutionScope(ITypeReference value)
		{
			if (value != null)
			{
				ITypeReference declaringType = value.Owner as ITypeReference;
				if (declaringType != null)
				{
					return Helper.GetNameWithResolutionScope(declaringType) + "+" + Helper.GetName(value);
				}

				string namespaceName = value.Namespace;
				if (namespaceName.Length == 0)
				{
					return Helper.GetName(value);
				}

				return namespaceName + "." + Helper.GetName(value);
			}

			throw new NotSupportedException();
		}

		public static string GetResolutionScope(ITypeReference value)
		{
			IModule module = value.Owner as IModule;
			if (module != null)
			{
				return value.Namespace;
			}

			ITypeDeclaration declaringType = value.Owner as ITypeDeclaration;
			if (declaringType != null)
			{
				return Helper.GetResolutionScope(declaringType) + "+" + Helper.GetName(declaringType);
			}

			throw new NotSupportedException();
		}

		public static bool IsValueType(ITypeReference value)
		{
			if (value != null)
			{
				ITypeDeclaration typeDeclaration = value.Resolve();
				if (typeDeclaration == null)
				{
					return false;
				}

				// TODO
				ITypeReference baseType = typeDeclaration.BaseType;
				return ((baseType != null) && ((baseType.Name == "ValueType") || (baseType.Name == "Enum")) && (baseType.Namespace == "System"));
			}

			return false;
		}

		public static bool IsDelegate(ITypeReference value)
		{
			if (value != null)
			{
				// TODO
				if ((value.Name == "MulticastDelegate") && (value.Namespace == "System"))
				{
					return false;
				}

				ITypeDeclaration typeDeclaration = value.Resolve();
				if (typeDeclaration == null)
				{
					return false;
				}

				ITypeReference baseType = typeDeclaration.BaseType;
				return ((baseType != null) && (baseType.Namespace == "System") && ((baseType.Name == "MulticastDelegate") || (baseType.Name == "Delegate")) && (baseType.Namespace == "System"));
			}

			return false;
		}

		public static bool IsEnumeration(ITypeReference value)
		{
			if (value != null)
			{
				ITypeDeclaration typeDeclaration = value.Resolve();
				if (typeDeclaration == null)
				{
					return false;
				}

				// TODO
				ITypeReference baseType = typeDeclaration.BaseType;
				return ((baseType != null) && (baseType.Name == "Enum") && (baseType.Namespace == "System"));
			}

			return false;
		}

		public static IAssemblyReference GetAssemblyReference(IType value)
		{
			ITypeReference typeReference = value as ITypeReference;
			if (typeReference != null)
			{
				ITypeReference declaringType = typeReference.Owner as ITypeReference;
				if (declaringType != null)
				{
					return GetAssemblyReference(declaringType);
				}

				IModuleReference moduleReference = typeReference.Owner as IModuleReference;
				if (moduleReference != null)
				{
					IModule module = moduleReference.Resolve();
					return module.Assembly;
				}

				IAssemblyReference assemblyReference = typeReference.Owner as IAssemblyReference;
				if (assemblyReference != null)
				{
					return assemblyReference;
				}
			}

			throw new NotSupportedException();
		}

		public static bool IsVisible(IType value, IVisibilityConfiguration visibility)
		{
			ITypeReference typeReference = value as ITypeReference;
			if (typeReference != null)
			{
				ITypeReference declaringType = typeReference.Owner as ITypeReference;
				if (declaringType != null)
				{
					if (!Helper.IsVisible(declaringType, visibility))
					{
						return false;
					}
				}

				ITypeDeclaration typeDeclaration = typeReference.Resolve();
				if (typeDeclaration == null)
				{
					return true;
				}

				switch (typeDeclaration.Visibility)
				{
					case TypeVisibility.Public:
					case TypeVisibility.NestedPublic:
						return visibility.Public;

					case TypeVisibility.Private:
					case TypeVisibility.NestedPrivate:
						return visibility.Private;

					case TypeVisibility.NestedFamilyOrAssembly:
						return visibility.FamilyOrAssembly;

					case TypeVisibility.NestedFamily:
						return visibility.Family;

					case TypeVisibility.NestedFamilyAndAssembly:
						return visibility.FamilyAndAssembly;

					case TypeVisibility.NestedAssembly:
						return visibility.Assembly;

					default:
						throw new NotImplementedException();
				}
			}

			throw new NotSupportedException();
		}

		public static IMethodDeclaration GetMethod(ITypeDeclaration value, string methodName)
		{
			IMethodDeclarationCollection methods = value.Methods;
			for (int i = 0; i < methods.Count; i++)
			{
				if (methodName == methods[i].Name)
				{
					return methods[i];
				}
			}

			return null;
		}
 
		private static ICollection GetInterfaces(ITypeDeclaration value)
		{
			ArrayList list = new ArrayList(0);

			list.AddRange(value.Interfaces);

			if (value.BaseType != null)
			{
				ITypeDeclaration baseType = value.BaseType.Resolve();
				foreach (ITypeReference interfaceReference in baseType.Interfaces)
				{
					if (list.Contains(interfaceReference))
					{
						list.Remove (interfaceReference);
					}
				}
			}

			foreach (ITypeReference interfaceReference in value.Interfaces)
			{
				ITypeDeclaration interfaceDeclaration = interfaceReference.Resolve();
				foreach (ITypeReference interfaceBaseReference in interfaceDeclaration.Interfaces)
				{
					if (list.Contains(interfaceBaseReference))
					{
						list.Remove(interfaceBaseReference);
					}
				}
			}

			ITypeReference[] array = new ITypeReference[list.Count];
			list.CopyTo (array, 0);
			return array;
		}

		public static ICollection GetInterfaces(ITypeDeclaration value, IVisibilityConfiguration visibility)
		{
			ArrayList list = new ArrayList(0);

			foreach (ITypeReference typeReference in GetInterfaces(value))
			{
				if (Helper.IsVisible(typeReference, visibility))
				{
					list.Add(typeReference);
				}
			}
			
			list.Sort();	
			return list;
		}

		public static ICollection GetFields(ITypeDeclaration value, IVisibilityConfiguration visibility)
		{
			ArrayList list = new ArrayList(0);
	
			IFieldDeclarationCollection fields = value.Fields;
			if (fields.Count > 0)
			{
				foreach (IFieldDeclaration fieldDeclaration in fields)
				{
					if ((visibility == null) || (Helper.IsVisible(fieldDeclaration, visibility)))
					{
						list.Add(fieldDeclaration);
					}
				}

				list.Sort();
			}

			return list;
		}

		public static ICollection GetMethods(ITypeDeclaration value, IVisibilityConfiguration visibility)
		{
			ArrayList list = new ArrayList(0);

			IMethodDeclarationCollection methods = value.Methods;

			if (methods.Count > 0)
			{
				foreach (IMethodDeclaration methodDeclaration in methods)
				{
					if ((visibility == null) || (Helper.IsVisible(methodDeclaration, visibility)))
					{
						list.Add(methodDeclaration);
					}
				}

				foreach (IPropertyDeclaration propertyDeclaration in value.Properties)
				{
					if (propertyDeclaration.SetMethod != null)
					{
						list.Remove(propertyDeclaration.SetMethod.Resolve());
					}

					if (propertyDeclaration.GetMethod != null)
					{
						list.Remove(propertyDeclaration.GetMethod.Resolve());
					}
				}

				foreach (IEventDeclaration eventDeclaration in value.Events)
				{
					if (eventDeclaration.AddMethod != null)
					{
						list.Remove(eventDeclaration.AddMethod.Resolve());
					}

					if (eventDeclaration.RemoveMethod != null)
					{
						list.Remove(eventDeclaration.RemoveMethod.Resolve());
					}

					if (eventDeclaration.InvokeMethod != null)
					{
						list.Remove(eventDeclaration.InvokeMethod.Resolve());
					}
				}

				list.Sort();
			}

			return list;
		}
		
		public static ICollection GetProperties(ITypeDeclaration value, IVisibilityConfiguration visibility)
		{
			ArrayList list = new ArrayList(0);

			IPropertyDeclarationCollection properties = value.Properties;
			if (properties.Count > 0)
			{
				foreach (IPropertyDeclaration propertyDeclaration in properties)
				{
					if ((visibility == null) || (Helper.IsVisible(propertyDeclaration, visibility)))
					{
						list.Add(propertyDeclaration);
					}
				}

				list.Sort();
			}

			return list;
		}

		public static ICollection GetEvents(ITypeDeclaration value, IVisibilityConfiguration visibility)
		{
			ArrayList list = new ArrayList(0);

			IEventDeclarationCollection events = value.Events;
			if (events.Count > 0)
			{
				foreach (IEventDeclaration eventDeclaration in events)
				{
					if ((visibility == null) || (Helper.IsVisible(eventDeclaration, visibility)))
					{
						list.Add(eventDeclaration);
					}
				}

				list.Sort();
			}

			return list;
		}

		public static ICollection GetNestedTypes(ITypeDeclaration value, IVisibilityConfiguration visibility)
		{
			ArrayList list = new ArrayList(0);

			ITypeDeclarationCollection nestedTypes = value.NestedTypes;
			if (nestedTypes.Count > 0)
			{
				foreach (ITypeDeclaration nestedType in nestedTypes)
				{
					if (Helper.IsVisible(nestedType, visibility))
					{
						list.Add(nestedType);
					}
				}

				list.Sort();
			}

			return list;
		}

		public static string GetName(IFieldReference value)
		{
			IType fieldType = value.FieldType;
			IType declaringType = value.DeclaringType;
			if (fieldType.Equals(declaringType))
			{
				ITypeReference typeReference = fieldType as ITypeReference;
				if (typeReference != null)
				{
					if (Helper.IsEnumeration(typeReference))
					{
						return value.Name;
					}
				}
			}

			return value.Name + " : " + value.FieldType.ToString();
		}

		public static string GetNameWithDeclaringType(IFieldReference value)
		{
			return Helper.GetNameWithResolutionScope(value.DeclaringType as ITypeReference) + "." + GetName(value);
		}

		public static bool IsVisible(IFieldReference value, IVisibilityConfiguration visibility)
		{
			if (Helper.IsVisible(value.DeclaringType, visibility))
			{
				IFieldDeclaration fieldDeclaration = value.Resolve();
				if (fieldDeclaration == null)
				{
					return true;
				}

				switch (fieldDeclaration.Visibility)
				{
					case FieldVisibility.Public:
						return visibility.Public;

					case FieldVisibility.Assembly:
						return visibility.Assembly;

					case FieldVisibility.FamilyOrAssembly:
						return visibility.FamilyOrAssembly;

					case FieldVisibility.Family:
						return visibility.Family;

					case FieldVisibility.Private:
						return visibility.Private;

					case FieldVisibility.FamilyAndAssembly:
						return visibility.FamilyAndAssembly;

					case FieldVisibility.PrivateScope:
						return visibility.Private;
				}

				throw new NotSupportedException();
			}

			return false;
		}

		public static string GetName(IMethodReference value)
		{
			ITypeCollection genericArguments = value.GenericArguments;
			if (genericArguments.Count > 0)
			{
				using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					for (int i = 0; i < genericArguments.Count; i++)
					{
						if (i != 0)
						{
							writer.Write(", ");
						}

						IType genericArgument = genericArguments[i];
						if (genericArgument != null)
						{
							writer.Write(genericArgument.ToString());
						}
						else
						{
							writer.Write("???");
						}
					}

					return value.Name + "<" + writer.ToString() + ">";
				}
			}

			return value.Name;
		}

		public static string GetNameWithParameterList(IMethodReference value)
		{
			using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				writer.Write(Helper.GetName(value));
				writer.Write("(");

				IParameterDeclarationCollection parameters = value.Parameters;
				for (int i = 0; i < parameters.Count; i++)
				{
					if (i != 0)
					{
						writer.Write(", ");
					}

					writer.Write(parameters[i].ParameterType.ToString());
				}

				if (value.CallingConvention == MethodCallingConvention.VariableArguments)
				{
					if (value.Parameters.Count > 0)
					{
						writer.Write(", ");
					}

					writer.Write("...");
				}

				writer.Write(")");

				if ((value.Name != ".ctor") && (value.Name != ".cctor"))
				{
					writer.Write(" : ");
					writer.Write(value.ReturnType.Type.ToString());
				}

				return writer.ToString();
			}
		}

		public static string GetNameWithDeclaringType(IMethodReference value)
		{
			ITypeReference typeReference = value.DeclaringType as ITypeReference;
			if (typeReference != null)
			{
				return Helper.GetNameWithResolutionScope(typeReference) + "." + Helper.GetNameWithParameterList(value);
			}

			IArrayType arrayType = value.DeclaringType as IArrayType;
			if (arrayType != null)
			{
				return arrayType.ToString() + "." + Helper.GetNameWithParameterList(value);
			}

			throw new NotSupportedException();
		}

		public static bool IsVisible(IMethodReference value, IVisibilityConfiguration visibility)
		{
			if (Helper.IsVisible(value.DeclaringType, visibility))
			{
				IMethodDeclaration methodDeclaration = value.Resolve();
				switch (methodDeclaration.Visibility)
				{
					case MethodVisibility.Public:
						return visibility.Public;

					case MethodVisibility.Assembly:
						return visibility.Assembly;

					case MethodVisibility.FamilyOrAssembly:
						return visibility.FamilyOrAssembly;

					case MethodVisibility.Family:
						return visibility.Family;

					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
						return visibility.Private;

					case MethodVisibility.FamilyAndAssembly:
						return visibility.FamilyAndAssembly;
				}

				throw new NotSupportedException();
			}

			return false;
		}

		public static string GetName(IPropertyReference value)
		{
			IParameterDeclarationCollection parameters = value.Parameters;
			if (parameters.Count > 0)
			{
				using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					for (int i = 0; i < parameters.Count; i++)
					{
						if (i != 0)
						{
							writer.Write(", ");
						}

						writer.Write(parameters[i].ParameterType.ToString());
					}

					return value.Name + "[" + writer.ToString() + "] : " + value.PropertyType.ToString();
				}
			}

			return value.Name + " : " + value.PropertyType.ToString();
		}

		public static string GetNameWithDeclaringType(IPropertyReference value)
		{
			return Helper.GetNameWithResolutionScope(value.DeclaringType as ITypeReference) + "." + Helper.GetName(value);
		}

		public static IMethodDeclaration GetSetMethod(IPropertyReference value)
		{
			IPropertyDeclaration propertyDeclaration = value.Resolve();
			if (propertyDeclaration.SetMethod != null)
			{
				return propertyDeclaration.SetMethod.Resolve();
			}

			return null;
		}

		public static IMethodDeclaration GetGetMethod(IPropertyReference value)
		{
			IPropertyDeclaration propertyDeclaration = value.Resolve();
			if (propertyDeclaration.GetMethod != null)
			{
				return propertyDeclaration.GetMethod.Resolve();
			}

			return null;
		}

		public static bool IsStatic(IPropertyReference value)
		{
			IMethodDeclaration setMethod = Helper.GetSetMethod(value);
			IMethodDeclaration getMethod = Helper.GetGetMethod(value);
			bool isStatic = false;

			isStatic |= ((setMethod != null) && (setMethod.Static));
			isStatic |= ((getMethod != null) && (getMethod.Static));
			return isStatic;
		}

		public static MethodVisibility GetVisibility(IPropertyReference value)
		{
			IMethodDeclaration getMethod = Helper.GetGetMethod(value);
			IMethodDeclaration setMethod = Helper.GetSetMethod(value);

			MethodVisibility visibility = MethodVisibility.Public;

			if ((setMethod != null) && (getMethod != null))
			{
				if (getMethod.Visibility == setMethod.Visibility)
				{
					visibility = getMethod.Visibility;
				}
			}
			else if (setMethod != null)
			{
				visibility = setMethod.Visibility;
			}
			else if (getMethod != null)
			{
				visibility = getMethod.Visibility;
			}

			return visibility;
		}

		public static bool IsVisible(IPropertyReference value, IVisibilityConfiguration visibility)
		{
			if (Helper.IsVisible(value.DeclaringType, visibility))
			{
				switch (Helper.GetVisibility(value))
				{
					case MethodVisibility.Public:
						return visibility.Public;

					case MethodVisibility.Assembly:
						return visibility.Assembly;

					case MethodVisibility.FamilyOrAssembly:
						return visibility.FamilyOrAssembly;

					case MethodVisibility.Family:
						return visibility.Family;

					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
						return visibility.Private;

					case MethodVisibility.FamilyAndAssembly:
						return visibility.FamilyAndAssembly;
				}

				throw new NotSupportedException();
			}

			return false;
		}

		public static string GetName(IEventReference value)
		{
			return value.Name;
		}

		public static string GetNameWithDeclaringType(IEventReference value)
		{
			return Helper.GetNameWithResolutionScope(value.DeclaringType as ITypeReference) + "." + Helper.GetName(value);
		}

		public static IMethodDeclaration GetAddMethod(IEventReference value)
		{
			IEventDeclaration eventDeclaration = value.Resolve();
			if (eventDeclaration.AddMethod != null)
			{
				return eventDeclaration.AddMethod.Resolve ();
			}

			return null;
		}

		public static IMethodDeclaration GetRemoveMethod(IEventReference value)
		{
			IEventDeclaration eventDeclaration = value.Resolve();
			if (eventDeclaration.RemoveMethod != null)
			{
				return eventDeclaration.RemoveMethod.Resolve();
			}

			return null;
		}

		public static IMethodDeclaration GetInvokeMethod(IEventReference value)
		{
			IEventDeclaration eventDeclaration = value.Resolve();
			if (eventDeclaration.InvokeMethod != null)
			{
				return eventDeclaration.InvokeMethod.Resolve ();
			}

			return null;
		}

		public static MethodVisibility GetVisibility(IEventReference value)
		{
			IMethodDeclaration addMethod = Helper.GetAddMethod(value);
			IMethodDeclaration removeMethod = Helper.GetRemoveMethod(value);
			IMethodDeclaration invokeMethod = Helper.GetInvokeMethod(value);

			if ((addMethod != null) && (removeMethod != null) && (invokeMethod != null))
			{
				if ((addMethod.Visibility == removeMethod.Visibility) && (addMethod.Visibility == invokeMethod.Visibility))
				{
					return addMethod.Visibility;
				}
			}
			else if ((addMethod != null) && (removeMethod != null))
			{
				if (addMethod.Visibility == removeMethod.Visibility)
				{
					return addMethod.Visibility;
				}
			}
			else if ((addMethod != null) && (invokeMethod != null))
			{
				if (addMethod.Visibility == invokeMethod.Visibility)
				{
					return addMethod.Visibility;
				}
			}
			else if ((removeMethod != null) && (invokeMethod != null))
			{
				if (removeMethod.Visibility == invokeMethod.Visibility)
				{
					return removeMethod.Visibility;
				}
			}
			else if (addMethod != null)
			{
				return addMethod.Visibility;
			}
			else if (removeMethod != null)
			{
				return removeMethod.Visibility;
			}
			else if (invokeMethod != null)
			{
				return invokeMethod.Visibility;
			}

			return MethodVisibility.Public;
		}

		public static bool IsVisible(IEventReference value, IVisibilityConfiguration visibility)
		{
			if (Helper.IsVisible(value.DeclaringType, visibility))
			{
				switch (Helper.GetVisibility(value))
				{
					case MethodVisibility.Public :
						return visibility.Public;

					case MethodVisibility.Assembly :
						return visibility.Assembly;

					case MethodVisibility.FamilyOrAssembly :
						return visibility.FamilyOrAssembly;

					case MethodVisibility.Family :
						return visibility.Family;

					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
						return visibility.Private;

					case MethodVisibility.FamilyAndAssembly :
						return visibility.FamilyAndAssembly;
				}

				throw new NotSupportedException();
			}

			return false;
		}

		public static bool IsStatic(IEventReference value)
		{
			bool isStatic = false;

			if (Helper.GetAddMethod(value) != null)
			{
				isStatic |= Helper.GetAddMethod(value).Static;
			}

			if (Helper.GetRemoveMethod(value) != null)
			{
				isStatic |= Helper.GetRemoveMethod(value).Static;
			}

			if (Helper.GetInvokeMethod(value) != null)
			{
				isStatic |= Helper.GetInvokeMethod(value).Static;
			}

			return isStatic;
		}

		public static int GetInstructionSize(IInstruction value)
		{
			int size = 0;

			if (value.Code < 0x100)
			{
				size += 1;
			}
			else if (value.Code < 0x10000)
			{
				size += 2;
			}

			switch (GetOperandType(value.Code))
			{
				case OperandType.None:
					break;

				case OperandType.ShortBranchTarget:
				case OperandType.SByte:
				case OperandType.ShortVariable:
					size += 1;
					break;

				case OperandType.Variable:
					size += 2;
					break;

				case OperandType.BranchTarget:
				case OperandType.Int32:
				case OperandType.Single:
				case OperandType.String:
				case OperandType.Signature:
				case OperandType.Method:
				case OperandType.Field:
				case OperandType.Type:
				case OperandType.Token:
					size += 4;
					break;

				case OperandType.Int64:
				case OperandType.Double:
					size += 8;
					break;

				case OperandType.Switch:
					size += 4;
					int[] array = (int[])value.Value;
					size += array.Length * 4;
					break;

				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Unknown operand type for operator '{0}'.", value.Code.ToString("x4")));
			}

			return size;
		}

		private enum OperandType
		{
			BranchTarget = 0,  // LabelReference
			ShortBranchTarget = 15,
			Field = 1,  // FieldReference
			Int32 = 2,
			Int64 = 3,
			Method = 4, // MethodReference
			None = 5,
			Phi = 6,
			Double = 7,
			Signature = 9,
			String = 10, // string
			Switch = 11,
			Token = 12,
			Type = 13,
			Variable = 14,
			SByte = 16,
			Single = 17,
			ShortVariable = 18
		}

		private static OperandType GetOperandType(int code)
		{
			switch (code)
			{
				case 0x00: return OperandType.None; // nop
				case 0x01: return OperandType.None; // break
				case 0x02: return OperandType.None; // ldarg.0
				case 0x03: return OperandType.None; // ldarg.1
				case 0x04: return OperandType.None; // ldarg.2
				case 0x05: return OperandType.None; // ldarg.3
				case 0x06: return OperandType.None; // ldloc.0
				case 0x07: return OperandType.None; // ldloc.1
				case 0x08: return OperandType.None; // ldloc.2
				case 0x09: return OperandType.None; // ldloc.3
				case 0x0a: return OperandType.None; // stloc.0
				case 0x0b: return OperandType.None; // stloc.1
				case 0x0c: return OperandType.None; // stloc.2
				case 0x0d: return OperandType.None; // stloc.3
				case 0x0e: return OperandType.ShortVariable; // ldarg.s
				case 0x0f: return OperandType.ShortVariable; // ldarga.s
				case 0x10: return OperandType.ShortVariable; // starg.s
				case 0x11: return OperandType.ShortVariable; // ldloc.s
				case 0x12: return OperandType.ShortVariable; // ldloca.s
				case 0x13: return OperandType.ShortVariable; // stloc.s
				case 0x14: return OperandType.None; // ldnull
				case 0x15: return OperandType.None; // ldc.i4.m1
				case 0x16: return OperandType.None; // ldc.i4.0
				case 0x17: return OperandType.None; // ldc.i4.1
				case 0x18: return OperandType.None; // ldc.i4.2
				case 0x19: return OperandType.None; // ldc.i4.3
				case 0x1a: return OperandType.None; // ldc.i4.4
				case 0x1b: return OperandType.None; // ldc.i4.5
				case 0x1c: return OperandType.None; // ldc.i4.6
				case 0x1d: return OperandType.None; // ldc.i4.7
				case 0x1e: return OperandType.None; // ldc.i4.8
				case 0x1f: return OperandType.SByte; // ldc.i4.s
				case 0x20: return OperandType.Int32; // ldc.i4
				case 0x21: return OperandType.Int64; // ldc.i8
				case 0x22: return OperandType.Single; // ldc.r4
				case 0x23: return OperandType.Double; // ldc.r8
				case 0x25: return OperandType.None; // dup
				case 0x26: return OperandType.None; // pop
				case 0x27: return OperandType.Method; // jmp
				case 0x28: return OperandType.Method; // call
				case 0x29: return OperandType.Signature; // calli
				case 0x2a: return OperandType.None; // ret
				case 0x2b: return OperandType.ShortBranchTarget; // br.s
				case 0x2c: return OperandType.ShortBranchTarget; // brfalse.s
				case 0x2d: return OperandType.ShortBranchTarget; // brtrue.s
				case 0x2e: return OperandType.ShortBranchTarget; // beq.s
				case 0x2f: return OperandType.ShortBranchTarget; // bge.s
				case 0x30: return OperandType.ShortBranchTarget; // bgt.s
				case 0x31: return OperandType.ShortBranchTarget; // ble.s
				case 0x32: return OperandType.ShortBranchTarget; // blt.s
				case 0x33: return OperandType.ShortBranchTarget; // bne.un.s
				case 0x34: return OperandType.ShortBranchTarget; // bge.un.s
				case 0x35: return OperandType.ShortBranchTarget; // bgt.un.s
				case 0x36: return OperandType.ShortBranchTarget; // ble.un.s
				case 0x37: return OperandType.ShortBranchTarget; // blt.un.s
				case 0x38: return OperandType.BranchTarget; // br
				case 0x39: return OperandType.BranchTarget; // brfalse
				case 0x3a: return OperandType.BranchTarget; // brtrue
				case 0x3b: return OperandType.BranchTarget; // beq
				case 0x3c: return OperandType.BranchTarget; // bge
				case 0x3d: return OperandType.BranchTarget; // bgt
				case 0x3e: return OperandType.BranchTarget; // ble
				case 0x3f: return OperandType.BranchTarget; // blt
				case 0x40: return OperandType.BranchTarget; // bne.un
				case 0x41: return OperandType.BranchTarget; // bge.un
				case 0x42: return OperandType.BranchTarget; // bgt.un
				case 0x43: return OperandType.BranchTarget; // ble.un
				case 0x44: return OperandType.BranchTarget; // blt.un
				case 0x45: return OperandType.Switch; // switch
				case 0x46: return OperandType.None; // ldind.i1
				case 0x47: return OperandType.None; // ldind.u1
				case 0x48: return OperandType.None; // ldind.i2
				case 0x49: return OperandType.None; // ldind.u2
				case 0x4a: return OperandType.None; // ldind.i4
				case 0x4b: return OperandType.None; // ldind.u4
				case 0x4c: return OperandType.None; // ldind.i8
				case 0x4d: return OperandType.None; // ldind.i
				case 0x4e: return OperandType.None; // ldind.r4
				case 0x4f: return OperandType.None; // ldind.r8
				case 0x50: return OperandType.None; // ldind.ref
				case 0x51: return OperandType.None; // stind.ref
				case 0x52: return OperandType.None; // stind.i1
				case 0x53: return OperandType.None; // stind.i2
				case 0x54: return OperandType.None; // stind.i4
				case 0x55: return OperandType.None; // stind.i8
				case 0x56: return OperandType.None; // stind.r4
				case 0x57: return OperandType.None; // stind.r8
				case 0x58: return OperandType.None; // add
				case 0x59: return OperandType.None; // sub
				case 0x5a: return OperandType.None; // mul
				case 0x5b: return OperandType.None; // div
				case 0x5c: return OperandType.None; // div.un
				case 0x5d: return OperandType.None; // rem
				case 0x5e: return OperandType.None; // rem.un
				case 0x5f: return OperandType.None; // and
				case 0x60: return OperandType.None; // or
				case 0x61: return OperandType.None; // xor
				case 0x62: return OperandType.None; // shl
				case 0x63: return OperandType.None; // shr
				case 0x64: return OperandType.None; // shr.un
				case 0x65: return OperandType.None; // neg
				case 0x66: return OperandType.None; // not
				case 0x67: return OperandType.None; // conv.i1
				case 0x68: return OperandType.None; // conv.i2
				case 0x69: return OperandType.None; // conv.i4
				case 0x6a: return OperandType.None; // conv.i8
				case 0x6b: return OperandType.None; // conv.r4
				case 0x6c: return OperandType.None; // conv.r8
				case 0x6d: return OperandType.None; // conv.u4
				case 0x6e: return OperandType.None; // conv.u8
				case 0x6f: return OperandType.Method; // callvirt
				case 0x70: return OperandType.Type; // cpobj
				case 0x71: return OperandType.Type; // ldobj
				case 0x72: return OperandType.String; // ldstr
				case 0x73: return OperandType.Method; // newobj
				case 0x74: return OperandType.Type; // castclass
				case 0x75: return OperandType.Type; // isinst
				case 0x76: return OperandType.None; // conv.r.un
				case 0x79: return OperandType.Type; // unbox
				case 0x7a: return OperandType.None; // throw
				case 0x7b: return OperandType.Field; // ldfld
				case 0x7c: return OperandType.Field; // ldflda
				case 0x7d: return OperandType.Field; // stfld
				case 0x7e: return OperandType.Field; // ldsfld
				case 0x7f: return OperandType.Field; // ldsflda
				case 0x80: return OperandType.Field; // stsfld
				case 0x81: return OperandType.Type; // stobj
				case 0x82: return OperandType.None; // conv.ovf.i1.un
				case 0x83: return OperandType.None; // conv.ovf.i2.un
				case 0x84: return OperandType.None; // conv.ovf.i4.un
				case 0x85: return OperandType.None; // conv.ovf.i8.un
				case 0x86: return OperandType.None; // conv.ovf.u1.un
				case 0x87: return OperandType.None; // conv.ovf.u2.un
				case 0x88: return OperandType.None; // conv.ovf.u4.un
				case 0x89: return OperandType.None; // conv.ovf.u8.un
				case 0x8a: return OperandType.None; // conv.ovf.i.un
				case 0x8b: return OperandType.None; // conv.ovf.u.un
				case 0x8c: return OperandType.Type; // box
				case 0x8d: return OperandType.Type; // newarr
				case 0x8e: return OperandType.None; // ldlen
				case 0x8f: return OperandType.Type; // ldelema
				case 0x90: return OperandType.None; // ldelem.i1
				case 0x91: return OperandType.None; // ldelem.u1
				case 0x92: return OperandType.None; // ldelem.i2
				case 0x93: return OperandType.None; // ldelem.u2
				case 0x94: return OperandType.None; // ldelem.i4
				case 0x95: return OperandType.None; // ldelem.u4
				case 0x96: return OperandType.None; // ldelem.i8
				case 0x97: return OperandType.None; // ldelem.i
				case 0x98: return OperandType.None; // ldelem.r4
				case 0x99: return OperandType.None; // ldelem.r8
				case 0x9a: return OperandType.None; // ldelem.ref
				case 0x9b: return OperandType.None; // stelem.i
				case 0x9c: return OperandType.None; // stelem.i1
				case 0x9d: return OperandType.None; // stelem.i2
				case 0x9e: return OperandType.None; // stelem.i4
				case 0x9f: return OperandType.None; // stelem.i8
				case 0xa0: return OperandType.None; // stelem.r4
				case 0xa1: return OperandType.None; // stelem.r8
				case 0xa2: return OperandType.None; // stelem.ref
				case 0xa3: return OperandType.Type; // ldelem.any
				case 0xa4: return OperandType.Type; // stelem.any
				case 0xa5: return OperandType.Type; // unbox.any
				case 0xb3: return OperandType.None; // conv.ovf.i1
				case 0xb4: return OperandType.None; // conv.ovf.u1
				case 0xb5: return OperandType.None; // conv.ovf.i2
				case 0xb6: return OperandType.None; // conv.ovf.u2
				case 0xb7: return OperandType.None; // conv.ovf.i4
				case 0xb8: return OperandType.None; // conv.ovf.u4
				case 0xb9: return OperandType.None; // conv.ovf.i8
				case 0xba: return OperandType.None; // conv.ovf.u8
				case 0xc2: return OperandType.Type; // refanyval
				case 0xc3: return OperandType.None; // ckfinite
				case 0xc6: return OperandType.Type; // mkrefany
				case 0xd0: return OperandType.Token; // ldtoken
				case 0xd1: return OperandType.None; // conv.u2
				case 0xd2: return OperandType.None; // conv.u1
				case 0xd3: return OperandType.None; // conv.i
				case 0xd4: return OperandType.None; // conv.ovf.i
				case 0xd5: return OperandType.None; // conv.ovf.u
				case 0xd6: return OperandType.None; // add.ovf
				case 0xd7: return OperandType.None; // add.ovf.un
				case 0xd8: return OperandType.None; // mul.ovf
				case 0xd9: return OperandType.None; // mul.ovf.un
				case 0xda: return OperandType.None; // sub.ovf
				case 0xdb: return OperandType.None; // sub.ovf.un
				case 0xdc: return OperandType.None; // endfinally
				case 0xdd: return OperandType.BranchTarget; // leave
				case 0xde: return OperandType.ShortBranchTarget; // leave.s
				case 0xdf: return OperandType.None; // stind.i
				case 0xe0: return OperandType.None; // conv.u
				case 0xf8: return OperandType.None; // prefix7
				case 0xf9: return OperandType.None; // prefix6
				case 0xfa: return OperandType.None; // prefix5
				case 0xfb: return OperandType.None; // prefix4
				case 0xfc: return OperandType.None; // prefix3
				case 0xfd: return OperandType.None; // prefix2
				case 0xfe: return OperandType.None; // prefix1
				case 0xff: return OperandType.None; // prefixref
			}

			switch (code)
			{
				case 0xfe00: return OperandType.None; // arglist
				case 0xfe01: return OperandType.None; // ceq
				case 0xfe02: return OperandType.None; // cgt
				case 0xfe03: return OperandType.None; // cgt.un
				case 0xfe04: return OperandType.None; // clt
				case 0xfe05: return OperandType.None; // clt.un
				case 0xfe06: return OperandType.Method; // ldftn
				case 0xfe07: return OperandType.Method; // ldvirtftn
				case 0xfe09: return OperandType.Variable; // ldarg
				case 0xfe0a: return OperandType.Variable; // ldarga
				case 0xfe0b: return OperandType.Variable; // starg
				case 0xfe0c: return OperandType.Variable; // ldloc
				case 0xfe0d: return OperandType.Variable; // ldloca
				case 0xfe0e: return OperandType.Variable; // stloc
				case 0xfe0f: return OperandType.None; // localloc
				case 0xfe11: return OperandType.None; // endfilter
				case 0xfe12: return OperandType.SByte; // unaligned
				case 0xfe13: return OperandType.None; // volatile
				case 0xfe14: return OperandType.None; // tail
				case 0xfe15: return OperandType.Type; // initobj
				case 0xfe16: return OperandType.Type; // constrained
				case 0xfe17: return OperandType.None; // cpblk
				case 0xfe18: return OperandType.None; // initblk
				case 0xfe1a: return OperandType.None; // rethrow
				case 0xfe1c: return OperandType.Type; // sizeof
				case 0xfe1d: return OperandType.None; // refanytype
				case 0xfe1e: return OperandType.None; // readonly
			}

			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Unknown IL instruction '{0}'.", code.ToString("X4", CultureInfo.InvariantCulture)));
		}

		public static int GetMethodBodySize(IMethodDeclaration value)
		{
			int size = 0;

			IMethodBody methodBody = value.Body as IMethodBody;
			if (methodBody != null)
			{
				IInstructionCollection instructions = methodBody.Instructions;
				if (instructions.Count != 0)
				{
					IInstruction lastInstruction = instructions[instructions.Count - 1];
					size = size + lastInstruction.Offset + Helper.GetInstructionSize(lastInstruction);
				}
			}

			return size;
		}
	}
}
