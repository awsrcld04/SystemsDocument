from datetime import datetime, date
from mongrel2 import handler

import hashlib
try:
    import json
except:
    import simplejson as json
import logging
import os
import redis
import sys

# custom modules
import generalutils
import redisutils

args0 = sys.argv[0]

generalutils.writepidfile(args0)

logging.basicConfig(filename=args0.split('.')[0]+'.log',level=logging.WARNING, \
                    format='%(asctime)s %(message)s')

sender_id = "AB60B07D-D7DB-4325-8B65-9F674F706C99"

conn = handler.Connection(sender_id, "tcp://127.0.0.1:9001",
                          "tcp://127.0.0.1:9000")

applogger = redisutils.getnewredislogger()

applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        + '-' + args0 +': RUNNING')

#connection to datastore; attach to internal network interface
dstrconn = redis.StrictRedis(host='localhost', port=7369, db=5)

nodemgr = redisutils.getnewnodeconn()

nextnode = nodemgr.get('NextNode')

currentnode = 'sdocnm'

if nextnode is None:
    nodemgr.set('NextNode','sdocnm')

while True:
    try:
        req = conn.recv()

        if req.is_disconnect():
            #print "DISCONNECT"
            continue
        else:           

            clientreq = req.body.strip()        

            if clientreq.split(':')[0] == "<status>":
                skeycheck = dstrconn.exists(clientreq.split(':')[1])
                # check to see if skey is in database
                if skeycheck:
                    serverlimit = dstrconn.get('scount' + clientreq.split(':')[1])
                    servercount = dstrconn.scard(clientreq.split(':')[1])
                    # check to see if the server limit has been reached for the skey
                    if int(servercount) < int(serverlimit):
                        servercheck = dstrconn.sismember(clientreq.split(':')[1], clientreq.split(':')[2])
                        # check to see if the server is already in the list of servers scan for a particular skey
                        if not servercheck:
                            # server limit has not been reached and server has not been scanned                  
                            currentnode = nodemgr.get('NextNode')
                            response = "STAT100" + ':' + currentnode
                        else:
                            # server has been scanned before for this skey
                            response = "STAT403"
                    else:
                        # server limit has been reached for the skey
                        response = "STAT402"
                else:
                    # skey is not in database
                    response = "STAT401"
                # leave next few lines commented out until multiple nodes are up and running
                #if currentnode == 'sdocnd1':
                #    nodemgr.set('NextNode','sdocnd2')
                #else:
                #    nodemgr.set('NextNode','sdocnm')
            else:
                # request is not valid
                response = "STAT400"

        conn.reply_http(req, response)
    except:
        logging.error('An error has occurred.')
        logging.exception('An exception has occurred.')
        logging.critical('A critical error has occurred.')