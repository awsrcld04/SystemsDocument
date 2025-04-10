#!/bin/sh

touch nodeblock_singlenode_buildpart6.do

# cleanup
txtrst=$(tput sgr0) # Text reset
txtylw=$(tput setaf 3) # Yellow
echo
echo
echo ${txtylw} Cleaning up
echo
echo
cd depot
rm pycrypto-2.6.1.tar.gz
rm zeromq-4.1.4.tar.gz
rm mongrel2-v1.11.0.tar.bz2
rm redis-3.0.7.tar.gz
rm wkhtmltox-0.12.3_linux-generic-amd64.tar.xz
rm -f -r mongrel2-v1.11.0
rm -f -r zeromq-4.1.4
rm -f -r pycrypto-2.6.1
rm -f -r redis-3.0.7
rm -f -r wkhtmltox
cd ..
rmdir depot
cd build
rm *.py
rm *.sh
rm *.conf
rm *.crtmarker
rm -f -r node
rm -f -r nodemgr
cd ..
rmdir build
sudo apt-get -y remove build-essential
sudo apt-get -y remove python-dev
sudo apt-get -y remove uuid-dev
sudo apt-get -y remove libsqlite3-dev
sudo apt-get -y remove python-pip
sudo apt-get -y remove python-setuptools
sudo apt-get -y remove pkg-config
sudo apt-get -y remove libsodium-dev
sudo apt-get -y remove tcl
sudo apt-get -y autoremove
echo
echo 
echo Done with cleanup 
echo PRODUCTION ${txtrst}
echo 
echo
# end of cleanup section
