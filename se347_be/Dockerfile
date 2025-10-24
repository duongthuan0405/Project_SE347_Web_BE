# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj(s) and restore as distinct layers
COPY ["se347_be/se347_be.csproj", "se347_be/"]
RUN dotnet restore "se347_be/se347_be.csproj"

# copy everything and publish
COPY . .
WORKDIR /src/se347_be
RUN dotnet publish "se347_be.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# optional: add a non-root user for security
# RUN useradd -m appuser && chown -R appuser /app
# USER appuser

COPY --from=build /app/publish .

# Use the PORT env var Render provides; ASP.NET listens on Kestrel's URLs from ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

# If you need to run DB migrations at container start, you can run a small shell script here.
ENTRYPOINT ["dotnet", "se347_be.dll"]
