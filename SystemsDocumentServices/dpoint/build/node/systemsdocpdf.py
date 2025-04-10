from datetime import datetime
import os
import sys

# workflow file locations
pdqwrkspc1 = '/apps/processdata_q/wrkspc1/'

# generate pdf using wkhtmltopdf
def generatepdf(inputfile_f, newfile_f, newserver_obj):
    tmpcomputersystem = newserver_obj['ComputerSystem']
    servername = tmpcomputersystem['servername']
    todaystamp = datetime.strftime(datetime.now(),"%m%d%Y")
    fmtd_todaystamp = todaystamp[0:2] + '/' + todaystamp[2:4] + '/' + todaystamp[4:8]
    headerelement = servername + '-' + fmtd_todaystamp
    os.system(pdqwrkspc1 + 'wkhtmltopdf --quiet --header-left \"SystemsDocument\" --header-right ' + headerelement +' --header-line --footer-line --footer-center \"Confidential\" --footer-right \"Page [page] of [topage]\" ' + inputfile_f + ' ' + newfile_f + ' > /dev/null')
