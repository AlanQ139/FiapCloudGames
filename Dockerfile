# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o csproj e restaura as dependências
COPY GameService/GameService.csproj GameService/
RUN dotnet restore GameService/GameService.csproj

# Copia o restante do código e publica
COPY . .
RUN dotnet publish GameService/GameService.csproj -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Instalar netcat (necessário pro wait-for-db.sh)
RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

# Expõe a porta padrão
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copia e habilita o script de espera do banco
COPY wait-for-db.sh /wait-for-db.sh
RUN chmod +x /wait-for-db.sh

# Usa o script para aguardar o banco antes de rodar a API
ENTRYPOINT ["/wait-for-db.sh", "mssql", "dotnet", "GameService.dll"]
