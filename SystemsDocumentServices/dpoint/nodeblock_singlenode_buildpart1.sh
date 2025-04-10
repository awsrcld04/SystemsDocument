#!/bin/sh

touch nodeblock_singlenode_buildpart1.do

# this is the build script for a nodeblock with a single node

sudo apt-get -qq -y install libsqlite3-0
sudo apt-get -qq -y install libsqlite3-dev
sudo apt-get -qq -y install sqlite3
sudo apt-get -qq -y install build-essential
sudo apt-get -qq -y install uuid-dev
sudo apt-get -qq -y install uuid-runtime
sudo apt-get -qq -y install curl
sudo apt-get -qq -y install python-setuptools
sudo apt-get -qq -y install python-dev
sudo apt-get -qq -y install python-pip
sudo apt-get -qq -y install pkg-config
sudo apt-get -qq -y install libsodium-dev
sudo apt-get -qq -y install libsodium13
sudo apt-get -qq -y install tcl
sudo apt-get -qq -y install mono-runtime
sudo apt-get -qq -y install libmono-2.0-1
sudo apt-get -qq -y install libmono-corlib2.0-cil
sudo apt-get -qq -y install libmono-peapi2.0-cil
sudo apt-get -qq -y install libmono-posix2.0-cil
sudo apt-get -qq -y install libmono-system-runtime2.0-cil
sudo apt-get -qq -y install mono-2.0-gac
