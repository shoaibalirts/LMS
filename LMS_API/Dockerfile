FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["LMS_API.csproj", "./"]
RUN dotnet restore "LMS_API.csproj"

COPY . .
RUN dotnet publish "LMS_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .


EXPOSE 8080

ENTRYPOINT ["dotnet", "LMS_API.dll"]