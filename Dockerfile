FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY Host/Host.csproj Host/
COPY Web/Web.csproj Web/
COPY Business/Business.csproj Business/
COPY Data/Data.csproj Data/

RUN dotnet restore "Host/Host.csproj"

COPY . .
RUN dotnet publish "Host/Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Host.dll"]