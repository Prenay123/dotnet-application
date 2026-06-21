# ==========================
# Build Stage
# ==========================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["SchoolApp.csproj", "./"]

RUN dotnet restore

COPY . .

RUN dotnet publish \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ==========================
# Runtime Stage
# ==========================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Create non-root user
RUN groupadd -r appgroup && \
    useradd -r -g appgroup appuser

WORKDIR /app

COPY --from=build /app/publish .

RUN chown -R appuser:appgroup /app

USER appuser

# ASP.NET Core Configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "SchoolApp.dll"]