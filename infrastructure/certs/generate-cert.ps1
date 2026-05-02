#!/usr/bin/env pwsh
# Generates a self-signed certificate for local benchmark TLS (gRPC-Web over HTTP/2).
# SANs: localhost, product-service, order-service (Docker service names).

$cert = New-SelfSignedCertificate `
    -DnsName "localhost","product-service","order-service" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter (Get-Date).AddYears(2) `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -FriendlyName "eShop Benchmark TLS"

$pfxPassword = ConvertTo-SecureString -String "benchmark" -Force -AsPlainText
$cert | Export-PfxCertificate -FilePath "$PSScriptRoot\server.pfx" -Password $pfxPassword | Out-Null

Write-Host "Certificate generated: $PSScriptRoot\server.pfx"
Write-Host "SANs: $($cert.DnsNameList -join ', ')"

Remove-Item "Cert:\CurrentUser\My\$($cert.Thumbprint)" -ErrorAction SilentlyContinue
