#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
WORKDIR /app
ENV ASPNETCORE_URLS https://127.0.0.1:888
ENV ASPNETCORE_URLS http://127.0.0.1:887
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=YourSecurePassword
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx

ADD ["src/MDD4All.SpecIF.Microservice/bin/Release/netcoreapp3.1/publish", "/app"]
COPY ["src/MDD4All.SpecIF.Microservice/bin/Release/netcoreapp3.1/localhost.pfx", "/https/"]
COPY ["src/MDD4All.SpecIF.Microservice/bin/Release/netcoreapp3.1/MDD4All.SpecIF.Microservice.xml", "/app/MDD4All.SpecIf.Microservice.xml"]


WORKDIR /app
ENTRYPOINT ["./MDD4All.SpecIf.Microservice"]
#funktioniert mit docker -t name build .
