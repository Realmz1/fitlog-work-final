FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Render's containers hit the host inotify instance limit, which makes the
# default FileSystemWatcher on appsettings.json throw at startup. Polling avoids
# inotify entirely; the cost is irrelevant here since config never changes at runtime.
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "FitLog.dll"]