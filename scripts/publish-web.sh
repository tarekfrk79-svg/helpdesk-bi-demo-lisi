#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT_DIR/scripts/_dotnet_env.sh"

PUBLISH_DIR="${1:-$ROOT_DIR/artifacts/publish/web}"

rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"

"$DOTNET" publish \
  "$ROOT_DIR/src/HelpDeskBiDemo.Web/HelpDeskBiDemo.Web.csproj" \
  -c Release \
  -m:1 \
  /p:BuildInParallel=false \
  /p:NuGetAudit=false \
  -o "$PUBLISH_DIR"

echo "Publish completed: $PUBLISH_DIR"
