# Build stage runs natively on the build host (no QEMU emulation of the SDK when cross-building,
# e.g. linux/amd64 from an Apple Silicon Mac). The publish output is portable IL, only the
# runtime stage below is platform specific.
#
#   docker buildx build --platform linux/amd64 -t harpywar/nfk-demoparser:latest --push .
#
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app

COPY . /app

RUN dotnet restore
RUN dotnet publish --no-restore -c release -o binaries demowebapi.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=builder /app/binaries .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
# parsed demo cache, mount a volume here (see docker-compose.yml)
ENV DemoCache__Directory=/app/cache

EXPOSE 8080

CMD [ "dotnet", "demowebapi.dll"]
