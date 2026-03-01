FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Backend.sln ./
COPY src/Backend.Api/Backend.Api.csproj src/Backend.Api/
COPY src/Backend.Application/Backend.Application.csproj src/Backend.Application/
COPY src/Backend.Domain/Backend.Domain.csproj src/Backend.Domain/
COPY src/Backend.Infrastructure/Backend.Infrastructure.csproj src/Backend.Infrastructure/
RUN dotnet restore src/Backend.Api/Backend.Api.csproj
COPY src/ ./src/
RUN dotnet publish src/Backend.Infrastructure/Backend.Infrastructure.csproj -c Release -o /app/infra
RUN dotnet publish src/Backend.Api/Backend.Api.csproj -c Release -o /app/publish
RUN cp -r /app/infra/* /app/publish/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Backend.Api.dll"]
