#!/bin/sh

touch nodeblock_singlenode_buildpart4.do

# copy redis configuration file
cp build/redis_prod.conf /apps/redis-3.0.7
cp build/redisshutdown.py /apps/redis-3.0.7
cp build/redisstart.sh /apps/redis-3.0.7

# copy index.html marker 
cp build/node/index.html /apps/mongrel2-v1.11.0/pickup

# download mongrel2.conf; load config to create database for m2sh
cp build/mongrel2_prod.conf /apps/mongrel2-v1.11.0/

# download script to generate certs for mongrel2
cp build/mongrel2certgen.sh /apps/mongrel2-v1.11.0/certs
cp build/mongrel2certgen_part*.sh /apps/mongrel2-v1.11.0/certs

# download common processing scripts
cp build/generalutils.py /apps/mongrel2-v1.11.0
cp build/redisutils.py /apps/mongrel2-v1.11.0
cp build/stopper.py /apps/mongrel2-v1.11.0
cp build/watchlogs.py /apps/mongrel2-v1.11.0
cp build/startstack_prod.sh /apps/mongrel2-v1.11.0
cp build/stopstack_prod.sh /apps/mongrel2-v1.11.0 

# download node processing scripts
cp build/node/download_encrypt.exe /apps/mongrel2-v1.11.0
cp build/node/upload_decrypt.exe /apps/mongrel2-v1.11.0
cp build/node/OnUpload.py /apps/mongrel2-v1.11.0
cp build/node/PickupReq.py /apps/mongrel2-v1.11.0
cp build/node/ProcessData_Q1.py /apps/mongrel2-v1.11.0
cp build/node/ProcessData_Q2.py /apps/mongrel2-v1.11.0
cp build/node/ProcessData_Q3.py /apps/mongrel2-v1.11.0
cp build/node/ProcessData_Q4.py /apps/mongrel2-v1.11.0
cp build/node/QueueForProcessingManager.py /apps/mongrel2-v1.11.0
cp build/node/systemsdochtml.py /apps/mongrel2-v1.11.0
cp build/node/systemsdocpdf.py /apps/mongrel2-v1.11.0

# download nodemgr processing scripts
cp build/nodemgr/SystemStatus_prod.py /apps/mongrel2-v1.11.0

# make mono files executable
chmod 755 /apps/mongrel2-v1.11.0/download_encrypt.exe
chmod 755 /apps/mongrel2-v1.11.0/upload_decrypt.exe
