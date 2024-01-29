#!/bin/bash
set -ex

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/..")

dotnet_tool_install_if_needed () {
  local tool=$1

  if ! command -v "${tool}" &> /dev/null
  then
      echo "'${tool}' could not be found"
      echo "installing..."
      dotnet tool install --global "${tool}"
  fi
}

dotnet_tool_install_if_needed "reportgenerator"
dotnet_tool_install_if_needed "dotnet-coverage"

COV_DIR=${ROOT_DIR}/tests/TestResults
COV_FILE=${COV_DIR}/cobertura.xml
HTML_DIR=${COV_DIR}/html

# dotnet test                         \
#   --collect:"XPlat Code Coverage"   \
#   --results-directory "${COV_DIR}"
#
# ATTENTION: 'dotnet test' creates subdir with guid
# WARKAROUND: use 'dotnet-coverage'

# out dir is created automatically
dotnet-coverage collect -f cobertura -o "${COV_FILE}" "dotnet test \"${ROOT_DIR}\""

# out dir is created automatically
reportgenerator             \
  -reports:"${COV_FILE}"    \
  -targetdir:"${HTML_DIR}"  \
  -reporttypes:Html
