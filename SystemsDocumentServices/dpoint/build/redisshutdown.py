import redis

r = redis.StrictRedis(host='localhost', port=6379, db=10)
r.shutdown()
