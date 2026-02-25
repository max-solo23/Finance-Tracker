FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["FinanceTracker.Api/FinanceTracker.Api.csproj", "FinanceTracker.Api/"]
COPY ["FinanceTracker/FinanceTracker.csproj", "FinanceTracker/"]

RUN dotnet restore "FinanceTracker.Api/FinanceTracker.Api.csproj"

COPY . .

RUN dotnet publish "FinanceTracker.Api/FinanceTracker.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet", "FinanceTracker.Api.dll" ]
