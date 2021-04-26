FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG GITHUB_USER
ARG GITHUB_PASSWORD
ENV GITHUB_USER=${GITHUB_USER}
ENV GITHUB_PASSWORD=${GITHUB_PASSWORD}
WORKDIR /app

COPY emulator-backend-8080.csproj Nuget.config ./
RUN dotnet restore --configfile Nuget.config

COPY . .
RUN dotnet publish -c release -o /app/output --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/output ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "emulator-backend-8080.dll"]