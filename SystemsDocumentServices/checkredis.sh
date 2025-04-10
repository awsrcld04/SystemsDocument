#!/bin/sh

if [ -f "/var/run/redis.pid" ]; then

python /apps/redis-3.0.7/redisshutdown.py

else

sh /apps/redis-3.0.7/redisstart.sh

fi
