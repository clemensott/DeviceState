FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder

WORKDIR /app
COPY . .

RUN dotnet restore
RUN dotnet publish "DeviceStateWeb.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runner

WORKDIR /app
COPY --from=builder /app/publish .
COPY template.db devices.

ENV ASPNETCORE_URLS=http://0.0.0.0:80

ENTRYPOINT ["dotnet", "DeviceStateWeb.dll"]
