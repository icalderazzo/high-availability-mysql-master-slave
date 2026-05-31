FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files to leverage Docker layer caching for restores
COPY OrdersService.sln ./
COPY src/OrdersService.API/OrdersService.API.csproj src/OrdersService.API/
COPY src/OrdersService.Application/OrdersService.Application.csproj src/OrdersService.Application/
COPY src/OrdersService.Infrastructure/OrdersService.Infrastructure.csproj src/OrdersService.Infrastructure/
COPY src/OrdersService.Domain/OrdersService.Domain.csproj src/OrdersService.Domain/

RUN dotnet restore "OrdersService.sln"

# Copy the rest of the source
COPY . .

WORKDIR /src/src/OrdersService.API

# Publish the app into a self-contained set of files (no platform-specific apphost)
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Listen on port 80 inside the container
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrdersService.API.dll"]


