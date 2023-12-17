FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder

WORKDIR /app
COPY . .

RUN dotnet publish "DeviceStateWeb.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runner

WORKDIR /app
COPY --from=builder /app/publish .
COPY template.db devices.db

ENTRYPOINT ["dotnet", "DeviceStateWeb.dll"]
