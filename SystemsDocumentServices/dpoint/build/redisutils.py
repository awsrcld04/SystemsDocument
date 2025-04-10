import redis

# redisutils.getnewredislogger()
def getnewredislogger():
    newlogger = redis.StrictRedis(host='localhost', port=6379, db=10) #connect to db10 for logging

    return newlogger

# redisutils.getnewredisconn()
def getnewredisconn():
    newconn = redis.StrictRedis(host='localhost', port=6379, db=2) #connect to db2 for processing

    return newconn

# redisutils.getnewnodeconn()
def getnewnodeconn():
    newmsgr = redis.StrictRedis(host='localhost', port=6379, db=3) #connect to db3 for nodes

    return newmsgr

# redisutils.getnewdstrintnetconn()
def getnewdstrintnetconn():
    # redirect to another system via spiped running locally
    newdstr = redis.StrictRedis(host='localhost', port=7369, db=5) #connect to db5 for licensing and transaction status

    return newdstr
