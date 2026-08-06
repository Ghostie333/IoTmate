# Generates a self-signed TLS certificate for local MQTT development.
#
# The certificate is written to the exact paths referenced by
# mosquitto.conf so the broker works locally with TLS enabled.
# On production these paths are replaced by the real Let's Encrypt
# certificate without touching the broker configuration.
#
# Usage:  pwsh ./scripts/generate-dev-certs.ps1
param(
    [int]$Days = 365,
    [string]$CertDir = "mosquitto/letsencrypt/live/mqtt.twojadomena.pl",
    [string]$Domain = "mqtt.twojadomena.pl"
)

$ErrorActionPreference = "Stop"

$openssl = "C:\Program Files\Git\usr\bin\openssl.exe"
if (-not (Test-Path -LiteralPath $openssl)) {
    throw "OpenSSL not found at $openssl. Install Git for Windows or specify the openssl path."
}

$outDir = Join-Path (Split-Path $PSScriptRoot -Parent) $CertDir
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

$keyPath = Join-Path $outDir "privkey.pem"
$certPath = Join-Path $outDir "fullchain.pem"

& $openssl req -x509 `
    -newkey rsa:2048 `
    -nodes `
    -sha256 `
    -days $Days `
    -subj "/CN=$Domain" `
    -addext "subjectAltName=DNS:mqtt,DNS:$Domain,DNS:localhost,IP:127.0.0.1" `
    -keyout $keyPath `
    -out $certPath

if ($LASTEXITCODE -ne 0) {
    throw "OpenSSL failed with exit code $LASTEXITCODE"
}

Write-Host "Generated dev certificate:"
Write-Host "  Certificate: $certPath"
Write-Host "  Private key: $keyPath"
