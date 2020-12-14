Remove-Item -Path .\External -Recurse -Force
New-Item -Path .\External -ItemType Directory
Copy-Item -Path ..\FileToAzureIoTHub\Senders\EnergyManager.cs .\External
Copy-Item -Path ..\IoTDataTranslator\IoTMessage.cs .\External
Copy-Item -Path ..\IoTDataTranslator\loriotmessage.cs .\External