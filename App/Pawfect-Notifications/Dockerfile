FROM mcr.microsoft.com/dotnet/sdk:9.0 AS server-build
WORKDIR /src
COPY . ./
RUN dotnet restore "./Pawfect-Notifications.csproj"
RUN dotnet publish "./Pawfect-Notifications.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=server-build /app/publish .

COPY --from=server-build /src/Templates ./Templates


ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8081

EXPOSE 8081

ENTRYPOINT ["dotnet", "Pawfect-Notifications.dll"]