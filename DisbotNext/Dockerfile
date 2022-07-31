#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DisbotNext/DisbotNext.csproj", "DisbotNext/"]
COPY ["DisbotNext.Common/DisbotNext.Common.csproj", "DisbotNext.Common/"]
COPY ["DisbotNext.Infrastructure.Common/DisbotNext.Infrastructures.Common.csproj", "DisbotNext.Infrastructure.Common/"]
COPY ["DisbotNext.Infrastructures/DisbotNext.Infrastructures.Sqlite.csproj", "DisbotNext.Infrastructures/"]
RUN dotnet restore "DisbotNext/DisbotNext.csproj"
COPY . .
WORKDIR "/src/DisbotNext"
RUN dotnet build "DisbotNext.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DisbotNext.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DisbotNext.dll"]