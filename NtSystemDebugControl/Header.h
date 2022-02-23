
typedef union _SYSDBG_LIVEDUMP_CONTROL_ADDPAGES {
    struct {
        ULONG HypervisorPages : 1;
        ULONG Reserved : 31;
    };
    ULONG AsUlong;
} SYSDBG_LIVEDUMP_CONTROL_ADDPAGES, * PSYSDBG_LIVEDUMP_CONTROL_ADDPAGES;

typedef union _SYSDBG_LIVEDUMP_CONTROL_FLAGS {
    struct {
        ULONG UseDumpStorageStack : 1;
        ULONG CompressMemoryPagesData : 1;
        ULONG IncludeUserSpaceMemoryPages : 1;
        ULONG AbortIfMemoryPressure : 1; // REDSTONE4
        ULONG Reserved : 28;
    };
    ULONG AsUlong;
} SYSDBG_LIVEDUMP_CONTROL_FLAGS, * PSYSDBG_LIVEDUMP_CONTROL_FLAGS;


typedef struct _SYSDBG_LIVEDUMP_CONTROL {
    ULONG Version;
    ULONG BugCheckCode;
    ULONG_PTR BugCheckParam1;
    ULONG_PTR BugCheckParam2;
    ULONG_PTR BugCheckParam3;
    ULONG_PTR BugCheckParam4;
    HANDLE DumpFileHandle;
    HANDLE CancelEventHandle;
    SYSDBG_LIVEDUMP_CONTROL_FLAGS Flags;
    SYSDBG_LIVEDUMP_CONTROL_ADDPAGES AddPagesControl;
} SYSDBG_LIVEDUMP_CONTROL, * PSYSDBG_LIVEDUMP_CONTROL;

typedef NTSTATUS (__stdcall*
    NtSystemDebugControl) (
        ULONG ControlCode,
        PVOID InputBuffer,
        ULONG InputBufferLength,
        PVOID OutputBuffer,
        ULONG OutputBufferLength,
        PULONG ReturnLength
    );