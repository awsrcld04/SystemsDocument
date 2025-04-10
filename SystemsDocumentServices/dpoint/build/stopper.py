import os
import signal
import sys

f = open(sys.argv[1], 'r')
processpid = f.read()
f.close()

processpid = processpid.strip()

os.kill(int(processpid), signal.SIGKILL)


