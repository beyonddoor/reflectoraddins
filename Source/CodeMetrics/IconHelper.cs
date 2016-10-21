namespace Reflector.CodeMetrics
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using Reflector.CodeModel;

	internal sealed class IconHelper
	{		
		private IconHelper()
		{
		}

		internal static int GetImageIndex(ITypeReference typeReference)
		{
			ITypeDeclaration typeDeclaration = typeReference.Resolve();
			if (typeDeclaration == null)
			{
				return BrowserResource.Error;
			}

			int icon = BrowserResource.Interface;

			if (!typeDeclaration.Interface)
			{
				icon = BrowserResource.Class;

				ITypeReference baseType = typeDeclaration.BaseType;
				if (baseType != null)
				{
					if (Helper.IsEnumeration(typeDeclaration))
					{
						icon = BrowserResource.Enumeration;
					}
					else if ((Helper.IsValueType(typeDeclaration)) && (!typeDeclaration.Abstract))
					{
						icon = BrowserResource.Structure;
					}
					else if (Helper.IsDelegate(typeDeclaration))
					{
						icon = BrowserResource.Delegate;
					}
				}
			}

			switch (typeDeclaration.Visibility)
			{
				case TypeVisibility.Public:
				case TypeVisibility.NestedPublic:
					return icon;

				case TypeVisibility.Private:
				case TypeVisibility.NestedAssembly:
					return icon + 1;

				case TypeVisibility.NestedFamilyOrAssembly:
					return icon + 2;

				case TypeVisibility.NestedFamily:
					return icon + 3;

				case TypeVisibility.NestedPrivate:
					return icon + 4;

				case TypeVisibility.NestedFamilyAndAssembly:
					return icon + 5;
			}

			throw new NotSupportedException();
		}

		internal static int GetImageIndex(IMemberReference memberReference)
		{
			IFieldReference fieldReference = memberReference as IFieldReference;
			if (fieldReference != null)
			{
				IFieldDeclaration fieldDeclaration = fieldReference.Resolve();
				if (fieldDeclaration == null)
				{
					return BrowserResource.Error;
				}

				int icon = BrowserResource.Field;
				
				if (IsEnumerationElement(fieldReference))
				{
					icon = BrowserResource.EnumerationElement;
				}
				else
				{
					if (fieldDeclaration.Static)
					{
						icon += 6;
					}
				}

				switch (fieldDeclaration.Visibility)
				{
					case FieldVisibility.Public:
						return icon;

					case FieldVisibility.Assembly:
						return icon + 1;

					case FieldVisibility.FamilyOrAssembly:
						return icon + 2;

					case FieldVisibility.Family:
						return icon + 3;

					case FieldVisibility.Private:
					case FieldVisibility.PrivateScope:
						return icon + 4;

					case FieldVisibility.FamilyAndAssembly:
						return icon + 5;
				}
			}

			IMethodReference methodReference = memberReference as IMethodReference;
			if (methodReference != null)
			{
				IArrayType arrayType = methodReference.DeclaringType as IArrayType;
				if (arrayType != null)
				{
					return BrowserResource.Method;
				}

				IMethodDeclaration methodDeclaration = methodReference.Resolve();
				if (methodDeclaration == null)
				{
					return BrowserResource.Error;	
				}

				int icon = BrowserResource.Method;

				string methodName = methodReference.Name;
				if ((methodName == ".ctor") || (methodName == ".cctor"))
				{
					icon = BrowserResource.Constructor;
				}
				else if ((methodDeclaration.Virtual) && (!methodDeclaration.Abstract))
				{
					icon += 12;	
				}

				if (methodDeclaration.Static)
				{
					icon += 6;
				}

				switch (methodDeclaration.Visibility)
				{
					case MethodVisibility.Public:
						return icon;

					case MethodVisibility.Assembly:
						return icon + 1;

					case MethodVisibility.FamilyOrAssembly:
						return icon + 2;

					case MethodVisibility.Family:
						return icon + 3;

					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
						return icon + 4;

					case MethodVisibility.FamilyAndAssembly:
						return icon + 5;
				}
			}

			IPropertyReference propertyReference = memberReference as IPropertyReference;
			if (propertyReference != null)
			{
				IPropertyDeclaration propertyDeclaration = propertyReference.Resolve();
				if (propertyDeclaration != null)
				{
					IMethodReference getMethodReference = propertyDeclaration.GetMethod;
					IMethodDeclaration getMethod = (getMethodReference == null) ? null : getMethodReference.Resolve();

					IMethodReference setMethodReference = propertyDeclaration.SetMethod;
					IMethodDeclaration setMethod = (setMethodReference == null) ? null : setMethodReference.Resolve();

					int index = BrowserResource.Property;
	
					if ((setMethod != null) && (getMethod != null))
					{
						index = BrowserResource.Property;
					}
					else if (setMethod != null)
					{
						index = BrowserResource.PropertyWrite;
					}
					else if (getMethod != null)
					{
						index = BrowserResource.PropertyRead;
					}
	
					if (Helper.IsStatic(propertyReference))
					{
						index += 6;
					}
	
					switch (Helper.GetVisibility(propertyReference))
					{
						case MethodVisibility.Public:
							return index + 0;
	
						case MethodVisibility.Assembly:
							return index + 1;
	
						case MethodVisibility.FamilyOrAssembly:
							return index + 2;
	
						case MethodVisibility.Family:
							return index + 3;
	
						case MethodVisibility.Private:
						case MethodVisibility.PrivateScope:
							return index + 4;
	
						case MethodVisibility.FamilyAndAssembly:
							return index + 5;
					}
				}
			}

			IEventReference eventReference = memberReference as IEventReference;
			if (eventReference != null)
			{
				int index = BrowserResource.Event;

				if (Helper.IsStatic(eventReference))
				{
					index += 6;
				}

				switch (Helper.GetVisibility(eventReference))
				{
					case MethodVisibility.Public:
						return index + 0;

					case MethodVisibility.Assembly:
						return index + 1;

					case MethodVisibility.FamilyOrAssembly:
						return index + 2;

					case MethodVisibility.Family:
						return index + 3;

					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
						return index + 4;

					case MethodVisibility.FamilyAndAssembly:
						return index + 5;
				}
			}

			throw new NotSupportedException();
		}

		internal static int GetImageIndex(IResource value)
		{
			switch (GetFileExtension(value.Name).ToLower(CultureInfo.InvariantCulture))
			{
				case ".bmp":
				case ".emf":
				case ".gif":
				case ".jpg":
				case ".png":
				case ".tif":
				case ".wmf":
				case ".ico":
				case ".cur":
				case ".exif":
				case ".jpeg":
				case ".tiff":
					return BrowserResource.ImageResource;

				case ".js":
				case ".cs":
				case ".vb":
				case ".txt":
				case ".xml":
				case ".xsl":
				case ".xsd":
				case ".css":
				case ".htm":
				case ".mht":
				case ".asp":
				case ".aspx":
				case ".html":
					return BrowserResource.TextResource;

				case ".resources":
					return BrowserResource.Resource;

				default:
					return BrowserResource.ByteArrayResource;
			}
		}

		internal static int GetColor(ITypeReference typeReference)
		{
			ITypeDeclaration typeDeclaration = typeReference.Resolve();
			if (typeDeclaration == null)
			{
				return ColorInformation.Error;
			}

			switch (typeDeclaration.Visibility)
			{
				case TypeVisibility.Private:
				case TypeVisibility.NestedPrivate:
				case TypeVisibility.NestedAssembly:
				case TypeVisibility.NestedFamilyAndAssembly:
					return ColorInformation.Hidden;

				case TypeVisibility.Public:
				case TypeVisibility.NestedPublic:
				case TypeVisibility.NestedFamily:
				case TypeVisibility.NestedFamilyOrAssembly:
					return ColorInformation.Normal;
			}

			throw new NotSupportedException();
		}

		internal static int GetColor(IMemberReference memberReference)
		{
			IFieldReference fieldReference = memberReference as IFieldReference;
			if (fieldReference != null)
			{
				IFieldDeclaration fieldDeclaration = fieldReference.Resolve();
				if (fieldDeclaration == null)
				{
					return ColorInformation.Error;
				}

				switch (fieldDeclaration.Visibility)
				{
					case FieldVisibility.Private:
					case FieldVisibility.PrivateScope:
					case FieldVisibility.Assembly:
					case FieldVisibility.FamilyAndAssembly:
						return ColorInformation.Hidden;

					case FieldVisibility.Public:
					case FieldVisibility.Family:
					case FieldVisibility.FamilyOrAssembly:
						return ColorInformation.Normal;
				}
			}

			IMethodReference methodReference = memberReference as IMethodReference;
			if (methodReference != null)
			{
				IArrayType arrayType = methodReference.DeclaringType as IArrayType;
				if (arrayType != null)
				{
					return IconHelper.GetColor(arrayType.ElementType as ITypeReference);
				}

				IMethodDeclaration methodDeclaration = methodReference.Resolve();
				if (methodDeclaration == null)
				{
					return ColorInformation.Error;	
				}

				switch (methodDeclaration.Visibility)
				{
					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
					case MethodVisibility.Assembly:
					case MethodVisibility.FamilyAndAssembly:
						return ColorInformation.Hidden;

					case MethodVisibility.Public:
					case MethodVisibility.Family:
					case MethodVisibility.FamilyOrAssembly:
						return ColorInformation.Normal;
				}
			}

			IPropertyReference propertyReference = memberReference as IPropertyReference;
			if (propertyReference != null)
			{
				switch (Helper.GetVisibility(propertyReference))
				{
					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
					case MethodVisibility.Assembly:
					case MethodVisibility.FamilyAndAssembly:
						return ColorInformation.Hidden;

					case MethodVisibility.Public:
					case MethodVisibility.Family:
					case MethodVisibility.FamilyOrAssembly:
						return ColorInformation.Normal;
				}
			}

			IEventReference eventReference = memberReference as IEventReference;
			if (eventReference != null)
			{
				switch (Helper.GetVisibility(eventReference))
				{
					case MethodVisibility.Private:
					case MethodVisibility.PrivateScope:
					case MethodVisibility.Assembly:
					case MethodVisibility.FamilyAndAssembly:
						return ColorInformation.Hidden;

					case MethodVisibility.Public:
					case MethodVisibility.Family:
					case MethodVisibility.FamilyOrAssembly:
						return ColorInformation.Normal;
				}
			}

			throw new NotSupportedException();
		}

		internal static int GetColorDeclaringType(ITypeReference typeReference)
		{
			ITypeReference declaringType = typeReference.Owner as ITypeReference;
			if (declaringType != null)
			{
				int color = GetColorDeclaringType(declaringType);
				if (color == ColorInformation.Hidden)
				{
					return color;		
				}
			}
			
			return GetColor(typeReference);
		}

		internal static int GetColorDeclaringType(IMemberReference memberReference)
		{
			ITypeReference typeReference = memberReference.DeclaringType as ITypeReference;
			if (typeReference != null)
			{
				int color = GetColorDeclaringType(typeReference);
				if (color == ColorInformation.Hidden)
				{
					return color;	
				}
			}
			
			return GetColor(memberReference);
		}

		private static bool IsEnumerationElement(IFieldReference value)
		{
			IType fieldType = value.FieldType;
			IType declaringType = value.DeclaringType;
			if (fieldType.Equals(declaringType))
			{
				ITypeReference typeReference = fieldType as ITypeReference;
				if (typeReference != null)
				{
					return Helper.IsEnumeration(typeReference);
				}
			}

			return false;
		}

		private static string GetFileExtension(string resourceName)
		{
			string fileExtension = string.Empty;
			int index = resourceName.LastIndexOf('.');
			if (index != -1)
			{
				fileExtension = resourceName.Substring(index);
			}
			
			return fileExtension;
		}

		internal sealed class ColorInformation
		{
			private static int hidden;
			private static int normal;
	
			static ColorInformation()
			{
				Color textColor = SystemColors.WindowText;
				Color backColor = SystemColors.Window;
	
				hidden |= Interpolate(textColor.A, backColor.A) << 24;
				hidden |= Interpolate(textColor.R, backColor.R) << 16;
				hidden |= Interpolate(textColor.G, backColor.G) << 8;
				hidden |= Interpolate(textColor.B, backColor.B) << 0;			
				
				normal = textColor.ToArgb();
			}
	
			public static int Hidden
			{
				get
				{	
					return hidden;
				}	
			}
	
			public static int Normal
			{
				get
				{	
					return normal;
				}	
			}
	
			public static int Error
			{
				get
				{
					return ColorInformation.Normal;
				}
			}
	
			public static byte Interpolate(byte foreground, byte background)
			{
				if (foreground > background)
				{
					return (byte)(background + ((foreground - background) * 0.5));
				}
	
				return (byte) (background - ((background - foreground) * 0.5));
			}
		}
	}
}
