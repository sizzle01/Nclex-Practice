# Three images from one solution (architecture.md section 8), picked with --target:
#   api         Nclex.Api on aspnet:10.0
#   worker      Nclex.Worker on aspnet:10.0 (also runs "import-questions" to load content)
#   migrations  the EF Core migration bundle, run once per deploy before the new api starts
# Build from this folder:  docker build --target api -t nclex-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, from the project files only, so the layer is reused until a dependency changes.
COPY .editorconfig global.json Directory.Build.props Directory.Packages.props NclexPractice.sln dotnet-tools.json ./
COPY src/Nclex.Domain/Nclex.Domain.csproj src/Nclex.Domain/
COPY src/Nclex.Application/Nclex.Application.csproj src/Nclex.Application/
COPY src/Nclex.Infrastructure/Nclex.Infrastructure.csproj src/Nclex.Infrastructure/
COPY src/Nclex.Api/Nclex.Api.csproj src/Nclex.Api/
COPY src/Nclex.Worker/Nclex.Worker.csproj src/Nclex.Worker/
RUN dotnet restore src/Nclex.Api/Nclex.Api.csproj \
 && dotnet restore src/Nclex.Worker/Nclex.Worker.csproj \
 && dotnet tool restore

COPY src/ src/
RUN dotnet publish src/Nclex.Api/Nclex.Api.csproj -c Release --no-restore -o /out/api \
 && dotnet publish src/Nclex.Worker/Nclex.Worker.csproj -c Release --no-restore -o /out/worker

# The bundle is one executable that applies pending migrations, so the deploy job needs no SDK.
# AppDbContextFactory creates the context, so the bundle never starts the API host (which would
# need the API's logging, auth and secrets just to migrate).
RUN dotnet ef migrations bundle \
      --project src/Nclex.Infrastructure --startup-project src/Nclex.Api \
      --configuration Release --output /out/migrations/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=build /out/api .
# Media is on local disk until S3 storage exists (docs/operations.md). Docker copies this folder's
# owner into a new named volume, so the non-root user can write there.
RUN mkdir -p /data/media && chown "$APP_UID" /data/media
# The base image's non-root user; the API listens on 8080 (ASPNETCORE_HTTP_PORTS).
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nclex.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS worker
WORKDIR /app
COPY --from=build /out/worker .
# Media is on local disk until S3 storage exists (docs/operations.md). Docker copies this folder's
# owner into a new named volume, so the non-root user can write there.
RUN mkdir -p /data/media && chown "$APP_UID" /data/media
USER $APP_UID
ENTRYPOINT ["dotnet", "Nclex.Worker.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrations
# aspnet, not runtime: Nclex.Infrastructure references the ASP.NET Core shared framework.
WORKDIR /app
COPY --from=build /out/migrations .
USER $APP_UID
# Pass the connection string at run time:  docker run nclex-migrations --connection "Host=..."
ENTRYPOINT ["./efbundle"]
