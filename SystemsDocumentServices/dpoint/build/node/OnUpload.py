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
import uuid

# custom modules
import generalutils
import redisutils

args0 = sys.argv[0]

generalutils.writepidfile(args0)

logging.basicConfig(filename=args0.split('.')[0]+'.log',level=logging.WARNING, \
                    format='%(asctime)s %(message)s')

sender_id = "82209006-86FF-4982-B5EA-D1E29E55D481"

conn = handler.Connection(sender_id, "tcp://127.0.0.1:9997",
                          "tcp://127.0.0.1:9996")

r = redisutils.getnewredisconn()

applogger = redisutils.getnewredislogger()

while True:
    try:

        applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
            + '-' + args0 +': WAITING FOR REQUEST')
        #print datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        #    + '-' + args0 +': WAITING FOR REQUEST'

        req = conn.recv()

        if req.is_disconnect():
            #print "DISCONNECT"
            continue

        elif req.headers.get('x-mongrel2-upload-done', None):

            applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + args0 +': request received')
            #print datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
            #    + '-' + args0 +': request received'

            # for troubleshooting
            #currentheaders = json.dumps(req.headers)

            applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + 'x-forwarded-for:' + req.headers['x-forwarded-for'] + '-' + 'user-agent:' + req.headers['user-agent'])

            # for troubleshooting
            #print currentheaders

            expected = req.headers.get('x-mongrel2-upload-start', "BAD")
            upload = req.headers.get('x-mongrel2-upload-done', None)

            if expected != upload:
                #print "GOT THE WRONG TARGET FILE: ", expected, upload
                continue

            #print upload

            upload = "." + upload

            #print upload

            body = open(upload, 'r').read()

            #print "UPLOAD DONE: BODY IS %d long, content length is %s" % (
            #    len(body), req.headers['content-length'])

            response = "UPLOAD GOOD: %s" % hashlib.md5(body).hexdigest()

            applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + args0 + ':' + response)

            ticket = str(uuid.uuid4()).replace('-','')
            r.set(ticket + '-upload', upload)
            r.sadd('QueueForProcessing', ticket)
            r.set(ticket + '-hash',hashlib.md5(body).hexdigest())

        elif req.headers.get('x-mongrel2-upload-start', None):
            applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + args0 +': request received')
            #print datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
            #    + '-' + args0 +': request received'

            # for troubleshooting
            #currentheaders = json.dumps(req.headers)

            applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + 'x-forwarded-for:' + req.headers['x-forwarded-for'] + '-' + 'user-agent:' + req.headers['user-agent'])

            # for troubleshooting
            #print currentheaders

            #print "UPLOAD starting, don't reply yet."
            #print "Will read file from %s." % req.headers.get('x-mongrel2-upload-start', None)
            continue

        else:
            applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + args0 +': request received')
            #print datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
            #    + '-' + args0 +': request received'

            # for troubleshooting
            #currentheaders = json.dumps(req.headers)

            applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + 'x-forwarded-for:' + req.headers['x-forwarded-for'] + '-' + 'user-agent:' + req.headers['user-agent'])

            # for troubleshooting
            #print currentheaders

            response = "<pre>\nSENDER: %r\nIDENT:%r\nPATH: %r\nHEADERS:%r\nBODY:%r</pre>" % (
                req.sender, req.conn_id, req.path, 
                json.dumps(req.headers), req.body)

            #added this line to test
            response = '200 Ok'

            #print response

        conn.reply_http(req, response)

    except:
        logging.error('An error has occurred.')
        logging.exception('An exception has occurred.')
        logging.critical('A critical error has occurred.')   