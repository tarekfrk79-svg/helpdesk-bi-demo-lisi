#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

export HOME="$ROOT_DIR/.dotnet-cli"
export DOTNET_CLI_HOME="$ROOT_DIR/.dotnet-cli"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_MULTILEVEL_LOOKUP=0
export DOTNET_ROOT="$ROOT_DIR/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"

DOTNET="$DOTNET_ROOT/dotnet"

if [[ ! -x "$DOTNET" ]]; then
  echo "Local .NET SDK not found at $DOTNET" >&2
  exit 1
fi
