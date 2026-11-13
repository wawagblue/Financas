FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY services/auth/TccProway.Api.csproj services/auth/
RUN dotnet restore services/auth/TccProway.Api.csproj
COPY . .
RUN dotnet publish services/auth/TccProway.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TccProway.Api.dll"]
