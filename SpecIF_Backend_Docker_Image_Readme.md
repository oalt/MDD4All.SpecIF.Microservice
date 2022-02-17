
# SpecIF Backend Setup

The following arguments can be added to the docker run command, to setup the SpecIF Backend container.

# Environment Variables

## Hosting certificate

Use the following variables:

-e ASPNETCORE_Kestrel__Certificates__Default__Password=\<password\>
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/\<certifcateName\>

And mount the certificate as a volume

-v \<PathToCertificateDirectory\>:/https/

## Database

To configure the connection to a MongoDB set:
-e dataConnection=mongodb://\<connectionstring\>:\<port\> (e.g. dataConnection=mongodb://mongodb:27017)

## Authorization

By default, this image starts with disabled authorization. To start a container __with authorization__ , use 
docker run \<other commmands\> --entrypoint ./MDD4All.SpecIf.Microservice \<imagename\>:\<imagetag\> mongodb false

*false* / *true* as 2nd run argument changes the authorization. *true* enables AnonynmousReadAccess

## Ports

The SpecIF Backend is accessible over 888 (https) and 887 (http, redirect to https)
-p \<outsideport\>:888 -p \<outsideport\>:887

## Access SpecIF

SpecIF is available at \<host\>:\<port\>, including an upload file endpoint and swagger documentation.

## Example docker run command

docker run 
-p 888:888 
-p 887:887 
-it 
-e dataConnection=mongodb://mongodb:27017 
-e ASPNETCORE_Kestrel__Certificates__Default__Password=password 
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx  
--entrypoint ./MDD4All.SpecIf.Microservice 
--network=specif 
-v %USERPROFILE%\.aspnet\https:/https/  
specif:latest 
mongodb true