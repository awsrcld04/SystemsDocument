from datetime import datetime, date

import os
import redis
import simplejson as json
import sys
import time

# custom modules
import generalutils
import redisutils
import systemsdochtml
import systemsdocpdf

# worflow file locations
pdqwrkspc2 = '/apps/processdata_q/wrkspc2/'
pdqwrkspc3 = '/apps/processdata_q/wrkspc3/'
pdqwrkspc4 = '/apps/processdata_q/wrkspc4/'
pdqwrkspc5 = '/apps/processdata_q/wrkspc5/'
appsmongrelpickup = '/apps/mongrel2-v1.11.0/pickup/'


# function to decrypt the file
def Stage1Decrypt(s1ticket, arg_scriptname):
    # create applogger to store message in redis
    applogger_s1 = redisutils.getnewredislogger()

    applogger_s1.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        + '-' +  arg_scriptname +'-Stage1: ' + s1ticket)    

    r_s1 = redisutils.getnewredisconn()

    # r_s1.get(s1ticket + '-stage0-file'); way to get file created in stage0 (main)
    inputfilename = r_s1.get(s1ticket + '-stage0-file')    

    nameparts = inputfilename.split('/')[4].split('.')[2]

    #path for decrypted file will start in /apps/pdqworkspace3/
    jsonfilename = pdqwrkspc3 + 'parsetmp' + nameparts + '.json'

    generalutils.uploaddecrypt(inputfilename, jsonfilename)

    if os.path.isfile(jsonfilename):
        r_s1.set(s1ticket + '-stage1-file', jsonfilename) # 'ticket-stage1-file', newfilename
        os.remove(inputfilename) # cleanup inputfilename from /apps/pdqworkspace2
        s1result = True
    else:
        s1result = False

    return s1result

def Stage2ConfigParse(s2ticket, arg_scriptname):
    # create applogger to store message in redis
    applogger_s2 = redisutils.getnewredislogger()

    applogger_s2.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        + '-' + arg_scriptname + '-Stage2: ' + s2ticket)    

    r_s2 = redisutils.getnewredisconn()

    # r_s2.get(s2ticket + '-stage1-file'); way to get file created in stage1
    inputfilename = r_s2.get(s2ticket + '-stage1-file')

    # load json from file
    newserver = generalutils.loadjsonfromstring(inputfilename) 

    custID = newserver['strCustID']
    scanID = newserver['strScanID']

    serverhashline = newserver['strServerHash']
    scandatetime = newserver['strConfigScanDateTime']

    # adds server hash to server list for the custID/skey
    s = redisutils.getnewdstrintnetconn()
    s.sadd(custID,serverhashline)

    r_s2.set(s2ticket + '-serverhash', serverhashline)
    r_s2.sadd('Servers',serverhashline) # store for future lookup

    md5hash = r_s2.get(s2ticket + '-hash')

    htmlfilename = pdqwrkspc4 + 'sdoc_' + md5hash + '_' + scandatetime[0:14] + '.html'

    r_s2.sadd('CustID', custID)

    r_s2.sadd(custID, scanID)

    # generate html file from json;
    systemsdochtml.parsejsontohtml(htmlfilename, newserver)

    if os.path.isfile(htmlfilename):
        r_s2.set(s2ticket + '-stage2-file', htmlfilename) # 'ticket-stage2-file', htmlfile
        # moved cleanup of json file to stage 3 since the json needed to be reloaded temporarily
        s2result = True
    else:
        s2result = False

    return s2result

def Stage3GenPDF(s3ticket, arg_scriptname):
    # create applogger to store message in redis
    applogger_s3 = redisutils.getnewredislogger()

    applogger_s3.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        + '-' + arg_scriptname +'-Stage3: ' + s3ticket)    

    r_s3 = redisutils.getnewredisconn()

    # r_s3.get(s3ticket + '-stage2-file'); way to get file created in stage1   
    htmlfile = r_s3.get(s3ticket + '-stage2-file')
    jsonfilename = r_s3.get(s3ticket + '-stage1-file')

    # load json from file
    newserver = generalutils.loadjsonfromstring(jsonfilename) 

    namept1 = htmlfile.split('/')[4].split('.')[0]

    #path for pdf will start in /apps/pdqworkspace5/
    pdffile = pdqwrkspc5 + namept1 + '.pdf'

    #full path needs to be passed in
    systemsdocpdf.generatepdf(htmlfile, pdffile, newserver)

    if os.path.isfile(pdffile):
        r_s3.set(s3ticket + '-stage3-file', pdffile) # 'ticket-stage3-file', datfile
        os.remove(jsonfilename) # cleanup inputfilename from /apps/pdqworkspace3
        os.remove(htmlfile) # cleanup htmlfile from /apps/pdqworkspace4
        s3result = True
    else:
        s3result = False

    return s3result

def Stage4Encrypt(s4ticket, arg_scriptname):
    # create applogger to store message in redis
    applogger_s4 = redisutils.getnewredislogger()

    applogger_s4.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        + '-' + arg_scriptname +'-Stage4: ' + s4ticket)    

    r_s4 = redisutils.getnewredisconn()

    # r_s4.get(s4ticket + '-stage3-file'); way to get file created in stage3   
    pdffile = r_s4.get(s4ticket + '-stage3-file')

    filenamespl1 = pdffile.split('/')[4]
    datfile = filenamespl1.split('.')[0] + '.dat'

    #full path needs to be passed in
    generalutils.downloadencrypt(pdffile, appsmongrelpickup + datfile)   

    if os.path.isfile(appsmongrelpickup + datfile):
        r_s4.set(s4ticket + '-stage4-file', datfile) # 'ticket-stage3-file', datfile
        md5hash = r_s4.get(s4ticket + '-hash') # need to get md5hash from when file was uploaded
        r_s4.set(md5hash,datfile) # associate hash with file to be downloaded
        r_s4.sadd('ReadyForPickup', datfile.split('_')[1]) # store hash to be checked when client sends request
        os.remove(pdffile) # cleanup pdffile from /apps/pdqworkspace5
        s4result = True
    else:
        s4result = False    

    return s4result

# main function
def main(args):
    args0 = args[0]

    generalutils.writepidfile(args0)

    logger_main = redisutils.getnewredislogger()

    logger_main.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
        + '-' + args0 + ': RUNNING')

    s = redisutils.getnewredisconn()

    workqueue = 'Queue' + args0[13]

    while True:
        time.sleep(1)
        
        numfilestoprocess = s.scard(workqueue)
        if numfilestoprocess > 0:
            logger_main.sadd('Messages', datetime.strftime(datetime.now(),"%m%d%Y%H%M%S%f") \
                + '-' + args0 + '-main: filestoprocess: ' + str(numfilestoprocess) )   
            for x in s.smembers(workqueue):
                s.srem(workqueue, x) #x will be a ticket               
                currentticket = x
                filename0 = s.get(currentticket +'-upload') # get value for '(ticket)-upload'
                infile = open(filename0, 'r')
                tmpfilecontents = infile.read()
                filename1 = pdqwrkspc2 + filename0.split('/')[2]
                outfile = open(filename1, 'w')
                outfile.write(tmpfilecontents)
                outfile.close()
                infile.close()
                os.remove(filename0)
                s.set(currentticket +'-stage0-file',filename1)
                stage1complete = Stage1Decrypt(currentticket, args0) # file result in /apps/pdqworkspace3
                if stage1complete:
                    stage2complete = Stage2ConfigParse(currentticket, args0) # file result in /apps/pdqworkspace4
                    if stage2complete:
                        stage3complete = Stage3GenPDF(currentticket, args0) # file result in /apps/pdqworkspace5
                        if stage3complete:
                            stage4complete = Stage4Encrypt(currentticket, args0) # file result /var/mongrel2-1.7.5/pickup

# calling main function
main(sys.argv)

