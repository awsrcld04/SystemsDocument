#!/bin/sh

touch nodeblock_singlenode_buildpart5.do

cp build/node/style.css /apps/processdata_q/wrkspc4
cp depot/wkhtmltox/bin/wkhtmltopdf /apps/processdata_q/wrkspc1

cp build/redis_prod.conf /apps/redis-3.0.7/
cp depot/redis-3.0.7/src/redis-server /apps/redis-3.0.7/
cp depot/redis-3.0.7/src/redis-cli /apps/redis-3.0.7/

# install redis python interface
cd /apps/redis-3.0.7
sudo pip install redis
cd /
