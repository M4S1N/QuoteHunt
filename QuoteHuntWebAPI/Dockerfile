# Stage 0: Test
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test
WORKDIR /src
COPY . ./

WORKDIR /src/QuoteHuntWebAPI.UnitTest
RUN dotnet restore
RUN dotnet test

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./

WORKDIR /src/QuoteHuntWebAPI.Server
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 5208
ENV ASPNETCORE_URLS=http://+:5208

ENTRYPOINT ["dotnet", "QuoteHuntWebAPI.Server.dll"]
