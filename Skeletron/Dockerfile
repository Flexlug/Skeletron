#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Skeletron/Skeletron.csproj", "Skeletron/"]
COPY ["Osu.NET.Api/Osu.NET.Api.csproj", "Osu.NET.Api/"]
COPY ["Osu.NET.Recognizer/Osu.NET.Recognizer.csproj", "Osu.NET.Recognizer/"]
RUN dotnet restore "Skeletron/Skeletron.csproj"
COPY . .
WORKDIR "/src/Skeletron"
RUN dotnet publish "Skeletron.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "Skeletron.dll"]