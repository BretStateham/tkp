Remove-Item -Recurse -Force -Path ./MicrosoftIntelIoTCamp 
Remove-Item -Recurse -Force -Path ./MicrosoftIntelIoTCampTest 
Remove-Item -Path ./MicrosoftIntelIoTCamp.zip
Remove-Item -Path ./MicrosoftIntelIoTCamp-master.zip
Write-Host "Downloading repo zip file. Please wait..."
# $wc = New-Object net.webclient
# $wc.Downloadfile("https://github.com/dxcamps/MicrosoftIntelIoTCamp/archive/master.zip", "./MicrosoftIntelIoTCamp-master.zip")
$ProgressPreference = 'SilentlyContinue'
Invoke-WebRequest -Uri "https://github.com/dxcamps/MicrosoftIntelIoTCamp/archive/master.zip" -OutFile "./MicrosoftIntelIoTCamp-master.zip"
Unblock-File -Path MicrosoftIntelIoTCamp-master.zip
Expand-Archive -Path MicrosoftIntelIoTCamp-master.zip -Destination ./ -Force
Rename-Item -Path ./MicrosoftIntelIoTCamp-master -NewName ./MicrosoftIntelIoTCamp
c:\t\TrainingKitPackager.exe c:\MicrosoftIntelIoTCampRelease\MicrosoftIntelIoTCamp
Compress-Archive -Path ./MicrosoftIntelIoTCamp/ -DestinationPath ./MicrosoftIntelIoTCamp.zip -Force
Expand-Archive -Path ./MicrosoftIntelIoTCamp.zip -Destination ./MicrosoftIntelIoTCampTest -Force

