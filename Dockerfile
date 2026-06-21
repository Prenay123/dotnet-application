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
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app

COPY --from=build /app/publish .

# ASP.NET Core Configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "SchoolApp.dll"]