.PHONY: build
build:
	dotnet build

.PHONY: dev
dev:
	dotnet watch run --project Hulk/Hulk
