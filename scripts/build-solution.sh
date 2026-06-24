#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT_DIR/scripts/_dotnet_env.sh"

"$DOTNET" build "$ROOT_DIR/HelpDeskBiDemo.sln" -m:1 /p:BuildInParallel=false /p:NuGetAudit=false "$@"
