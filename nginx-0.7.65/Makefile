
default:	build

clean:
	rm -rf Makefile objs

build:
	$(MAKE) -f objs/Makefile

install:
	$(MAKE) -f objs/Makefile install

upgrade:
	../nginx-local/sbin/nginx -t

	kill -USR2 `cat ../nginx-local/logs/nginx.pid`
	sleep 1
	test -f ../nginx-local/logs/nginx.pid.oldbin

	kill -QUIT `cat ../nginx-local/logs/nginx.pid.oldbin`

upgrade1:
	# upgrade 0.1.x to 0.2+

	../nginx-local/sbin/nginx -t

	cp ../nginx-local/logs/nginx.pid ../nginx-local/logs/nginx.pid.oldbin

	kill -USR2 `cat ../nginx-local/logs/nginx.pid`
	sleep 1
	test -f ../nginx-local/logs/nginx.pid.oldbin

	cp ../nginx-local/logs/nginx.pid ../nginx-local/logs/nginx.pid.newbin

	kill -QUIT `cat ../nginx-local/logs/nginx.pid.oldbin`
