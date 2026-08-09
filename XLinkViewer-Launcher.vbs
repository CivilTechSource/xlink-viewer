Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")

' Get the directory where this script is located
strScriptPath = WScript.ScriptFullName
strScriptDir = objFSO.GetParentFolderName(strScriptPath)

' Paths
strExePath = strScriptDir & "\XLinkViewer.exe"
strLockFile = objShell.ExpandEnvironmentStrings("%TEMP%") & "\XLinkViewer_Launch.lock"
strArgsFile = objShell.ExpandEnvironmentStrings("%TEMP%") & "\XLinkViewer_Args.txt"

' Check if XLinkViewer is already running
On Error Resume Next
Set objWMI = GetObject("winmgmts:\\.\root\cimv2")
Set colProcesses = objWMI.ExecQuery("Select * from Win32_Process Where Name = 'XLinkViewer.exe'")
If colProcesses.Count > 0 Then
    WScript.Quit
End If
On Error Goto 0

' Check if we're in a batch collection window
If objFSO.FileExists(strLockFile) Then
    ' Add this file to the arguments list
    If WScript.Arguments.Count > 0 Then
        Set objArgsFile = objFSO.OpenTextFile(strArgsFile, 8, True) ' 8 = Append
        objArgsFile.WriteLine WScript.Arguments(0)
        objArgsFile.Close
    End If
    WScript.Quit
End If

' Start a new collection window
Set objLockFile = objFSO.CreateTextFile(strLockFile, True)
objLockFile.WriteLine "lock"
objLockFile.Close

' Write the first argument
If WScript.Arguments.Count > 0 Then
    Set objArgsFile = objFSO.CreateTextFile(strArgsFile, True)
    objArgsFile.WriteLine WScript.Arguments(0)
    objArgsFile.Close
End If

' Wait for additional files (Windows Explorer calls context menu commands rapidly)
WScript.Sleep 1500

' Collect all arguments from the file
strAllArgs = ""
If objFSO.FileExists(strArgsFile) Then
    Set objArgsFile = objFSO.OpenTextFile(strArgsFile, 1) ' 1 = Read
    Do Until objArgsFile.AtEndOfStream
        strLine = objArgsFile.ReadLine
        If strLine <> "" Then
            strAllArgs = strAllArgs & " """ & strLine & """"
        End If
    Loop
    objArgsFile.Close
    objFSO.DeleteFile strArgsFile
End If

' Clean up lock file
If objFSO.FileExists(strLockFile) Then
    objFSO.DeleteFile strLockFile
End If

' Launch XLinkViewer with all collected arguments
If strAllArgs <> "" Then
    objShell.Run """" & strExePath & """" & strAllArgs, 1, False
Else
    objShell.Run """" & strExePath & """", 1, False
End If

