FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/HabitTracker.Domain/HabitTracker.Domain.csproj", "src/HabitTracker.Domain/"]
COPY ["src/HabitTracker.Application/HabitTracker.Application.csproj", "src/HabitTracker.Application/"]
COPY ["src/HabitTracker.Infrastructure/HabitTracker.Infrastructure.csproj", "src/HabitTracker.Infrastructure/"]
COPY ["src/HabitTracker.Api/HabitTracker.Api.csproj", "src/HabitTracker.Api/"]
RUN dotnet restore "src/HabitTracker.Api/HabitTracker.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/HabitTracker.Api/HabitTracker.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HabitTracker.Api.dll"]
