# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copia csproj e nuget.config
COPY FiapCloudGames/GameService/GameService.csproj GameService/
COPY FiapCloudGames/nuget.config .

# restore (baixa o SharedMessages do GitHub Packages)
RUN dotnet restore GameService/GameService.csproj

# copia o código
COPY FiapCloudGames/GameService/. GameService/

# publish
RUN dotnet publish GameService/GameService.csproj -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "GameService.dll"]
