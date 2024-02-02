#!/bin/bash
set -ex

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

docker compose -f "${SCRIPT_DIR}/docker-compose.yaml" up

# navigate to http://localhost:8080/swagger/index.html