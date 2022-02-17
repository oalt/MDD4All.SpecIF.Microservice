#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-buster-slim

ENV ASPNETCORE_URLS https://localhost:888
ENV ASPNETCORE_URLS http://localhost:887
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=

ADD ["src/MDD4All.SpecIF.Microservice/bin/Release/netcoreapp3.1/publish", "/app"]
COPY ["localhost.pfx", "/https/localhost.pfx"]
COPY ["src/MDD4All.SpecIF.Microservice/bin/Release/netcoreapp3.1/MDD4All.SpecIF.Microservice.xml", "/app/MDD4All.SpecIF.Microservice.xml"]

WORKDIR /app
ENTRYPOINT ["./MDD4All.SpecIf.Microservice"]
#funktioniert mit docker -t name build .
