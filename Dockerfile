# Stage: Development

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as development
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ApplicationInsights:InstrumentationKey=KEY_APPLICATION_INSIGHTS
ENV IdentityServer:ProviderKey=PROVIDER_KEY
ENV IdentityServer:Authority=AUTHORITY
ENV IdentityServer:ApiName=API_NAME
ENV IdentityServer:ApiSecret=API_SECRET
RUN apt update && \
    apt install unzip && \
    curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

WORKDIR /app

EXPOSE 5000
EXPOSE 5001

COPY . .

RUN dotnet restore
RUN dotnet build "./src/Wiz.Template.Gateway/Wiz.Template.Gateway.csproj" -c Debug
CMD dotnet watch --project src/Wiz.Template.Gateway/Wiz.Template.Gateway.csproj run

# Stage: Staging/Production

FROM development as build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build "./src/Wiz.Template.Gateway/Wiz.Template.Gateway.csproj" -c Release

FROM build AS publish
RUN dotnet publish "./src/Wiz.Template.Gateway/Wiz.Template.Gateway.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS production
ENV ASPNETCORE_ENVIRONMENT=Staging
WORKDIR /app
COPY --from=publish /publish .
ENTRYPOINT ["dotnet", "Wiz.Template.Gateway.dll"]
EXPOSE 80