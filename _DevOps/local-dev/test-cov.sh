#!/bin/bash
set -ex

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

dotnet_tool_install_if_needed () {
  local tool=$1

  if ! command -v "${tool}" &> /dev/null
  then
      echo "'${tool}' could not be found"
      echo "installing..."
      dotnet tool install --global "${tool}"
  fi
}

#dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.0
dotnet_tool_install_if_needed "reportgenerator"
#dotnet tool install --global dotnet-coverage --version 17.10.1
dotnet_tool_install_if_needed "dotnet-coverage"

OUT_DIR=${ROOT_DIR}/tests/TestResults
COV_DIR_TMP=${OUT_DIR}/tmp # + random guid
COV_FILE=${OUT_DIR}/cobertura.xml
HTML_DIR=${OUT_DIR}/html
JUNIT_FILE=${OUT_DIR}/junit.xml

rm -rf ${OUT_DIR}/* || echo ok

# out dir is created automatically
dotnet test "${ROOT_DIR}"           \
  --test-adapter-path:.             \
  --logger:"junit;LogFilePath=${JUNIT_FILE};MethodFormat=Class;FailureBodyFormat=Verbose" \
  --collect:"XPlat Code Coverage"   \
  --results-directory "${COV_DIR_TMP}"

# ATTENTION: 'dotnet test' creates subdir with random guid for each project
# combine all XMLs
# generate HTML
# generate Summary
reportgenerator                                       \
  -reports:"${COV_DIR_TMP}/**/coverage.cobertura.xml" \
  -targetdir:"${HTML_DIR}"                            \
  -reporttypes:"Cobertura;HtmlInline;TextSummary"

mv "${HTML_DIR}/Cobertura.xml"  "${COV_FILE}"
mv "${HTML_DIR}/Summary.txt"    "${OUT_DIR}/"

grep "Line coverage" "${OUT_DIR}/Summary.txt"
