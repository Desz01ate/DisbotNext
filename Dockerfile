# Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY . /src

WORKDIR /src
RUN dotnet restore DisbotNext.sln

WORKDIR /src/DisbotNext.Infrastructures
RUN dotnet clean
RUN dotnet build DisbotNext.Infrastructures.Sqlite.csproj -c Release

WORKDIR /src/DisbotNext
RUN dotnet clean
RUN dotnet build DisbotNext.csproj -c Release

COPY . .
RUN dotnet publish -c Release -o /src/out


FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /src
COPY --from=build /src/out .

ENTRYPOINT ["dotnet", "DisbotNext.dll"]