import redis
import time

r = redis.StrictRedis(host='localhost', port=6379, db=10)

while True:
    msgarray = []
    print (r.scard('Messages'))
    for msg in r.smembers('Messages'):
        msgarray.append(msg)
    msgarray.sort()
    for msg in msgarray:
        print (msg)
    time.sleep(30)
