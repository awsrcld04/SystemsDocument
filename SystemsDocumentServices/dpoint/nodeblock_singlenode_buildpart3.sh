#!/bin/sh

touch nodeblock_singlenode_buildpart3.do

cd depot

# wkhtmltox (wkhtmltopdf)
tar -xJf wkhtmltox-0.12.3_linux-generic-amd64.tar.xz

# redis
tar xzf redis-3.0.7.tar.gz
cd redis-3.0.7
make

# cd up one level
cd ..

# zeromq
tar -xzvf zeromq-4.1.4.tar.gz
cd zeromq-4.1.4/
./configure
make
sudo make install
ldconfig

# cd up one level
cd ..

# pyzmq
# use easy_install to install zmq python language binding
easy_install pyzmq

# pycrypto
tar -xzvf pycrypto-2.6.1.tar.gz
cd pycrypto-2.6.1
python setup.py install

# cd up one level
cd ..

# mongrel2
tar -xvjf mongrel2-v1.11.0.tar.bz2
cd mongrel2-v1.11.0
make all install

# install mongrel2 python module
cd examples/python
python setup.py install
