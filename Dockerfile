# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FiapCloudGames/GameService/GameService.csproj GameService/
COPY SharedMessages/SharedMessages.csproj SharedMessages/

RUN dotnet restore GameService/GameService.csproj

COPY FiapCloudGames/GameService/. GameService/
COPY SharedMessages/. SharedMessages/

RUN dotnet publish GameService/GameService.csproj -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "GameService.dll"]
