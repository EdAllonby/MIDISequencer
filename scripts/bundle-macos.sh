#!/usr/bin/env bash
# Builds a double-clickable Sequencer.app for this Mac.
#
#   scripts/bundle-macos.sh            # Apple Silicon (osx-arm64), the default
#   scripts/bundle-macos.sh osx-x64    # Intel Macs
#
# The app is self-contained (it does not need .NET installed) and ad-hoc signed, which is enough to run
# it on the Mac that built it. Sharing it with other people needs a Developer ID signature and
# notarisation instead.
set -euo pipefail

RID="${1:-osx-arm64}"
VERSION="1.0.0"
BUNDLE_ID="com.github.edallonby.midisequencer"

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PUBLISH="$ROOT/dist/publish/$RID"
APP="$ROOT/dist/Sequencer.app"

echo "Publishing for $RID..."
rm -rf "$PUBLISH" "$APP"
dotnet publish "$ROOT/Sequencer.View/Sequencer.View.csproj" \
  --configuration Release \
  --runtime "$RID" \
  --self-contained true \
  --output "$PUBLISH" \
  -p:UseAppHost=true \
  --nologo -v quiet

echo "Assembling Sequencer.app..."
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -R "$PUBLISH/." "$APP/Contents/MacOS/"

# Icon: every size macOS asks for, from the 1024 px source.
ICONSET="$ROOT/dist/AppIcon.iconset"
rm -rf "$ICONSET" && mkdir -p "$ICONSET"
for size in 16 32 128 256 512; do
  sips -z $size $size "$ROOT/Sequencer.View/Assets/AppIcon.png" --out "$ICONSET/icon_${size}x${size}.png" >/dev/null
  sips -z $((size * 2)) $((size * 2)) "$ROOT/Sequencer.View/Assets/AppIcon.png" --out "$ICONSET/icon_${size}x${size}@2x.png" >/dev/null
done
iconutil -c icns "$ICONSET" -o "$APP/Contents/Resources/AppIcon.icns"
rm -rf "$ICONSET"

cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>                <string>Sequencer</string>
  <key>CFBundleDisplayName</key>         <string>Sequencer</string>
  <key>CFBundleIdentifier</key>          <string>$BUNDLE_ID</string>
  <key>CFBundleExecutable</key>          <string>Sequencer</string>
  <key>CFBundleIconFile</key>            <string>AppIcon</string>
  <key>CFBundlePackageType</key>         <string>APPL</string>
  <key>CFBundleShortVersionString</key>  <string>$VERSION</string>
  <key>CFBundleVersion</key>             <string>$VERSION</string>
  <key>LSMinimumSystemVersion</key>      <string>12.0</string>
  <key>NSHighResolutionCapable</key>     <true/>
  <key>NSPrincipalClass</key>            <string>NSApplication</string>
</dict>
</plist>
PLIST

echo "Signing (ad hoc)..."
# codesign treats every file in Contents/MacOS (managed .dll files included) as nested code, so sign each
# one, then the executable, then the bundle as a whole.
sign() { codesign --force --sign - --timestamp=none "$1" 2>&1 | grep -v "replacing existing signature" || true; }
while IFS= read -r -d '' file; do sign "$file"; done < <(find "$APP/Contents/MacOS" -type f ! -name Sequencer -print0)
sign "$APP/Contents/MacOS/Sequencer"
sign "$APP"
codesign --verify --strict "$APP"

echo "Built $APP ($(du -sh "$APP" | cut -f1))"
echo "Open it with:  open \"$APP\"   (or drag it into /Applications)"
