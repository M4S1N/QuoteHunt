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

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    libglib2.0-0 \
    libnss3 \
    libx11-xcb1 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxi6 \
    libxtst6 \
    libcups2 \
    libxrandr2 \
    libasound2 \
    libatk1.0-0 \
    libpangocairo-1.0-0 \
    libatspi2.0-0 \
    libgtk-3-0 \
    ca-certificates \
    fonts-liberation \
    libappindicator3-1 \
    libgbm-dev \
    libxshmfence1 \
    chromium \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

EXPOSE 5208
ENV ASPNETCORE_URLS=http://+:5208
ENV SCRAPER_URL=https://quotes.toscrape.com
ENV SCRAPER_USERNAME=user
ENV SCRAPER_PASSWORD=password

ENTRYPOINT ["dotnet", "QuoteHuntWebAPI.Server.dll"]
