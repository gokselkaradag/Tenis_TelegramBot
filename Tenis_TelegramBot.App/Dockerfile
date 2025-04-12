FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Chrome ve ChromeDriver kur
RUN apt-get update && apt-get install -y \
    wget unzip gnupg ca-certificates \
    libasound2 libnspr4 libnss3 libxss1 libx11-xcb1 \
    xdg-utils libu2f-udev libvulkan1 fonts-liberation \
    libatk-bridge2.0-0 libgtk-3-0 libgbm1 \
 && wget -O /tmp/chrome-linux64.zip https://storage.googleapis.com/chrome-for-testing-public/135.0.7049.84/linux64/chrome-linux64.zip \
 && unzip /tmp/chrome-linux64.zip -d /opt/ \
 && ln -s /opt/chrome-linux64/chrome /usr/bin/google-chrome \
 && wget -O /tmp/chromedriver.zip https://storage.googleapis.com/chrome-for-testing-public/135.0.7049.84/linux64/chromedriver-linux64.zip \
 && unzip /tmp/chromedriver.zip -d /usr/local/bin/ \
 && mv /usr/local/bin/chromedriver-linux64/chromedriver /usr/bin/chromedriver \
 && chmod +x /usr/bin/chromedriver \
 && rm -rf /tmp/*.zip

ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Tenis_TelegramBot.App.dll"]
