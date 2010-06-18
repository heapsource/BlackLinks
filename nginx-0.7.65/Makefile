
default:	build

clean:
	rm -rf Makefile objs

build:
	$(MAKE) -f objs/Makefile

install:
	$(MAKE) -f objs/Makefile install

upgrade:
	/home/chamo/nginx-local/sbin/nginx -t

	kill -USR2 `cat /home/chamo/nginx-local/logs/nginx.pid`
	sleep 1
	test -f /home/chamo/nginx-local/logs/nginx.pid.oldbin

	kill -QUIT `cat /home/chamo/nginx-local/logs/nginx.pid.oldbin`

upgrade1:
	# upgrade 0.1.x to 0.2+

	/home/chamo/nginx-local/sbin/nginx -t

	cp /home/chamo/nginx-local/logs/nginx.pid /home/chamo/nginx-local/logs/nginx.pid.oldbin

	kill -USR2 `cat /home/chamo/nginx-local/logs/nginx.pid`
	sleep 1
	test -f /home/chamo/nginx-local/logs/nginx.pid.oldbin

	cp /home/chamo/nginx-local/logs/nginx.pid /home/chamo/nginx-local/logs/nginx.pid.newbin

	kill -QUIT `cat /home/chamo/nginx-local/logs/nginx.pid.oldbin`
