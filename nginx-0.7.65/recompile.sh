echo > ../nginx-local/logs/error.log
sudo killall nginx
make
make install
gmcs -unsafe -r:System.Web.dll -r:../nginx-hello/BlackLinks/bin/Debug/BlackLinks.dll -target:library -out:../nginx-hello/main.dll ../nginx-hello/main.cs ../nginx-hello/NginxBlackHostManager.cs ../nginx-hello/NginxBlackRequest.cs
../nginx-local/sbin/nginx

cp ../nginx-hello/BlackLinks/bin/Debug/BlackLinks.dll  ../nginx-hello/
cp ../nginx-hello/main.dll ../nginx-hello/sampleApplication/bin/Debug/
