FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SACS/SACS.csproj", "SACS/"]
RUN dotnet restore "SACS/SACS.csproj"
COPY . .
WORKDIR "/src/SACS"
RUN dotnet build "SACS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SACS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SACS.dll"]
