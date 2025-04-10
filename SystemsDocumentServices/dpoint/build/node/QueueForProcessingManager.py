from datetime import datetime, date

import os
import redis
import sys
import time

# custom modules
import generalutils
import redisutils

args0 = sys.argv[0]

generalutils.writepidfile(args0)

applogger = redisutils.getnewredislogger()

applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
    + '-' + args0 + ': RUNNING')

s = redisutils.getnewredisconn()

# on startup for this script, initialize the queue, QueueNumber, to 1
s.set('QueueNumber','1')

while True:
    time.sleep(.5)
    
    numfilestoprocess = s.scard('QueueForProcessing')
    if numfilestoprocess > 0:
        applogger.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
            + '-' + args0 + ': filestoprocess: ' + str(numfilestoprocess) )   
        ticket = s.spop('QueueForProcessing')

        queuenum = s.get('QueueNumber')

        nextqueue = 'Queue' + queuenum

        s.sadd(nextqueue, ticket)

        if queuenum == '4':
            s.set('QueueNumber','1')
        else:
            s.incr('QueueNumber')
