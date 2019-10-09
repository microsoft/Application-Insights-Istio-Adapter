FROM alpine:3.10.2
RUN apk update && apk add libintl libstdc++ openssl libc6-compat
COPY / /istio-mixer-plugin
WORKDIR /istio-mixer-plugin
EXPOSE 6789
ENTRYPOINT ["./Microsoft.IstioMixerPlugin.ConsoleHost", "noninteractive"]