FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 AS builder

WORKDIR /src

COPY ["src/Skeletron/", "Skeletron/"]
COPY ["src/Skeletron.Persistence/", "Skeletron.Persistence/"]
COPY ["src/Osu.NET.Api/", "Osu.NET.Api/"]

RUN dotnet restore "Skeletron/Skeletron.csproj"

COPY . /src
WORKDIR /src/Skeletron
RUN dotnet publish "Skeletron.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runner

WORKDIR /app
COPY --from=builder /app/publish ./

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "Skeletron.dll"]
