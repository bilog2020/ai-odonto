FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY AiOdonto.slnx .
COPY src/AiOdonto.Api/AiOdonto.Api.csproj src/AiOdonto.Api/
RUN dotnet restore src/AiOdonto.Api/AiOdonto.Api.csproj

COPY src/ src/
RUN dotnet publish src/AiOdonto.Api/AiOdonto.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AiOdonto.Api.dll"]
