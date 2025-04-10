#!/bin/sh

# redis
chmod -x dpoint/build/redis_prod.conf
chmod -x dpoint/build/redisshutdown.py
chmod +x dpoint/build/redisstart.sh

# pickup marker
chmod -x dpoint/build/node/index.html

# output stylesheet
chmod -x dpoint/build/node/style.css

# mongrel2
chmod -x dpoint/build/mongrel2_prod.conf
chmod -x dpoint/build/mongrel2certgen.sh
chmod +x dpoint/build/mongrel2certgen_part*.sh

# common processing scripts
chmod -x dpoint/build/generalutils.py
chmod -x dpoint/build/redisutils.py
chmod -x dpoint/build/stopper.py
chmod -x dpoint/build/watchlogs.py
chmod +x dpoint/build/startstack_prod.sh
chmod +x dpoint/build/stopstack_prod.sh

# node processing scripts
chmod +x dpoint/build/node/download_encrypt.exe
chmod +x dpoint/build/node/upload_decrypt.exe
chmod -x dpoint/build/node/OnUpload.py
chmod -x dpoint/build/node/PickupReq.py
chmod -x dpoint/build/node/ProcessData_Q1.py
chmod -x dpoint/build/node/ProcessData_Q2.py
chmod -x dpoint/build/node/ProcessData_Q3.py
chmod -x dpoint/build/node/ProcessData_Q4.py
chmod -x dpoint/build/node/QueueForProcessingManager.py
chmod -x dpoint/build/node/systemsdochtml.py

# download nodemgr processing scripts
chmod -x dpoint/build/nodemgr/SystemStatus_prod.py
