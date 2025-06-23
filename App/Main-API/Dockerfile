###############  Stage 1 – Build the Angular front-end  ###############
FROM node:20-alpine AS client-build

# Angular CLI for ng build
RUN npm i -g @angular/cli@18

WORKDIR /src/Client

COPY Client/package*.json ./
RUN npm ci --legacy-peer-deps       

COPY Client/ ./
RUN ng build --base-href=/ pawfect-match-angular

RUN mkdir -p /static && cp -r ../wwwroot/* /static/



###############  Stage 2 – Build / publish the ASP.NET Core API  ###############
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS server-build
WORKDIR /src

COPY . ./

COPY --from=client-build /static ./wwwroot

RUN dotnet restore "./Main-API.csproj"
RUN dotnet publish "./Main-API.csproj" -c Release -o /app/publish


###############  Stage 3 – Final, lightweight runtime  ###############
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

COPY --from=server-build /app/publish .
COPY --from=client-build /static ./wwwroot


ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Main-API.dll"]
