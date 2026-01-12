#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

dotnet run --project "${root_dir}/src/Cloudify.Api/Cloudify.Api.csproj" --urls "https://localhost:5001" &
api_pid=$!

cleanup() {
  kill "${api_pid}" 2>/dev/null || true
}

trap cleanup EXIT

dotnet run --project "${root_dir}/src/Cloudify.Ui/Cloudify.Ui.csproj"
