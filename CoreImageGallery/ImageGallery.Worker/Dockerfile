FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-stretch AS build
WORKDIR /src
COPY ["ImageGallery.Worker/ImageGallery.Worker.csproj", "ImageGallery.Worker/"]
COPY ["ImageGallery.AzFunctions/ImageGallery.AzFunctions.csproj", "ImageGallery.AzFunctions/"]
COPY ["ImageGallery.Model/ImageGallery.Model.csproj", "ImageGallery.Model/"]
RUN dotnet restore "ImageGallery.Worker/ImageGallery.Worker.csproj"
COPY . .
WORKDIR "/src/ImageGallery.Worker"
RUN dotnet build "ImageGallery.Worker.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ImageGallery.Worker.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ImageGallery.Worker.dll"]