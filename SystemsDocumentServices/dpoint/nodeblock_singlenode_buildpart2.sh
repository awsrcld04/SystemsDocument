#!/bin/sh

touch nodeblock_singlenode_buildpart2.do

# create /apps directory under /
# create /metadata directory under /

mkdir /metadata

mkdir /apps
mkdir /apps/redis-3.0.7
mkdir /apps/mongrel2-v1.11.0
mkdir /apps/processdata_q

# setup directories for mongrel2
mkdir /apps/mongrel2-v1.11.0/run
mkdir /apps/mongrel2-v1.11.0/logs
mkdir /apps/mongrel2-v1.11.0/tmp
mkdir /apps/mongrel2-v1.11.0/certs
mkdir /apps/mongrel2-v1.11.0/pickup

# setup directories for processing workflow
mkdir /apps/processdata_q/wrkspc1
mkdir /apps/processdata_q/wrkspc2
mkdir /apps/processdata_q/wrkspc3
mkdir /apps/processdata_q/wrkspc4
mkdir /apps/processdata_q/wrkspc5
