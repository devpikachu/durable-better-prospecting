## 21.2.0
**:green_heart: CI/CD**
- Added `pull: true` to all steps
- Removed redundant matrix definition

**:arrow_up: Dependencies**
- Upgraded to `Cake.Frosting@5.1.0`
- Upgraded to `Common.Build@0.5.1`
- Upgraded to `Common.Build.Generator@0.5.1`
- Upgraded to `Common.Mod@0.6.0`
- Upgraded to `Common.Mod.Generator@0.6.0`

## 21.1.3
**:bug: Bug Fixes**
- Fix CTD due to improper registering of `OnAssetsLoaded` event handler
- Fix CTD due to null `Readings` array access in `ReadingPacket`

## 21.1.2
**:bug: Bug Fixes**
- Fix CTD due to accessing assets before `ASSETS_LOADED`

## 21.1.1
**:bug: Bug Fixes**
- Fix CTD due to a null `Privileges` array

## 21.1.0
**:question: Other**
- Complete rewrite
