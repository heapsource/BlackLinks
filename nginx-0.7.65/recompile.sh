echo > ../nginx-local/logs/error.log
sudo killall nginx
make
make install
gmcs -unsafe -debug -r System.dll -r Mono.Posix.dll -r System.Runtime.Remoting.dll -r:System.Web.dll -r:../nginx-hello/BlackLinks/bin/Debug/BlackLinks.dll -target:library -out:../nginx-hello/NginxBlackLinks.dll ../nginx-hello/main.cs ../nginx-hello/NginxBlackHostManager.cs ../nginx-hello/NginxBlackRequest.cs
../nginx-local/sbin/nginx

cp ../nginx-hello/NginxBlackLinks.dll* ../nginx-hello/sampleApplication/bin/Debug/

cp ../nginx-hello/NginxBlackLinks.dll* ../nginx-local/sbin
cp ../nginx-hello/BlackLinks/bin/Debug/BlackLinks.dll* ../nginx-local/sbin
