#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export AZURE_CONFIG_DIR="$ROOT_DIR/.azure-config"

exec "$ROOT_DIR/.azure-cli-venv/bin/python" -m azure.cli "$@"
