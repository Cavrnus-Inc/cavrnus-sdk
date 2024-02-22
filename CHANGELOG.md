# Changelog

## [0.1.0] - 2023-12-01

### Added
- Initial package creation.

## [0.2.0] - 2023-12-11

### Added
- Add Ar components to handle locally adjusting world origin of Ar tracked objects.

## [1.0.0] - 2024-01-03

### Added
- Add permissions demo showcasing restricting desired UI elements in a scene.
- Add smooth copresence along with other enhancements to avatar system.
- Demo assets are now URP compliant.
- Fix flipped video streams.
- Remove Magic Leap specific option in cav settings object.

## [1.1.0] - 2024-01-05

### Added
- Add v1 streamboard functionality.
- Added local avatar components for copresence.

## [1.2.0] - 2024-01-23

### Added
- Cavrnus Core as the primary setup mechanism.

## [1.3.0] - 2024-01-24

### Added
- Major folder reorg. Moved core samples content into main sdk folder.

## [1.4.0] - 2024-01-24

### Changed
-Project core folder reorganization along with misc code cleanup.

## [1.5.0] - 2024-01-29

### Added
- Cavrnus core now has messaging for optional canvases.

### Fixed
- Cleaned up Welcome Modal. Auto load toggle feature implemented.

## [2.0.0] - 2024-02-08

### Changed
- Refactored all functions into CavrnusFunctionLibrary
- Reworked namespaces to match the new structure

## [2.0.1] - 2024-02-08

### Removed
- Remove bad using

## [2.1.0] - 2024-02-09

### Changed
- CavrnusPropertiesContainer names are now absolute, and are automatically filled in and managed for users by default


## [2.1.1] - 2024-02-12

### Fixed
- Both local and remote video streams now update correctly


## [2.1.2] - 2024-02-13

### Added
- Added new temporary banner for welcome modal and fix pathing issues


## [2.1.3] - 2024-02-14

### Added
- Add new welcome modal with updated graphics

### Fixed
- Fixes to scenes with broken prefabs and configurating core asset
- Fixes to avatar components


## [2.1.4] - 2024-02-15

### Fixed
- Fixed some pathing issues with editor windows

## [2.2.0] - 2024-02-16

### Added
- Finished Holo Loading & Samples


## [2.2.1] - 2024-02-16

### Fixed
- Local user transforms are the only send-only option


## [2.2.2] - 2024-02-16

### Fixed
- Fixed Holo Sample Path


## [2.2.3] - 2024-02-17

### Fixed
- Fixed file data paths on Android/iOS/MacOS


## [2.2.4] - 2024-02-17

### Fixed
- Added #if check for older Unity version without VisionOS


## [2.2.5] - 2024-02-19

### Added
- Add library search for holos


## [2.2.6] - 2024-02-20

### Fixed
- Increased Physics Lambda for Transforms


## [2.2.7] - 2024-02-21

### Fixed
- Fixed spawned objects popping from origin by delaying 3 frames to let props catch up


## [2.2.8] - 2024-02-21

### Fixed
- Fixes to CSC remote avatar spawning
- Welcome modal verbiage changes
- User name tag works again


## [2.2.9] - 2024-02-21

### Fixed
- Further Avatar Spawn Improvements
