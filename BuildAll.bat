dotnet publish -r win-x64 -c Release -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-arm -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-arm64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained


cd C:\repos\PDSportsNet8\Build

copy /Y "C:\repos\PDSportsNet8\bin\Release\net8.0\win-x64\publish\PDSports.exe" .
"C:\Program Files\7-Zip\7z" a -tzip PDSports-WIN-8.zip PDSports.exe SampleFavorites.json "MoneyLine Odds Explained.txt" PDSports.pdf

copy /Y "C:\repos\PDSportsNet8\bin\Release\net8.0\osx-x64\publish\PDSports" .
"C:\Program Files\7-Zip\7z" a -t7z PDSports-OSX-8.7z PDSports SampleFavorites.json "MoneyLine Odds Explained.txt" PDSports.pdf

copy /Y "C:\repos\PDSportsNet8\bin\Release\net8.0\linux-x64\publish\PDSports" .
"C:\Program Files\7-Zip\7z" a -t7z PDSports-LIN64-8.7z PDSports SampleFavorites.json "MoneyLine Odds Explained.txt" PDSports.pdf

copy /Y "C:\repos\PDSportsNet8\bin\Release\net8.0\linux-arm\publish\PDSports" .
"C:\Program Files\7-Zip\7z" a -t7z PDSports-RasPi-8.7z PDSports SampleFavorites.json "MoneyLine Odds Explained.txt" PDSports.pdf

copy /Y "C:\repos\PDSportsNet8\bin\Release\net8.0\linux-arm64\publish\PDSports" .
"C:\Program Files\7-Zip\7z" a -t7z PDSports-RasPi64-8.7z PDSports SampleFavorites.json "MoneyLine Odds Explained.txt" PDSports.pdf


