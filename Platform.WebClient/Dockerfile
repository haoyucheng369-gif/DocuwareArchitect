FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Platform.Identity/Platform.Identity.csproj", "Platform.Identity/"]
COPY ["Platform.DotNetApi/Platform.DotNetApi.csproj", "Platform.DotNetApi/"]
COPY ["Platform.WebClient/Platform.WebClient.csproj", "Platform.WebClient/"]
RUN dotnet restore "Platform.WebClient/Platform.WebClient.csproj"

COPY . .
WORKDIR /src/Platform.WebClient
RUN dotnet publish "Platform.WebClient.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Platform.WebClient.dll"]
