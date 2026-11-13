# Build a partir da raiz do repositório:  docker build -f src/Dockerfile .
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Financas.csproj", "src/"]
RUN dotnet restore "src/Financas.csproj"
COPY src/ src/
RUN dotnet publish "src/Financas.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Financas.dll"]
