namespace Reflector.ComLoader
{
	using System;
	using System.IO;
	using System.Runtime.InteropServices;

	internal sealed class NativeMethods
	{
		private NativeMethods()
		{
		}

		public enum RegKind
		{
			RegKind_Default = 0,
			RegKind_Register = 1,
			RegKind_None = 2
		}

		[DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		public static extern void LoadTypeLibEx(string strTypeLibName, RegKind regKind, out ITypeLib typeLib);

		[ComImport, Guid("00020402-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface ITypeLib
		{
			[PreserveSig]
			int GetTypeInfoCount();
			void GetTypeInfo(int index, out ITypeInfo ppTI);
			void GetTypeInfoType(int index, out TYPEKIND pTKind);
			void GetTypeInfoOfGuid(ref Guid guid, out ITypeInfo ppTInfo);
			void GetLibAttr(out IntPtr ppTLibAttr);
			void GetTypeComp(out ITypeComp ppTComp);
			void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
			[return: MarshalAs(UnmanagedType.Bool)]
			bool IsName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal);
			void FindName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] ITypeInfo[] ppTInfo, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgMemId, ref short pcFound);
			[PreserveSig]
			void ReleaseTLibAttr(IntPtr pTLibAttr);
		}

		[ComImport, Guid("00020401-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface ITypeInfo
		{
			void GetTypeAttr(out IntPtr ppTypeAttr);
			void GetTypeComp(out ITypeComp ppTComp);
			void GetFuncDesc(int index, out IntPtr ppFuncDesc);
			void GetVarDesc(int index, out IntPtr ppVarDesc);
			void GetNames(int memid, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0x2)] string[] rgBstrNames, int cMaxNames, out int pcNames);
			void GetRefTypeOfImplType(int index, out int href);
			void GetImplTypeFlags(int index, out IMPLTYPEFLAGS pImplTypeFlags);
			void GetIDsOfNames([In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 0x1)] string[] rgszNames, int cNames, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0x1)] int[] pMemId);
			void Invoke([MarshalAs(UnmanagedType.IUnknown)] object pvInstance, int memid, short wFlags, ref DISPPARAMS pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, out int puArgErr);
			void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
			void GetDllEntry(int memid, INVOKEKIND invKind, IntPtr pBstrDllName, IntPtr pBstrName, IntPtr pwOrdinal);
			void GetRefTypeInfo(int hRef, out ITypeInfo ppTI);
			void AddressOfMember(int memid, INVOKEKIND invKind, out IntPtr ppv);
			void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObj);
			void GetMops(int memid, out string pBstrMops);
			void GetContainingTypeLib(out ITypeLib ppTLB, out int pIndex);
			[PreserveSig]
			void ReleaseTypeAttr(IntPtr pTypeAttr);
			[PreserveSig]
			void ReleaseFuncDesc(IntPtr pFuncDesc);
			[PreserveSig]
			void ReleaseVarDesc(IntPtr pVarDesc);
		}

		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020403-0000-0000-C000-000000000046")]
		public interface ITypeComp
		{
			void Bind([MarshalAs(UnmanagedType.LPWStr)] string szName, int lHashVal, short wFlags, out ITypeInfo ppTInfo, out DESCKIND pDescKind, out BINDPTR pBindPtr);
			void BindType([MarshalAs(UnmanagedType.LPWStr)] string szName, int lHashVal, out ITypeInfo ppTInfo, out ITypeComp ppTComp);
		}
	}
}
