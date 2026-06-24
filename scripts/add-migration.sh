#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT_DIR/scripts/_dotnet_env.sh"

MIGRATION_NAME="${1:-}"

if [[ -z "$MIGRATION_NAME" ]]; then
  echo "Usage: bash scripts/add-migration.sh <MigrationName>" >&2
  exit 1
fi

export ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection:-Server=tcp:localhost,1433;Database=HelpDeskBiDemo;User ID=sa;Password=StrongPassword123!;TrustServerCertificate=True;Encrypt=False}"

"$DOTNET" build \
  "$ROOT_DIR/src/HelpDeskBiDemo.Web/HelpDeskBiDemo.Web.csproj" \
  -m:1 \
  /p:BuildInParallel=false \
  /p:NuGetAudit=false \
  -v minimal

"$DOTNET" tool restore >/dev/null

"$DOTNET" tool run dotnet-ef migrations add "$MIGRATION_NAME" \
  --project "$ROOT_DIR/src/HelpDeskBiDemo.Infrastructure" \
  --startup-project "$ROOT_DIR/src/HelpDeskBiDemo.Web" \
  --output-dir Data/Migrations \
  --no-build
