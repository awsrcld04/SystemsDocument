#!/bin/sh

cd dpoint

FILTER=$(find -type f \( -name "*.??~" \) )

if [ -z ${FILTER} ]; then

rm *.??~

fi

cd build

FILTER=$(find -type f \( -name "*.??~" \) )

if [ -z ${FILTER} ]; then

rm *.??~

fi

FILTER=$(find -type f \( -name "*.????~" \) )

if [ -z ${FILTER} ]; then

rm *.????~

fi

cd node

FILTER=$(find -type f \( -name "*.??~" \) )

if [ -z ${FILTER} ]; then

rm *.??~

fi

FILTER=$(find -type f \( -name "*.???~" \) )

if [ -z ${FILTER} ]; then

rm *.???~

fi

FILTER=$(find -type f \( -name "*.????~" \) )

if [ -z ${FILTER} ]; then

rm *.????~

fi

cd ..

cd nodemgr

FILTER=$(find -type f \( -name "*.??~" \) )

if [ -z ${FILTER} ]; then

rm *.??~

fi
