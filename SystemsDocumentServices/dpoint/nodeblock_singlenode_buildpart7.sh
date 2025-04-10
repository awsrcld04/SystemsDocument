#!/bin/sh

touch nodeblock_singlenode_buildpart7.do

# set correct timezone
echo "US/Eastern" | sudo tee /etc/timezone
sudo dpkg-reconfigure --frontend noninteractive tzdata
echo
echo
