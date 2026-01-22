#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  scripts/run-release-local.sh zexcel <version>
  scripts/run-release-local.sh zpdf <version>

Examples:
  scripts/run-release-local.sh zexcel 0.2.1-alpha1
  scripts/run-release-local.sh zpdf 0.1.10-alpha1
EOF
}

if [[ $# -ne 2 ]]; then
  usage
  exit 1
fi

kind="$1"
version="$2"
base_version="${version%%-*}"

case "$kind" in
  zexcel)
    project="src/zExcelGenerator/zExcelGenerator.csproj"
    ;;
  zpdf)
    project="src/zPdfGenerator/zPdfGenerator.csproj"
    ;;
  *)
    echo "Unknown kind: $kind"
    usage
    exit 1
    ;;
esac

echo "Building $project with Version=$version, AssemblyVersion=$base_version, FileVersion=$base_version"
dotnet build "$project" -c Release --no-restore \
  -p:Version="$version" \
  -p:AssemblyVersion="$base_version" \
  -p:FileVersion="$base_version"

echo "Packing $project with PackageVersion=$version"
dotnet pack "$project" -c Release --no-build -o ./artifacts \
  -p:PackageVersion="$version" \
  -p:Version="$version" \
  -p:AssemblyVersion="$base_version" \
  -p:FileVersion="$base_version"

echo "Done. Packages in ./artifacts"
