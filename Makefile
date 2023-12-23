PACKAGE_NAME = "interval.fs"
RELEASE ?= $$(git describe --tags)
NUGET_SOURCE = "https://api.nuget.org/v3/index.json"

# (-) Avoids error-ing when the `.env` is not found
-include .env
export

all : test pack push delete
.PHONY : all

test:
	@echo "Running testing suite..."
	dotnet run --project "Interval.Tests" --no-build --verbosity normal

pack:
	@echo "Packing release: $(RELEASE)"
	rm -f */bin/Release/*.nupkg
	dotnet pack -c Release /p:Version=$$(echo $(RELEASE) | sed 's/v//g')

push:
	@echo "Pushing release '$(RELEASE)' to "
	dotnet nuget push */bin/Release/*.nupkg -k "${NUGET_API_KEY}" -s ${NUGET_SOURCE} --skip-duplicate

delete:
	dotnet nuget delete ${PACKAGE_NAME} $$(echo $(RELEASE) | sed 's/v//g') \
		-k "${NUGET_DELETE_KEY}" \
		-s ${NUGET_SOURCE} \
		--non-interactive