# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./

WORKDIR /src/QuoteHuntScraper.Server
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./

EXPOSE 5033
ENV ASPNETCORE_URLS=http://+:5033

ENV SCRAPER_URL=https://quotes.toscrape.com
ENV SCRAPER_USERNAME=user
ENV SCRAPER_PASSWORD=password

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5033/internal/Scraper/health || exit 1
ENTRYPOINT ["dotnet", "QuoteHuntScraper.Server.dll"]
