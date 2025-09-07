FROM mcr.microsoft.com/dotnet/sdk:9.0 AS server-build
WORKDIR /src
COPY . ./
RUN dotnet restore "./Pawfect-Messenger.csproj"
RUN dotnet publish "./Pawfect-Messenger.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=server-build /app/publish .


ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8082

EXPOSE 8082

ENTRYPOINT ["dotnet", "Pawfect-Messenger.dll"]