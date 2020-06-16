d:
cd \
cd consul
start "Consul" consul agent -dev
timeout /T 30 /nobreak
start "MongoDB Server" mongod --dbpath=d:\specifdb
cd \
cd kafka
start "Zookeeper" bin\windows\zookeeper-server-start.bat config\zookeeper.properties
timeout /T 10 /nobreak
start "Kafka" bin\windows\kafka-server-start.bat config\server.properties
