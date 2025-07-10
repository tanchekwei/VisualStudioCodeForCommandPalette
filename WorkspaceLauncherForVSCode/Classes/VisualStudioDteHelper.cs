// using System;
// using System.Collections.Generic;
// using System.Runtime.InteropServices;
// using EnvDTE;
// using Microsoft.VisualStudio.OLE.Interop;

// namespace WorkspaceLauncherForVSCode.Classes
// {
//     internal static class VisualStudioDteHelper
//     {
//         internal static List<string> GetAllOpenSolutionPaths()
//         {
//             List<string> solutionPaths = new();

//             IRunningObjectTable rot;
//             IEnumMoniker enumMoniker;
//             GetRunningObjectTable(0, out rot);
//             rot.EnumRunning(out enumMoniker);

//             IMoniker[] monikers = new IMoniker[1];
//             while (enumMoniker.Next(1, monikers, out var fetched) == 0)
//             {
//                 rot.GetObject(monikers[0], out object? comObject);
//                 if (comObject is not DTE dte)
//                     continue;

//                 try
//                 {
//                     var solution = dte.Solution?.FullName;
//                     if (!string.IsNullOrEmpty(solution))
//                     {
//                         solutionPaths.Add(solution);
//                     }
//                 }
//                 catch
//                 {
//                     // Some DTEs may be busy/unavailable
//                 }
//             }

//             return solutionPaths;
//         }

// internal static List<(string SolutionPath, IntPtr Hwnd)> GetAllDteSolutionWindows()
// {
//     List<(string, IntPtr)> result = new();

//     GetRunningObjectTable(0, out var rot);
//     rot.EnumRunning(out var enumMoniker);

//     IMoniker[] monikers = new IMoniker[1];
//     while (enumMoniker.Next(1, monikers, out var fetched) == 0)
//     {
//         rot.GetObject(monikers[0], out object? comObject);
//         if (comObject is not DTE dte)
//             continue;

//         try
//         {
//             var solutionPath = dte.Solution?.FullName;
//             var hwnd = new IntPtr(dte.MainWindow.HWnd);

//             if (!string.IsNullOrEmpty(solutionPath))
//             {
//                 result.Add((solutionPath, hwnd));
//             }
//         }
//         catch
//         {
//             // ignore inaccessible instances
//         }
//     }

//     return result;
// }

//     [DllImport("ole32.dll")]
//         private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

//         [DllImport("ole32.dll")]
//         private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);
//     }
// }
