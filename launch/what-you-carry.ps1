# Update, build, and start What You Carry on Windows (README, "Launch the game").
#
#   what-you-carry              update the checkout to the newest main, build, and start the game
#   what-you-carry -NoUpdate    build and start the checkout as it is
#   what-you-carry -Install     get the pinned Godot, add the command what-you-carry, and start nothing
#
# The first run is: powershell -ExecutionPolicy Bypass -File launch\what-you-carry.ps1 -Install
# The command what-you-carry is a file what-you-carry.cmd in the WindowsApps directory of the user, which is on the
# command path, so each PowerShell window finds it. The update stops when a tracked file has a change, so it never
# discards work. The environment variable WYC_GODOT names the Godot .NET binary, as for the test suite.
# The script runs in Windows PowerShell 5.1 and in PowerShell 7.
param(
    [switch]$NoUpdate,
    [switch]$Install
)

$ErrorActionPreference = 'Stop'

$GodotVersion = '4.7.2-stable'
# The SHA-512 of the Windows zip of the Godot release. The smoke workflow pins the same value (D-626).
$GodotSha512 = '79229fd112b0c9cbeab82363a4ef7be18ea70f1caf86bf912789335b136fbe7e01db0053a33461438c5da1c680c17bbb10040bd09bedc51221cd4423d0367757'
$GodotDirectory = Join-Path $HOME 'godot'
$DefaultGodot = Join-Path $GodotDirectory "Godot_v$($GodotVersion)_mono_win64\Godot_v$($GodotVersion)_mono_win64_console.exe"
$Repo = Split-Path -Parent $PSScriptRoot
$GameDirectory = Join-Path $Repo 'WhatYouCarry.Game'
$CommandFile = Join-Path $env:LOCALAPPDATA 'Microsoft\WindowsApps\what-you-carry.cmd'

# Runs one program, and stops the script with the step name when the program exits with a code other than 0.
function Invoke-Step([string]$Step, [string]$Program, [string[]]$Arguments) {
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "what-you-carry: the step '$Step' failed with exit code $LASTEXITCODE."
    }
}

function Install-Launcher {
    if (Test-Path -LiteralPath $DefaultGodot) {
        Write-Host "what-you-carry: Godot is at $DefaultGodot."
    }
    else {
        $zip = Join-Path $env:TEMP "Godot_v$($GodotVersion)_mono_win64.zip"
        $url = "https://github.com/godotengine/godot/releases/download/$GodotVersion/Godot_v$($GodotVersion)_mono_win64.zip"
        Write-Host "== install: download $url"
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing
        $hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA512).Hash.ToLowerInvariant()
        if ($hash -ne $GodotSha512) {
            Remove-Item -LiteralPath $zip
            throw "what-you-carry: the SHA-512 of $zip is $hash, and the script pins $GodotSha512."
        }
        Expand-Archive -LiteralPath $zip -DestinationPath $GodotDirectory -Force
        Remove-Item -LiteralPath $zip
        Write-Host "what-you-carry: Godot is at $DefaultGodot."
    }

    $script = Join-Path $PSScriptRoot 'what-you-carry.ps1'
    $text = "@echo off`r`npowershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$script`" %*`r`n"
    Set-Content -LiteralPath $CommandFile -Value $text -NoNewline -Encoding Ascii
    Write-Host "what-you-carry: the command is $CommandFile. Open a new PowerShell window, and write what-you-carry."
}

function Update-Checkout {
    Write-Host '== update: the newest main'
    Invoke-Step 'update' 'git' @('-C', $Repo, 'fetch', 'origin', 'main')
    $changes = & git -C $Repo status --porcelain --untracked-files=no
    if ($LASTEXITCODE -ne 0) {
        throw "what-you-carry: the step 'update' failed with exit code $LASTEXITCODE."
    }
    if ($changes) {
        throw "what-you-carry: the step 'update' failed: a tracked file in $Repo has a change. Commit or remove the change, or start with -NoUpdate."
    }
    Invoke-Step 'update' 'git' @('-C', $Repo, 'checkout', 'main')
    Invoke-Step 'update' 'git' @('-C', $Repo, 'pull', '--ff-only', 'origin', 'main')
    Invoke-Step 'update' 'git' @('-C', $Repo, 'log', '-1', '--oneline')
}

if ($Install) {
    Install-Launcher
    exit 0
}

$godot = $env:WYC_GODOT
if (-not $godot) {
    $godot = $DefaultGodot
}
if (-not (Test-Path -LiteralPath $godot)) {
    throw "what-you-carry: the step 'check' failed: no Godot .NET binary at $godot. Run the script with -Install, or set WYC_GODOT."
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "what-you-carry: the step 'check' failed: no dotnet on the command path. Install the SDK version of $Repo\global.json."
}

if (-not $NoUpdate) {
    Update-Checkout
}

# The build writes the Game assembly where Godot reads it, so the game needs no Godot import step. The smoke
# workflow starts the game the same way on each platform. The Godot step --build-solutions failed on Windows (F-207).
Write-Host '== build: dotnet build'
Invoke-Step 'build' 'dotnet' @('build', (Join-Path $Repo 'WhatYouCarry.slnx'))

Write-Host '== play: Escape or Start ends the session (D-311)'
Invoke-Step 'play' $godot @('--path', $GameDirectory)
