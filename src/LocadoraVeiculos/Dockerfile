# Imagem da API para a versão online (Cloud Run).
# Roda no modo demo: banco em memória + frota de exemplo, sem SQL Server.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY LocadoraVeiculos.csproj ./
RUN dotnet restore LocadoraVeiculos.csproj
COPY . ./
RUN dotnet publish LocadoraVeiculos.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_HTTP_PORTS=8080 \
    BancoEmMemoria=true \
    PopularVeiculosDeExemplo=true
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "LocadoraVeiculos.dll"]
