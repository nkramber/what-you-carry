# Commands of this checkout. Run `make` for the list.
# The Godot binary of the machine. The environment variable WYC_GODOT overrides it, as the test suite reads it too.
GODOT ?= $(if $(WYC_GODOT),$(WYC_GODOT),/Applications/Godot_mono.app/Contents/MacOS/Godot)
SOLUTION := WhatYouCarry.slnx
GAME := WhatYouCarry.Game
TOOLS := dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj --
# The Codex CLI of the cross-provider review. The codex-review target installs the newest release first (D-512).
CODEX ?= $(shell npm prefix -g)/bin/codex

.DEFAULT_GOAL := help
.PHONY: help play windowed build build-game test test-fast smoke bot sounds analyze lint codex-review

help: ## Print this list
	@grep -hE '^[a-z-]+:.*##' $(MAKEFILE_LIST) | sed -E 's/:.*## /\t/' | expand -t 14

play: build-game ## Play the game, borderless fullscreen (D-310)
	$(GODOT) --path $(GAME)

windowed: build-game ## Play the game in a window
	$(GODOT) --path $(GAME) --windowed

build: ## Build the solution
	dotnet build $(SOLUTION)

build-game: build ## Build the solution and the Godot assemblies of the game
	$(GODOT) --headless --editor --path $(GAME) --build-solutions --quit

test: build ## Run every test, the smoke session included
	dotnet test $(SOLUTION) --no-build

test-fast: build ## Run every test but the smoke session
	dotnet test $(SOLUTION) --no-build --filter "Category!=Smoke"

smoke: build-game ## Run the headless smoke session (D-114)
	$(GODOT) --headless --path $(GAME) --fixed-fps 60 -- --smoke

bot: build-game ## Run the bot session with a frame log (M-3)
	$(GODOT) --path $(GAME) -- --bot --frame-log frames.txt

sounds: ## Render every sound from its parameter file (D-453)
	$(TOOLS) audio-synth --root .

analyze: ## Analyse one reference into its sound file: make analyze SOUND=footstep [PART=1] [FRAME=1024]
	$(TOOLS) audio-analyze --root . --sound $(SOUND) $(if $(PART),--part $(PART),) $(if $(FRAME),--frame $(FRAME),)

lint: ## Run the STE check, the determinism lint, and the asset check
	$(TOOLS) ste-check --root .
	$(TOOLS) det-lint --root .
	$(TOOLS) asset-qa --root .

codex-review: ## Run the cross-provider review of one PR through the Codex CLI: make codex-review PR=93 (D-511)
	@test -n "$(PR)" || { echo "Name the PR: make codex-review PR=<number>" >&2; exit 2; }
	npm install -g @openai/codex@latest
	$(TOOLS) codex-review --root . --pr $(PR) --codex $(CODEX)
