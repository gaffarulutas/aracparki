#!/usr/bin/env bash
# Free app ports, watch Tailwind CSS, then run AracParki.Web with hot reload (HTTPS).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WEB="$ROOT/src/AracParki.Web"
PORTS=(7133 5245)
CSS_PID=""

cleanup() {
  if [[ -n "${CSS_PID}" ]] && kill -0 "${CSS_PID}" 2>/dev/null; then
    echo "Stopping Tailwind watch (PID ${CSS_PID})"
    kill "${CSS_PID}" 2>/dev/null || true
    wait "${CSS_PID}" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

free_port() {
  local port="$1"
  local pids
  pids="$(lsof -nP -iTCP:"$port" -sTCP:LISTEN -t 2>/dev/null || true)"
  if [[ -z "$pids" ]]; then
    echo "Port $port: free"
    return 0
  fi

  echo "Port $port: stopping PID(s) $pids"
  # shellcheck disable=SC2086
  kill $pids 2>/dev/null || true
  sleep 0.4

  pids="$(lsof -nP -iTCP:"$port" -sTCP:LISTEN -t 2>/dev/null || true)"
  if [[ -n "$pids" ]]; then
    echo "Port $port: force-killing PID(s) $pids"
    # shellcheck disable=SC2086
    kill -9 $pids 2>/dev/null || true
  fi
}

for port in "${PORTS[@]}"; do
  free_port "$port"
done

cd "$WEB"

if [[ ! -d node_modules ]]; then
  echo "Installing npm dependencies…"
  npm install
fi

echo "Starting: Tailwind CSS watch → wwwroot/css/tw-ui.css"
npm run watch:css &
CSS_PID=$!

echo "Starting: dotnet watch (https → https://localhost:7133)"
dotnet watch --launch-profile https
