#!/bin/bash
set -ex

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

docker build                        \
  -t dotnet-test:latest             \
  -f "${SCRIPT_DIR}/api-dockerfile" \
  "${ROOT_DIR}"


# docker run --rm -p 8080:8080 dotnet-test:latest
# navigate to http://localhost:8080/swagger/index.html