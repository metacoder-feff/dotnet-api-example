#!/bin/bash
set -ex

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

for PROJECT in $(find ${ROOT_DIR} -iname '*.csproj' -not -path "${ROOT_DIR}/tmp/*")
do
  # echo Update  $PROJECT
  # dotnet package update --project $PROJECT || echo er
  for PACKAGE in $(dotnet list $PROJECT package --outdated | grep '>' | sed -r 's/^ *> ([a-zA-Z0-9\.]*) .*$/\1/gm')
  do
    dotnet add $PROJECT package $PACKAGE
  done
done
echo Update Complete