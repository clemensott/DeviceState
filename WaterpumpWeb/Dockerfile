FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder

RUN apt-get update
RUN apt-get install npm -y

WORKDIR /app
COPY . .

RUN dotnet publish "WaterpumpWeb.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runner

WORKDIR /app
COPY --from=builder /app/publish .
COPY template.db waterpump.db

ENTRYPOINT ["dotnet", "WaterpumpWeb.dll"]
