FROM node:20-alpine AS client-build

RUN npm i -g @angular/cli@18

WORKDIR /src/Client

COPY Client/package*.json ./
RUN npm install --force 

COPY Client/ ./

RUN if [ -f /etc/secrets/environment.Production.ts ]; then \
      cp /etc/secrets/environment.Production.ts ./src/environments/environment.Production.ts; \
    fi
RUN if [ -f /etc/secrets/environment.Development.ts ]; then \
      cp /etc/secrets/environment.Development.ts ./src/environments/environment.Development.ts; \
    fi

ENV NODE_OPTIONS="--max-old-space-size=512"

RUN ng build --base-href=/ pawfect-match-angular --configuration=production --build-optimizer=false

RUN mkdir -p /static && cp -r ../wwwroot/* /static/


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS server-build
WORKDIR /src

COPY . ./

# --- Copy backend secret config if it exists ---
RUN if [ -f /etc/secrets/environment.json ]; then \
      mkdir -p /src/Pawfect-API/Configuration/Production && \
      cp /etc/secrets/environment.json /src/Pawfect-API/Configuration/Production/environment.json; \
    fi

COPY --from=client-build /static ./wwwroot

RUN dotnet restore "./Pawfect-API.csproj"
RUN dotnet publish "./Pawfect-API.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

COPY --from=server-build /app/publish .
COPY --from=client-build /static ./wwwroot
COPY --from=server-build /src/AiTemplates ./AiTemplates

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Pawfect-API.dll"]
