﻿FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS restore
WORKDIR /src

COPY ["Firepuma.WebPush.Domain/Firepuma.WebPush.Domain.csproj", "Firepuma.WebPush.Domain/"]
COPY ["Firepuma.WebPush.Infrastructure/Firepuma.WebPush.Infrastructure.csproj", "Firepuma.WebPush.Infrastructure/"]
COPY ["Firepuma.WebPush.Tests/Firepuma.WebPush.Tests.csproj", "Firepuma.WebPush.Tests/"]

COPY ["Firepuma.WebPush.Worker/Firepuma.WebPush.Worker.csproj", "Firepuma.WebPush.Worker/"]
RUN dotnet restore "Firepuma.WebPush.Worker/Firepuma.WebPush.Worker.csproj"
COPY . .


FROM restore AS test
RUN dotnet restore "Firepuma.WebPush.Tests/Firepuma.WebPush.Tests.csproj"
RUN dotnet test --no-restore --logger:trx --results-directory /testsresults


FROM test AS publish
ARG target_configuration=Release
WORKDIR /src/Firepuma.WebPush.Worker
RUN dotnet publish  "Firepuma.WebPush.Worker.csproj" -c ${target_configuration} --no-restore -o /app


FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final
LABEL maintainer="Francois Hill <francoishill11@gmail.com>"
WORKDIR /app
COPY --from=publish /app .

ARG BUILD_ID
ENV BUILD_ID="${BUILD_ID}"

ENTRYPOINT ["dotnet", "Firepuma.WebPush.Worker.dll"]