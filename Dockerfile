FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/FileUploadAndReport.Demo.Api/FileUploadAndReport.Demo.Api.csproj", "src/FileUploadAndReport.Demo.Api/"]
RUN dotnet restore "src/FileUploadAndReport.Demo.Api/FileUploadAndReport.Demo.Api.csproj"
COPY . .
WORKDIR "/src/src/FileUploadAndReport.Demo.Api"
RUN dotnet build "FileUploadAndReport.Demo.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "FileUploadAndReport.Demo.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/data
VOLUME ["/app/data"]
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileUploadAndReport.Demo.Api.dll"]
