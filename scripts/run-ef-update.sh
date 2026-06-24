#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT_DIR/scripts/_dotnet_env.sh"

if [[ -z "${ConnectionStrings__DefaultConnection:-}" ]]; then
  echo "Set ConnectionStrings__DefaultConnection before running this script." >&2
  exit 1
fi

"$DOTNET" build \
  "$ROOT_DIR/src/HelpDeskBiDemo.Web/HelpDeskBiDemo.Web.csproj" \
  -m:1 \
  /p:BuildInParallel=false \
  /p:NuGetAudit=false \
  -v minimal

"$DOTNET" tool restore >/dev/null

"$DOTNET" tool run dotnet-ef database update \
  --project "$ROOT_DIR/src/HelpDeskBiDemo.Infrastructure" \
  --startup-project "$ROOT_DIR/src/HelpDeskBiDemo.Web" \
  --no-build \
  "$@"
