FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Platform.RestApi/Platform.RestApi.csproj", "Platform.RestApi/"]
RUN dotnet restore "Platform.RestApi/Platform.RestApi.csproj"

COPY . .
WORKDIR /src/Platform.RestApi
RUN dotnet publish "Platform.RestApi.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Platform.RestApi.dll"]
