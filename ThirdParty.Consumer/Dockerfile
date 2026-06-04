FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Platform.DotNetSdk/Platform.DotNetSdk.csproj", "Platform.DotNetSdk/"]
COPY ["ThirdParty.Consumer/ThirdParty.Consumer.csproj", "ThirdParty.Consumer/"]
RUN dotnet restore "ThirdParty.Consumer/ThirdParty.Consumer.csproj"

COPY . .
WORKDIR /src/ThirdParty.Consumer
RUN dotnet publish "ThirdParty.Consumer.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ThirdParty.Consumer.dll"]
