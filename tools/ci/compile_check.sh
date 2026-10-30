#!/usr/bin/env bash
# Offline compile check of the Rapadura assemblies with Unity's own Roslyn, without touching the Editor.
# Reuses the Bee .rsp files of the last Editor compile (references/defines) but compiles the .cs files on disk now,
# so new/renamed files are picked up. Usage: bash tools/ci/compile_check.sh [OUT_DIR]   (prints only errors; exit 1 on error)
set -u
cd "$(dirname "$0")/../.."
OUT="${1:-${TMPDIR:-/tmp}/rapadura_cc}"; mkdir -p "$OUT"; command -v cygpath >/dev/null && OUT=$(cygpath -m "$OUT")
U="${UNITY_EDITOR_DATA:-/c/Program Files/Unity/Hub/Editor/6000.3.22f1/Editor/Data}"
DOTNET="$U/NetCoreRuntime/dotnet.exe"; CSC="$U/DotNetSdkRoslyn/csc.dll"
D=$(ls -d Library/Bee/artifacts/*.dag | head -1)
fail=0
build() { # name sourceDir [excludeDir...]
  local name=$1 src=$2; shift 2
  local rsp="$OUT/$name.rsp"
  grep -v '\.cs"$' "$D/$name.rsp" | grep -v '^-out:\|^-refout:\|^/additionalfile:\|^-analyzer:' \
    | sed "s#$D/\(Rapadura\.[A-Za-z.]*\)\.ref\.dll#$OUT/\1.dll#" > "$rsp"
  echo "-out:\"$OUT/$name.dll\"" >> "$rsp"
  local ex=""; for e in "$@"; do ex="$ex -path $e -prune -o"; done
  eval find "$src" $ex -name '"*.cs"' -print | sed 's#.*#"&"#' >> "$rsp"
  local log; log=$("$DOTNET" "$CSC" @"$rsp" 2>&1 | grep -E 'error CS' | sort -u)
  if [ -n "$log" ]; then echo "== $name"; echo "$log"; fail=1; else echo "ok $name"; fi
}
build Rapadura.Runtime Assets/Scripts Assets/Scripts/Editor
build Rapadura.PhotoMode.Urp Assets/Rendering/PhotoMode
build Rapadura.Editor Assets/Scripts/Editor
build Rapadura.Tests.EditMode Assets/Tests/EditMode Assets/Tests/EditMode/Mocap
build Rapadura.Tests.Mocap Assets/Tests/EditMode/Mocap
build Rapadura.Tests.EditorTools Assets/Tests/EditorTools
build Rapadura.Tests.PlayMode Assets/Tests/PlayMode
exit $fail
