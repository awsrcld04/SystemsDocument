from datetime import datetime
import os
import sys
import json

# return a count of items
def count(items):
    cnt = 0
    for x in items:
        cnt = cnt + 1
    return cnt

# execute c# program to perform decryption
# full path needs to be passed in
def uploaddecrypt(inputfile_f, newfile_f):
    os.system('mono /apps/mongrel2-v1.11.0/upload_decrypt.exe ' + inputfile_f \
              + ' ' + newfile_f)

# execute c# program to perform encryption
# full path needs to be passed in
def downloadencrypt(inputfile_f, newfile_f):
    os.system('mono /apps/mongrel2-v1.11.0/download_encrypt.exe ' + inputfile_f \
              + ' ' + newfile_f)    

# load json from string; create string by reading in file contents; return json
def loadjsonfromstring(inputfile_f):
    f = open(inputfile_f,'r')
    filecontents = f.read()
    f.close()

    newjsonstring = json.loads(filecontents)

    return newjsonstring

def writepidfile(filename_f):   
    f = open(filename_f + '.pid','w')
    tmppid = str(os.getpid())
    f.write(tmppid)
    f.close()

def formatnumber(numstring):
    if len(numstring) == 1:
        fmtd_num = numstring + 'B (' + numstring + ')'
        return fmtd_num

    if len(numstring) == 2:
        fmtd_num = numstring + 'B (' + numstring + ')'
        return fmtd_num

    if len(numstring) == 3:
        fmtd_num = numstring + 'B (' + numstring + ')'
        return fmtd_num

    if len(numstring) == 4:
        newnumstring = numstring[0:1] + ',' + numstring[1:4]
        fmtd_num = numstring[0:1] + "." + numstring[1:2] + 'KB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 5:
        newnumstring = numstring[0:2] + ',' + numstring[2:5]
        fmtd_num = numstring[0:2] + "." + numstring[2:3] + 'KB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 6:
        newnumstring = numstring[0:3] + ',' + numstring[3:6]
        fmtd_num = numstring[0:3] + "." + numstring[3:4] + 'KB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 7:
        newnumstring = numstring[0:1] + ',' + numstring[1:4] + ',' + numstring[5:8]
        fmtd_num = numstring[0:1] + "." + numstring[1:2] + 'MB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 8:
        newnumstring = numstring[0:2] + ',' + numstring[2:5] + ',' + numstring[5:8]
        fmtd_num = numstring[0:2] + "." + numstring[2:3] + 'MB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 9:
        newnumstring = numstring[0:3] + ',' + numstring[3:6] + ',' + numstring[6:9]
        fmtd_num = numstring[0:3] + "." + numstring[3:4] + 'MB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 10:
        newnumstring = numstring[0:1] + ',' + numstring[1:4] + ',' + numstring[4:7] + \
            ',' + numstring[7:10]
        fmtd_num = numstring[0:1] + "." + numstring[1:2] + 'GB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 11:
        newnumstring = numstring[0:2] + ',' + numstring[2:5] + ',' + numstring[5:8] + \
            ',' + numstring[8:11]
        fmtd_num = numstring[0:2] + "." + numstring[2:3] + 'GB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 12:
        newnumstring = numstring[0:3] + ',' + numstring[3:6] + ',' + numstring[6:9] + \
            ',' + numstring[9:12]
        fmtd_num = numstring[0:3] + "." + numstring[3:4] + 'GB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 13:
        newnumstring = numstring[0:1] + ',' + numstring[1:4] + ',' + numstring[4:7] + \
            ',' + numstring[7:10] + ',' + numstring[10:13]
        fmtd_num = numstring[0:1] + "." + numstring[1:2] + 'TB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 14:
        newnumstring = numstring[0:2] + ',' + numstring[2:5] + ',' + numstring[5:8] + \
            ',' + numstring[8:11] + ',' + numstring[11:14]
        fmtd_num = numstring[0:2] + "." + numstring[2:3] + 'TB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 15:
        newnumstring = numstring[0:3] + ',' + numstring[3:6] + ',' + numstring[6:9] + \
            ',' + numstring[9:12] + ',' + numstring[12:15]
        fmtd_num = numstring[0:3] + "." + numstring[3:4] + 'TB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 16:
        newnumstring = numstring[0:1] + ',' + numstring[1:4] + ',' + numstring[4:7] + \
            ',' + numstring[7:10] + ',' + numstring[10:13] + ',' + numstring[13:16]
        fmtd_num = numstring[0:1] + "." + numstring[1:2] + 'PB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 17:
        newnumstring = numstring[0:2] + ',' + numstring[2:5] + ',' + numstring[5:8] + \
            ',' + numstring[8:11] + ',' + numstring[11:14] + ',' + numstring[14:17]
        fmtd_num = numstring[0:2] + "." + numstring[2:3] + 'PB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 18:
        newnumstring = numstring[0:3] + ',' + numstring[3:6] + ',' + numstring[6:9] + \
            ',' + numstring[9:12] + ',' + numstring[12:15] + ',' + numstring[15:18]
        fmtd_num = numstring[0:3] + "." + numstring[3:4] + 'PB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 19:
        newnumstring = numstring[0:1] + ',' + numstring[1:4] + ',' + numstring[4:7] + \
            ',' + numstring[7:10] + ',' + numstring[10:13] + ',' + numstring[13:16] + \
            ',' + numstring[16:19]
        fmtd_num = numstring[0:1] + "." + numstring[1:2] + 'EB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 20:
        newnumstring = numstring[0:2] + ',' + numstring[2:5] + ',' + numstring[5:8] + \
            ',' + numstring[8:11] + ',' + numstring[11:14] + ',' + numstring[14:17] + \
            ',' + numstring[17:20]
        fmtd_num = numstring[0:2] + "." + numstring[2:3] + 'EB (' + newnumstring + ')'
        return fmtd_num

    if len(numstring) == 21:
        newnumstring = numstring[0:3] + ',' + numstring[3:6] + ',' + numstring[6:9] + \
            ',' + numstring[9:12] + ',' + numstring[12:15] + ',' + numstring[15:18] + \
            ',' + numstring[18:21]
        fmtd_num = numstring[0:3] + "." + numstring[3:4] + 'EB (' + newnumstring + ')'
        return fmtd_num
