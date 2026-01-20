# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MemberOpsAPI/MemberOpsAPI.csproj", "MemberOpsAPI/"]
RUN dotnet restore "MemberOpsAPI/MemberOpsAPI.csproj"

COPY . .
WORKDIR "/src/MemberOpsAPI"
RUN dotnet build "MemberOpsAPI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MemberOpsAPI.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Fly.io uses port 8080 by default
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MemberOpsAPI.dll"]
