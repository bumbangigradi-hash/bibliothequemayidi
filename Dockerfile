
# Étape 1 : Construction de l'application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["bibliotheque_mayidi.csproj", "./"]

RUN dotnet restore "bibliotheque_mayidi.csproj"

COPY . .

RUN dotnet publish "bibliotheque_mayidi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Étape 2 : Exécution de l'application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "bibliotheque_mayidi.dll"]
