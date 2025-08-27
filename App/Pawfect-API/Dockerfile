FROM node:20-alpine AS client-build

# Angular CLI for ng build
RUN npm i -g @angular/cli@18

WORKDIR /src/Client

COPY Client/package*.json ./
RUN npm ci --legacy-peer-deps       

COPY Client/ ./
RUN ng build --base-href=/ pawfect-match-angular --configuration=production --delete-output-path --build-optimizer=false


RUN mkdir -p /static && cp -r ../wwwroot/* /static/



FROM mcr.microsoft.com/dotnet/sdk:9.0 AS server-build
WORKDIR /src

COPY . ./

COPY --from=client-build /static ./wwwroot

RUN dotnet restore "./Pawfect-API.csproj"
RUN dotnet publish "./Pawfect-API.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

COPY --from=server-build /app/publish .
COPY --from=client-build /static ./wwwroot


ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Pawfect-API.dll"]
