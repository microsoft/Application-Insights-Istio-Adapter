FROM alpine:3.7
#FROM microsoft/dotnet:2.1.0-aspnetcore-runtime-alpine
#FROM mcr.microsoft.com/dotnet/core/runtime:latest
RUN apk update && apk add libintl libstdc++ openssl libc6-compat
COPY / /istio-mixer-plugin
WORKDIR /istio-mixer-plugin
EXPOSE 6789
ENTRYPOINT ["./Microsoft.IstioMixerPlugin.ConsoleHost", "noninteractive"]