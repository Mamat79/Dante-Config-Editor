#!/usr/bin/env bash
set -euo pipefail

# Ce script doit tourner sur macOS : il publie l'application autonome,
# construit le bundle .app, applique une signature ad hoc puis crée le DMG.
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
RID="${1:-}"

case "$RID" in
  osx-arm64)
    ARCH_LABEL="AppleSilicon"
    ;;
  osx-x64)
    ARCH_LABEL="Intel"
    ;;
  *)
    echo "Usage: $0 osx-arm64|osx-x64" >&2
    exit 2
    ;;
esac

PROJECT="$ROOT/src/DanteConfigEditor.Mac/DanteConfigEditor.Mac.csproj"
STAGING="$ROOT/tmp/macos/$RID"
PUBLISH="$STAGING/publish"
APP="$STAGING/Dante Config Editor 2026.1.app"
CONTENTS="$APP/Contents"
MACOS="$CONTENTS/MacOS"
RESOURCES="$CONTENTS/Resources"
ICONSET="$STAGING/DanteEdit.iconset"
DMG_STAGE="$STAGING/dmg"
DIST="$ROOT/dist/macos"
DMG="$DIST/DanteConfigEditor2026_1_macOS_${ARCH_LABEL}.dmg"

rm -rf "$STAGING"
mkdir -p "$PUBLISH" "$MACOS" "$RESOURCES/Docs" "$ICONSET" "$DMG_STAGE" "$DIST"
mkdir -p "$RESOURCES/Resources/Support"
mkdir -p "$RESOURCES/Machine Banks"

dotnet publish "$PROJECT" \
  -c Release \
  -r "$RID" \
  --self-contained true \
  -p:PublishTrimmed=false \
  -p:DebugType=None \
  -p:DebugSymbols=false \
  -o "$PUBLISH"

cp -R "$PUBLISH/." "$MACOS/"
chmod +x "$MACOS/DanteConfigEditorV3.Mac"

if [[ -d "$MACOS/Docs" ]]; then
  cp -R "$MACOS/Docs/." "$RESOURCES/Docs/"
  rm -rf "$MACOS/Docs"
fi

cp "$ROOT/packaging/macos/Info.plist" "$CONTENTS/Info.plist"
cp "$ROOT/Resources/Support/paypal-support-qr.png" \
  "$RESOURCES/Resources/Support/paypal-support-qr.png"
cp -R "$ROOT/Resources/MachineBanks/Bundled/DCE Generic Roles 2026.1" \
  "$RESOURCES/Machine Banks/"
cp -R "$ROOT/Resources/MachineBanks/Bundled/DCE Community Devices 2026.1" \
  "$RESOURCES/Machine Banks/"

# L'icône historique est déclinée aux tailles attendues par iconutil.
ICON_SOURCE="$ROOT/packaging/macos/DanteEdit.png"
sips -z 16 16 "$ICON_SOURCE" --out "$ICONSET/icon_16x16.png" >/dev/null
sips -z 32 32 "$ICON_SOURCE" --out "$ICONSET/icon_16x16@2x.png" >/dev/null
sips -z 32 32 "$ICON_SOURCE" --out "$ICONSET/icon_32x32.png" >/dev/null
sips -z 64 64 "$ICON_SOURCE" --out "$ICONSET/icon_32x32@2x.png" >/dev/null
sips -z 128 128 "$ICON_SOURCE" --out "$ICONSET/icon_128x128.png" >/dev/null
sips -z 256 256 "$ICON_SOURCE" --out "$ICONSET/icon_128x128@2x.png" >/dev/null
sips -z 256 256 "$ICON_SOURCE" --out "$ICONSET/icon_256x256.png" >/dev/null
sips -z 512 512 "$ICON_SOURCE" --out "$ICONSET/icon_256x256@2x.png" >/dev/null
sips -z 512 512 "$ICON_SOURCE" --out "$ICONSET/icon_512x512.png" >/dev/null
sips -z 1024 1024 "$ICON_SOURCE" --out "$ICONSET/icon_512x512@2x.png" >/dev/null
iconutil -c icns "$ICONSET" -o "$RESOURCES/DanteEdit.icns"

plutil -lint "$CONTENTS/Info.plist"
xattr -cr "$APP"

# Sans certificat Apple Developer, cette signature garantit seulement
# l'intégrité locale. La notarisation reste nécessaire pour éviter l'alerte Gatekeeper.
codesign --force --deep --sign - --timestamp=none "$APP"
codesign --verify --deep --strict "$APP"

cp -R "$APP" "$DMG_STAGE/"
mkdir -p "$DMG_STAGE/Machine Banks"
cp "$ROOT/machine-banks/DCE_Generic_Roles_2026_1.dce-bank.zip" \
  "$DMG_STAGE/Machine Banks/"
cp "$ROOT/machine-banks/DCE_Community_Devices_2026_1.dce-bank.zip" \
  "$DMG_STAGE/Machine Banks/"
cp "$ROOT/machine-banks/README.md" \
  "$DMG_STAGE/Machine Banks/README.md"
ln -s /Applications "$DMG_STAGE/Applications"
rm -f "$DMG"
hdiutil create \
  -volname "Dante Config Editor 2026.1" \
  -srcfolder "$DMG_STAGE" \
  -ov \
  -format UDZO \
  "$DMG" >/dev/null
hdiutil verify "$DMG" >/dev/null
DMG_DIR="$(dirname "$DMG")"
DMG_NAME="$(basename "$DMG")"
(
  cd "$DMG_DIR"
  shasum -a 256 "$DMG_NAME" > "$DMG_NAME.sha256"
)

echo "DMG créé : $DMG"
