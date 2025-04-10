import generalutils
import redis
import sys
import time

def main(args):

    args0 = sys.argv[0]

    generalutils.writepidfile(args0)    
    
    s = redis.StrictRedis(host='localhost', port=6379, db=5)

    while True:
        time.sleep(.5)
        
        numtransactionstoprocess = s.scard('Transactions')
        if numtransactionstoprocess > 0:
            for trecord in s.smembers('Transactions'):
                transactionrecord = bytes.decode(trecord)
                # store server limit for the skey
                s.set('scount' + transactionrecord.split(':')[1], transactionrecord.split(':')[2])
                # initialize skey to store servers
                s.sadd(transactionrecord.split(':')[1],'113355')
                # store a copy of the transaction under ProcessedTransactions
                s.sadd('ProcessedTransactions',transactionrecord)
                # store the skey under the email address
                s.sadd(transactionrecord.split(':')[0],transactionrecord.split(':')[1])
                s.srem('Transactions',transactionrecord)

# calling main function
main(sys.argv)

