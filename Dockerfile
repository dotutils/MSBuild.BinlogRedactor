FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build

WORKDIR /src
#COPY *.sln .
#COPY ./src/Microsoft.Build.BinlogRedactor/Microsoft.Build.BinlogRedactor.csproj src/Microsoft.Build.BinlogRedactor/Microsoft.Build.BinlogRedactor.csproj
#COPY ./src/Microsoft.Build.BinlogRedactor.CLI/Microsoft.Build.BinlogRedactor.CLI.csproj src/Microsoft.Build.BinlogRedactor.CLI/Microsoft.Build.BinlogRedactor.CLI.csproj
#COPY ./test/Microsoft.Build.BinlogRedactor.Tests/Microsoft.Build.BinlogRedactor.Tests.csproj test/Microsoft.Build.BinlogRedactor.Tests/Microsoft.Build.BinlogRedactor.Tests.csproj
#copying just the project files for restore is more optimal (layering) - but for simplicity let's copy everything in one go (as we need props, dependabot etc.)
COPY . .

RUN dotnet restore
#COPY . .
WORKDIR "/src/."

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0-preview
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.Build.BinlogRedactor.CLI.dll"]