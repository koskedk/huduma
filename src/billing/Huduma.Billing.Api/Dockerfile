FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/billing/Huduma.Billing.Api/Huduma.Billing.Api.csproj", "Huduma.Billing.Api/"]
RUN dotnet restore "src/billing/Huduma.Billing.Api/Huduma.Billing.Api.csproj"
COPY . .
WORKDIR "/src/Huduma.Billing.Api"
RUN dotnet build "Huduma.Billing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Huduma.Billing.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Huduma.Billing.Api.dll"]
