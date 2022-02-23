#include <iostream>
#include <Windows.h>
#include "Header.h"

static NtSystemDebugControl g_NtSystemDebugControl = NULL;

int main() {

    HMODULE module;
    NTSTATUS status;
    ULONG returnLength;

    HANDLE file = CreateFile(L"c:\\dump123.d", 0x10000000, 0, NULL, 1, 0x80, NULL);
    SYSDBG_LIVEDUMP_CONTROL dumpControl = {};

    memset(&dumpControl, 0, sizeof(dumpControl));

    dumpControl.Version = 1;
    dumpControl.BugCheckCode = 0x161;
    dumpControl.DumpFileHandle = file;

    module = LoadLibrary(L"ntdll.dll");
    g_NtSystemDebugControl = (NtSystemDebugControl)GetProcAddress(module,"NtSystemDebugControl");
    FreeLibrary(module);
    
    if (file == INVALID_HANDLE_VALUE) {
        DWORD result = GetLastError();
        printf("CreateFileW failed: %d\n", result);
        return 0;
    }

    status = g_NtSystemDebugControl(37, (PVOID)(&dumpControl), sizeof(dumpControl), NULL, 0, &returnLength);
    
    printf("NtSystemDebugControl status:  %08x\n", status);
       
    std::cout << "Bye!\n";


}
