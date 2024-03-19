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


## [2.2.10] - 2024-02-21

### Fixed
- Fixed minor Avatar Manager warnings


## [2.2.11] - 2024-02-22

### Fixed
- Fixed property flickering due to value bounce-back


## [2.2.12] - 2024-02-23

### Fixed
- Fixed statically-defined user props being redefined/overridden


## [2.2.13] - 2024-02-23

### Changed
- Changed both package and github repo name
- Updated pathing references as well

### Fixed
- Including a few meta files in transfer that were missing


## [2.2.14] - 2024-02-23

### Changed
- Changed the CSC defaults to be blank for customers to fill in


## [2.2.15] - 2024-02-23

### Fixed
- Fixed Nametag default Property Name

## [2.2.16] - 2024-02-23

### Fixed
- Avatar nametag rotation and transform property names



## [2.2.17] - 2024-02-23

### Added
- Reverted a few changes with avatar propertynames
- Fix local user setup helper to be capital Transform


## [2.2.18] - 2024-02-27

### Changed
- Updated login flow options with new verbiage as well
- Updated documentation links


## [2.3.0] - 2024-02-27

### Fixed
- Missing holo library sprites 


## [2.3.1] - 2024-02-27

### Fixed
- Fixed error where ui canvas is missing from CSC


## [2.3.2] - 2024-02-27

### Added
- SyncWorldTransform sync component


## [2.4.0] - 2024-02-28

### Added
- User Tokens will now automatically cache if told to

### Fixed
- Users post their position properly on join


## [2.4.1] - 2024-02-28

### Fixed
- User tokens are cleared properly if Save is not set


## [2.4.2] - 2024-02-29

### Added
- SyncXrCameraTransform no-code component

### Changed
- Now have specific Sync transform components for Local and World.


## [2.4.3] - 2024-03-05

### Fixed
- Fixed Ping-Ponging Property Values when multiple Transients conflict


## [2.4.4] - 2024-03-05

### Added
- Added sync components for material textures and sprites
- Added several new demo scenes for various methods of updating materials/textures


## [2.4.5] - 2024-03-05

### Fixed
- Fixed change detection epsilons for colors, floats, and vectors


## [2.4.6] - 2024-03-05

### Fixed
- Fixed Space Picker error


## [2.4.7] - 2024-03-05

### Added
- Added a new feature

### Changed
- 

### Deprecated
- 

### Removed
- 

### Fixed
- Fixed problems when locally cached user token is invalid


## [2.4.8] - 2024-03-06

### Fixed
- Fixed space picker loading UI sticking around


## [2.4.9] - 2024-03-06

### Changed
- Changed several menus to be visually consistent rest of UI



## [2.4.10] - 2024-03-07

## [2.4.10] - 2024-03-07

### Changed

- User login flow to account for member and guest auth tokens


### Fixed

- Fixed missing icons in space picker menu.

- Space picker menu results are now sorted alphabetically.

- Space picker menu always shows available spaces.


## [2.4.11] - 2024-03-11

### Fixed
- Fixed euler angle properties detecting changes when none are present


## [2.4.13] - 2024-03-15

### Added
- Added package registry


## [2.4.14] - 2024-03-18

### Added
- Changes to README


## [2.4.15] - 2024-03-18

### Fixed
- Fixed README image paths


## [2.4.16] - 2024-03-19

### Fixed
- Fixed user transients accidentally finalizing, causing unexpected properties to show
