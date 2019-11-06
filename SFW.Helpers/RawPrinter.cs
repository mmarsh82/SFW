using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SFW.Helpers
{
    public static partial class RawPrinter
    {
        #region Imported Properties

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        #endregion

        /// <summary>
        ///  When the function is given a printer name and an unmanaged array
        ///  of bytes, the function sends those bytes to the print queue.
        /// </summary>
        /// <param name="szPrinterName">Printer Name</param>
        /// <param name="pBytes">Byte Array</param>
        /// <param name="dwCount">Byte Count</param>
        /// <returns>true on success, false on failure</returns>
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, int dwCount)
        {
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "SFW_BarLabel";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out IntPtr hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out int dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                int dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            // Open the file.
            using (FileStream fs = new FileStream(szFileName, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    var bytes = new byte[fs.Length];
                    bool bSuccess = false;
                    // Your unmanaged pointer.
                    IntPtr pUnmanagedBytes = new IntPtr(0);
                    int nLength = Convert.ToInt32(fs.Length);
                    // Read the contents of the file into the array.
                    bytes = br.ReadBytes(nLength);
                    // Allocate some unmanaged memory for those bytes.
                    pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                    // Copy the managed byte array into the unmanaged array.
                    Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
                    // Send the unmanaged bytes to the printer.
                    bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
                    // Free the unmanaged memory that you allocated earlier.
                    Marshal.FreeCoTaskMem(pUnmanagedBytes);
                    return bSuccess;
                }
            }
        }
        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            // How many characters are in the string?
            int dwCount = szString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            IntPtr pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
    }
}
