#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PUBLISH_DIR="$ROOT_DIR/artifacts/publish/web"
ZIP_PATH="${1:-$ROOT_DIR/artifacts/HelpDeskBiDemo.Web.zip}"

if ! command -v zip >/dev/null 2>&1; then
  echo "The 'zip' command is required to create the Azure package." >&2
  exit 1
fi

"$ROOT_DIR/scripts/publish-web.sh" "$PUBLISH_DIR"

rm -f "$ZIP_PATH"
mkdir -p "$(dirname "$ZIP_PATH")"

(
  cd "$PUBLISH_DIR"
  zip -rq "$ZIP_PATH" .
)

echo "Azure ZIP package created: $ZIP_PATH"
