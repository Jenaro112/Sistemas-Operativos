# Contexto de build: carpeta Ejercicio3-Lamport (para incluir Lamport.Common)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Lamport.Common/ Lamport.Common/
COPY InventoryService/ InventoryService/
RUN dotnet publish InventoryService/InventoryService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "InventoryService.dll"]
