// using System;
// using System.Runtime.InteropServices;
// using System.Runtime.Versioning;
// using System.Security;

// public static class Marshal2
// {
//     internal const string OLEAUT32 = "oleaut32.dll";
//     internal const string OLE32 = "ole32.dll";

//     [System.Security.SecurityCritical]  // auto-generated_required
//     public static Object GetActiveObject(string progID)
//     {
//         Object obj = null;
//         Guid clsid;

//         // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
//         // CLSIDFromProgIDEx doesn't exist.
//         try
//         {
//             CLSIDFromProgIDEx(progID, out clsid);
//         }
//         //            catch
//         catch (Exception ex)
//         {
//             CLSIDFromProgID(progID, out clsid);
//         }

//         GetActiveObject(ref clsid, IntPtr.Zero, out obj);
//         return obj;
//     }

//     //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
//     [DllImport(OLE32, PreserveSig = false)]
//     [ResourceExposure(ResourceScope.None)]
//     [SuppressUnmanagedCodeSecurity]
//     [System.Security.SecurityCritical]  // auto-generated
//     private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

//     //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
//     [DllImport(OLE32, PreserveSig = false)]
//     [ResourceExposure(ResourceScope.None)]
//     [SuppressUnmanagedCodeSecurity]
//     [System.Security.SecurityCritical]  // auto-generated
//     private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

//     //[DllImport(Microsoft.Win32.Win32Native.OLEAUT32, PreserveSig = false)]
//     [DllImport(OLEAUT32, PreserveSig = false)]
//     [ResourceExposure(ResourceScope.None)]
//     [SuppressUnmanagedCodeSecurity]
//     [System.Security.SecurityCritical]  // auto-generated
//     private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out Object ppunk);

// }
