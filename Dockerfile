#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM nmchardy/specif:latest
WORKDIR /app
ENV ASPNETCORE_URLS https://localhost:888
ENV ASPNETCORE_URLS http://localhost:887

 

ENV ASPNETCORE_Kestrel__Certificates__Default__Password=YourSecurePassword
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx

ADD ["src/MDD4All.SpecIF.Microservice/bin/Release/netcoreapp3.1/publish", "/app"]

COPY ["src/MDD4All.SpecIF.Microservice/bin/Release/netcoreapp3.1/MDD4All.SpecIF.Microservice.xml", "/app/MDD4All.SpecIF.Microservice.xml"]


WORKDIR /app
ENTRYPOINT ["./MDD4All.SpecIF.Microservice"]
#funktioniert mit docker -t name build .
