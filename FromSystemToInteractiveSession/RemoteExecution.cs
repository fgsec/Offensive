using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace RemoteExecution
{
    public class Program
    {
        public static void Main(String[] args)
        {
            OpenProcessAsUser(@"C:\windows\system32\calc.exe", true, "user");
        }
        public static bool logOutput = true;

        public static TokenData GetTokenForTargetUser(string targetUser)
        {
            logToFile(String.Format("[#] Trying to read user from process information..."));
            IntPtr tokenHandle = IntPtr.Zero;
            TokenData tokendata = new TokenData() { userName = "", userToken = IntPtr.Zero };
            Process[] explorer_processes = Process.GetProcessesByName("explorer");
            foreach (Process process in explorer_processes)
            {
                IntPtr hProcess = OpenProcess(ProcessAccessFlags.QueryInformation, true, process.Id);
                if(hProcess != IntPtr.Zero)
                {
                    if(OpenProcessToken(process.Handle, TOKEN_DUPLICATE | TOKEN_ASSIGN_PRIMARY | TOKEN_QUERY, ref tokenHandle))
                    {
                        WindowsIdentity wi = new WindowsIdentity(tokenHandle);
                        string user = wi.Name;
                        tokendata.userName = user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
                        logToFile(String.Format("[#] Found process for: '{0}'", user));
                        if (user.ToUpper().Equals(targetUser.ToUpper()) || targetUser.Equals(""))
                        {
                            tokendata.userToken = tokenHandle;
                            return tokendata;
                        }
                    }
                }
            }
            return tokendata;
        }

        public static void OpenProcessAsUser(String command_line, bool logOutput, string targetUser="")
        {
            Program.logOutput = logOutput;

            logToFile("Started!");

            TokenData tokendata = GetTokenForTargetUser(targetUser);

            if (tokendata.userName != "" && tokendata.userToken != IntPtr.Zero)
            {

                logToFile(String.Format("[!] Spawning '{0}' for '{1}'", tokendata.userName, command_line));

                IntPtr token = tokendata.userToken;
   
                STARTUPINFO startupinfo = default(STARTUPINFO);
                startupinfo.cb = Marshal.SizeOf(startupinfo);
                startupinfo.lpDesktop = "Winsta0\\default";

                PROCESS_INFORMATION process_INFORMATION = default(PROCESS_INFORMATION);

                bool rs = CreateProcessAsUser(token, null, command_line, IntPtr.Zero, IntPtr.Zero, false, 131088U, IntPtr.Zero, @"C:\windows\system32\", ref startupinfo, out process_INFORMATION);

                logToFile(String.Format("[-] Ending.. createprocess results: {0},{1}", rs, Marshal.GetLastWin32Error()));
            }
            else
            {
                logToFile(String.Format("[-] Ending.. error trying to get user from process: {0}", Marshal.GetLastWin32Error()));
            }

        }

        public static void logToFile(string content)
        {
            if(Program.logOutput)
            using (StreamWriter sw = File.AppendText(@"c:\users\public\extwindows.txt"))
            {
                sw.WriteLine(content);
            }

        }

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandle, uint dwCreationFlags, IntPtr lpEnvrionment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        public static int SE_PRIVILEGE_DISABLED = 0x00000000;
        public static int SE_PRIVILEGE_ENABLED = 0x00000002;
        public static int TOKEN_QUERY = 0x00000008;
        public static int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public static int TOKEN_DUPLICATE = 0x00000002;
        public static int TOKEN_IMPERSONATE = 0x00000004;
        public static int TOKEN_ASSIGN_PRIMARY = 0x00000001;
        public static int TOKEN_ADJUST_SESSIONID = (0x0100);
        public static int TOKEN_ADJUST_DEFAULT = (0x0080);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        public struct TokenData
        {
            public IntPtr userToken;
            public string userName;
        }

    }
}
