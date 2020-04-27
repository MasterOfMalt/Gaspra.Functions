# Project Name to build and pack
$ProjectName = "Gaspra."
$Version = "1.0.0-local"

#Restore paket
dotnet tool restore
dotnet paket restore

#Build projects release
dotnet build ./src/$ProjectName.sln -c Release

#Pack as nuget
dotnet pack ./src/$ProjectName.sln -c Release -o ./.pack /p:Version=$Version