#!/bin/sh

touch nodeblock_singlenode_buildpart8.do

# load mongrel2 configuration
cd /apps/mongrel2-v1.11.0
m2sh load -config mongrel2_prod.conf

# on intial setup don't start stack yet - need generate certs for mongrel2
