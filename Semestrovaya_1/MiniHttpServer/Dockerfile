FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish MiniHttpServer/MiniHttpServer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8888
ENTRYPOINT ["dotnet", "MiniHttpServer.dll"]