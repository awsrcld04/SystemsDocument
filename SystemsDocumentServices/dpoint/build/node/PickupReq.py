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

sender_id = "90484746-B7CD-4792-C6D3-A282B2827F90"

conn = handler.Connection(sender_id, "tcp://127.0.0.1:9999",
                          "tcp://127.0.0.1:9998")

r = redisutils.getnewredisconn()

applogger = redisutils.getnewredislogger()

applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        + '-' + args0 +': RUNNING')

while True:
    try:    
        req = conn.recv()

        # for troubleshooting
        #currentheaders = json.dumps(req.headers)

        if req.is_disconnect():
            #print "DISCONNECT"
            continue
        else:    
            md5hash = req.body      

            readyflag = r.sismember('ReadyForPickup',md5hash.strip())
            filetodownload = r.get(md5hash)

            if readyflag == True:        
                response = '<ready>' + ':' + filetodownload
            else:
                response = '<retry>'

        conn.reply_http(req, response)
    except:
        logging.error('An error has occurred.')
        logging.exception('An exception has occurred.')
        logging.critical('A critical error has occurred.')    