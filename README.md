# Gaspra.Functions

#### Contents

- [Build Status](#Build-Status)
- [Install](#Install)
- [Uninstall](#Uninstall)
- [Usage](#Usage)

#### Build Status
[![publish to nuget](https://github.com/Gaspra/Gaspra.DatabaseUtility/workflows/publish%20to%20nuget/badge.svg?branch=master)](https://www.nuget.org/packages/Gaspra.Functions)

#### Install
As this is a dotnet tool you can install using the dotnet CLI:

```dotnet tool install gaspra.functions``` (append `-g` if you'd like it installed globally)

![image](https://user-images.githubusercontent.com/35634732/103634248-b8af6080-4f3e-11eb-97c3-90fc76f170d1.png)

#### Uninstall
To uninstall simply run:

```dotnet uninstall gaspra.functions``` (use `-g` again if you installed it globally)

![image](https://user-images.githubusercontent.com/35634732/103634620-3c694d00-4f3f-11eb-9ec2-bb6bd98699ef.png)

#### Usage

##### Merge Sprocs
Just run `gaspra.functions ms -c "connection string"`, this will run the merge sproc generator against the database in the connection string and create a folder from wherever you're calling the function from called `.output` which will contain all the scripts.