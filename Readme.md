# Usage help

[docs\Readme.md](docs\Readme.md)

# Setting up development

## Prerequisities (until https://github.com/dotnet/msbuild/pull/9132 MSBuild changes are merged and published)

* create `.offline-packages` folder in root of your enlistment
* checkout the MSBuild on the branch from PR https://github.com/dotnet/msbuild/pull/9132
* build, then `dotnet pack` the `Microsoft.Build`, `Microsoft.Build.Framework` and `Microsoft.NET.StringTools` packages and copy them to the `.offline-packages`
* update the version of `Microsoft.Build` in `packages.props` - so that it matches the version you put into `.offline-packages`

## Build

* `dotnet build` in the root

## Test run

* place sample binlog into the build output folder (or any of it's subfolders)
* `Microsoft.Build.BinlogRedactor.CLI.exe -p "some secret" -r`

