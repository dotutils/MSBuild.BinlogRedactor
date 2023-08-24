# Usage help

[docs\Readme.md](docs\Readme.md)

# Setting up development

## Build

* `dotnet build` in the root

## Test run of the app

* place sample binlog into the build output folder (or any of it's subfolders)
* `Microsoft.Build.BinlogRedactor.CLI.exe -p "some secret" -r`

## Unit/Integration tests

* `dotnet tests`
