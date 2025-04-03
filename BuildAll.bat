dotnet publish -r win-x64 -c Release -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-arm -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-arm64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained


cd C:\repos\PhilipsHueWebhookHandler\Build

copy /Y "C:\repos\PhilipsHueWebhookHandler\bin\Release\net8.0\win-x64\publish\PhilipsHueWebhookHandler.exe" .
"C:\Program Files\7-Zip\7z" a -tzip PhilipsHueWebhookHandler-WIN-8.zip PhilipsHueWebhookHandler.exe SampleConfig.json PhilipsHueWebhookHandler.pdf

copy /Y "C:\repos\PhilipsHueWebhookHandler\bin\Release\net8.0\osx-x64\publish\PhilipsHueWebhookHandler" .
"C:\Program Files\7-Zip\7z" a -t7z PhilipsHueWebhookHandler-OSX-8.7z PhilipsHueWebhookHandler SampleConfig.json PhilipsHueWebhookHandler.pdf

copy /Y "C:\repos\PhilipsHueWebhookHandler\bin\Release\net8.0\linux-x64\publish\PhilipsHueWebhookHandler" .
"C:\Program Files\7-Zip\7z" a -t7z PhilipsHueWebhookHandler-LIN64-8.7z PhilipsHueWebhookHandler SampleConfig.json PhilipsHueWebhookHandler.pdf

copy /Y "C:\repos\PhilipsHueWebhookHandler\bin\Release\net8.0\linux-arm\publish\PhilipsHueWebhookHandler" .
"C:\Program Files\7-Zip\7z" a -t7z PhilipsHueWebhookHandler-RasPi-8.7z PhilipsHueWebhookHandler SampleConfig.json PhilipsHueWebhookHandler.pdf

copy /Y "C:\repos\PhilipsHueWebhookHandler\bin\Release\net8.0\linux-arm64\publish\PhilipsHueWebhookHandler" .
"C:\Program Files\7-Zip\7z" a -t7z PhilipsHueWebhookHandler-RasPi64-8.7z PhilipsHueWebhookHandler SampleConfig.json PhilipsHueWebhookHandler.pdf


