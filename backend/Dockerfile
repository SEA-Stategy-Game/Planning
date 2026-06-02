FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire backend source
COPY . .

# Restore and publish the API project (this automatically resolves project references)
WORKDIR /src/PlanBackend.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Default port if none is provided
ENV PORT=8080
ENV CoreBaseUrl=http://host.docker.internal:8085
EXPOSE 5000

# Run the application and bind it to the port specified by the PORT environment variable
ENTRYPOINT ["sh", "-c", "dotnet PlanBackend.Api.dll --urls http://0.0.0.0:${PORT}"]
