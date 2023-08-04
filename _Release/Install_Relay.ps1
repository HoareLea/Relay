# Get the last logged on user
$LastLoggedOnUser = Get-WmiObject -Class Win32_UserProfile -Filter "Special=False" |
    Where-Object {
        ($_.LocalPath.Split('\')[-1] -notlike '*_adm') -and
        ($_.LocalPath.Split('\')[-1] -notlike '*system') -and
        ($_.LocalPath.Split('\')[-1] -notlike '*Administrator*') -and
        ($_.LocalPath.Split('\')[-1] -notlike '*admin*') -and
        ($_.LocalPath.Split('\')[-1] -notlike 'WDS') -and
        ($_.LocalPath.Split('\')[-1] -notlike '*Public*') -and
        ($_.LocalPath.Split('\')[-1] -notlike '*Default*')
    } |
    Sort-Object -Property LastUseTime -Descending |
    Select-Object -First 1 -ExpandProperty LocalPath

$LastLoggedOnUser = $LastLoggedOnUser.Split('\')[-1]

# Get the %AppData% path of the last logged on user
$AppDataPath = "C:\Users\$LastLoggedOnUser\AppData\Roaming"

# Function to get a list of installed software
function Get-InstalledSoftware {
    $uninstallKeys = Get-ItemProperty -Path "HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*", "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*" |
                     Where-Object { $_.DisplayName -match "Revit (\d{4})" } |
                     Select-Object DisplayName, DisplayVersion
    return $uninstallKeys
}
Write-Output Get-InstalledSoftware
Write-Output $AppDataPath
$fileUrl = "https://github.com/HoareLea/Relay/blob/master/_Release/Revit2023/Relay.addin"

if(!test-path -path "C:\windows\temp\Relay.addin")
    {
        try { Invoke-WebRequest -Uri $fileUrl -Headers @{'Authorization' = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(('{0}:{1}' -f 'HLDigitalSupport','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')))}  -OutFile "C:\windows\temp\Relay.addin" } catch { Write-Output $_ }
    }