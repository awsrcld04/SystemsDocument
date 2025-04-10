#!/bin/sh

FILE=*.sh

if [ -f $FILE ];
then
echo "File $FILE exists"
else
echo "File $FILE does not exists"
fi

FILTER=$(find $HOME -type f \( -name "*.out" \) )

if [ -z ${FILTER} ]; then
