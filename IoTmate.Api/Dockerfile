# https://aka.ms/customizecontainer
# Launching
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy projects files
COPY ["IoTmate.Api/IoTmate.Api.csproj", "IoTmate.Api/"]
COPY ["IoTmate.Dto/IoTmate.Dto.csproj", "IoTmate.Dto/"]
COPY ["IoTmate.Model/IoTmate.Model.csproj", "IoTmate.Model/"]
COPY ["IoTmate.Repository/IoTmate.Repository.csproj", "IoTmate.Repository/"]

RUN dotnet restore "IoTmate.Api/IoTmate.Api.csproj"

# Copy code
COPY . .

# Build project
WORKDIR "/src/IoTmate.Api"
RUN dotnet build "IoTmate.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IoTmate.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Finish
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTmate.Api.dll"]