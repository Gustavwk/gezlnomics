param(
  [ValidateSet("help", "config", "up", "down", "ps", "logs", "rebuild", "types", "sync")]
  [string]$Command = "help",
  [int]$TimeoutSeconds = 120,
  [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

function Get-EnvValue {
  param(
    [string]$Key,
    [string]$DefaultValue
  )

  $envFile = Join-Path $PSScriptRoot "..\.env"
  if (-not (Test-Path $envFile)) {
    return $DefaultValue
  }

  $line = Get-Content $envFile | Where-Object { $_ -match "^$Key=" } | Select-Object -First 1
  if (-not $line) {
    return $DefaultValue
  }

  return $line.Substring($Key.Length + 1).Trim()
}

function Wait-ForSwagger {
  param([int]$Timeout)

  $apiPort = Get-EnvValue -Key "API_PORT" -DefaultValue "8080"
  $swaggerUrl = "http://localhost:$apiPort/swagger/v1/swagger.json"
  $deadline = (Get-Date).AddSeconds($Timeout)

  Write-Host "Venter paa Swagger: $swaggerUrl"
  do {
    try {
      $response = Invoke-WebRequest -Uri $swaggerUrl -UseBasicParsing -TimeoutSec 5
      if ($response.StatusCode -eq 200) {
        Write-Host "Swagger er klar."
        return
      }
    }
    catch {
      Start-Sleep -Seconds 2
    }
  } while ((Get-Date) -lt $deadline)

  throw "Timeout: Swagger blev ikke klar inden for $Timeout sekunder."
}

function Run-Compose {
  param([string[]]$ComposeArgs)
  & docker compose @ComposeArgs
}

function Run-TypesGenerate {
  Push-Location (Join-Path $PSScriptRoot "..\frontend")
  try {
    & npm run types:generate
  }
  finally {
    Pop-Location
  }
}

switch ($Command) {
  "help" {
    Write-Host "Brug: .\scripts\dev.ps1 <command>"
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "  config   Valider docker compose config"
    Write-Host "  up       Start stack (default: --build -d)"
    Write-Host "  down     Stop stack"
    Write-Host "  ps       Vis status"
    Write-Host "  logs     Foelg logs"
    Write-Host "  rebuild  Build uden cache"
    Write-Host "  types    Generer frontend-typer fra Swagger"
    Write-Host "  sync     config + up + vent paa Swagger + types"
    Write-Host ""
    Write-Host "Flags:"
    Write-Host "  -NoBuild           Bruges med 'up' for at skippe build"
    Write-Host "  -TimeoutSeconds N  Timeout for Swagger-wait (types/sync)"
    break
  }
  "config" {
    Run-Compose -ComposeArgs @("config")
    break
  }
  "up" {
    if ($NoBuild) {
      Run-Compose -ComposeArgs @("up", "-d")
    }
    else {
      Run-Compose -ComposeArgs @("up", "--build", "-d")
    }
    break
  }
  "down" {
    Run-Compose -ComposeArgs @("down")
    break
  }
  "ps" {
    Run-Compose -ComposeArgs @("ps")
    break
  }
  "logs" {
    Run-Compose -ComposeArgs @("logs", "-f", "--tail=200")
    break
  }
  "rebuild" {
    Run-Compose -ComposeArgs @("build", "--no-cache")
    break
  }
  "types" {
    Wait-ForSwagger -Timeout $TimeoutSeconds
    Run-TypesGenerate
    break
  }
  "sync" {
    Run-Compose -ComposeArgs @("config")
    if ($NoBuild) {
      Run-Compose -ComposeArgs @("up", "-d")
    }
    else {
      Run-Compose -ComposeArgs @("up", "--build", "-d")
    }
    Wait-ForSwagger -Timeout $TimeoutSeconds
    Run-TypesGenerate
    Run-Compose -ComposeArgs @("ps")
    break
  }
}
