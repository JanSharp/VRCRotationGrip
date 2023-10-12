
# Changelog

## [1.1.4] - 2023-10-12

### Changed

- Update vrc worlds dependency to 3.4.x ([`b0eb651`](https://github.com/JanSharp/VRCRotationGrip/commit/b0eb6518a57f9f51041e1d283b6c23399303eeec))

### Removed

- Remove udonsharp dependency as it has been merged into worlds ([`b0eb651`](https://github.com/JanSharp/VRCRotationGrip/commit/b0eb6518a57f9f51041e1d283b6c23399303eeec))

## [1.1.3] - 2023-08-05

### Changed

- Change LICENSE.txt to LICENSE.md so Unity sees it in the package manager window ([`d5577b3`](https://github.com/JanSharp/VRCRotationGrip/commit/d5577b383fd8e92654e41d835df01072ec622976))

### Added

- Add vpm dependency on `com.vrchat.worlds` for clarity ([`c789398`](https://github.com/JanSharp/VRCRotationGrip/commit/c789398d362cf83121dbf891e9559b4b4ac08b88))

### Fixed

- Fix build error on publish ([`8e66a3b`](https://github.com/JanSharp/VRCRotationGrip/commit/8e66a3bb8f405896a94245f9ea89335dce5b7a06))

## [1.1.2] - 2023-07-23

### Fixed

- Fix license to match the MIT "variant" used by Unity ([`3d198c0`](https://github.com/JanSharp/VRCRotationGrip/commit/3d198c024eef800298dd0a57270f4a08bba1e40c))

## [1.1.1] - 2023-07-18

_First version of this package that is in the VCC listing._

### Changed

- **Breaking:** Separate Rotation Grip into its own repo ([`9c6f3f4`](https://github.com/JanSharp/VRCRotationGrip/commit/9c6f3f416b9a9fae2883ff834133f684f8067d5d), [`d30da01`](https://github.com/JanSharp/VRCRotationGrip/commit/d30da0168fd0360210ae40ee8d55ce97b5326990), [`b44919a`](https://github.com/JanSharp/VRCRotationGrip/commit/b44919a716e4db9c5e5e3157ec38386cb116d36b))
- **Breaking:** Update OnBuildUtil and other general editor scripting, use SerializedObjects ([`de04745`](https://github.com/JanSharp/VRCRotationGrip/commit/de04745880f0ea37345b5fd4e54de94fe7f05368), [`ee4ffb5`](https://github.com/JanSharp/VRCRotationGrip/commit/ee4ffb5ffe6218097cd01b94becc93bafb6ad2ca))

### Added

- Add a note about max deviation oddities in readme ([`0eae3d5`](https://github.com/JanSharp/VRCRotationGrip/commit/0eae3d5b416e404b5b67e1c1e94eec1a3fc6a5a5))
- Add installation instructions to readme ([`95cfd4a`](https://github.com/JanSharp/VRCRotationGrip/commit/95cfd4abbba6e624cf6f977ae05a5f48450a0c97))

### Fixed

- Fix exception on build for newly created rotation grips ([`eccab84`](https://github.com/JanSharp/VRCRotationGrip/commit/eccab84c79b69390752b011c1a02071e62a73bfc))

## [1.1.0] - 2023-06-11

### Changed

- **Breaking:** Remove and change use of deprecated UdonSharp editor functions ([`c258db3`](https://github.com/JanSharp/VRCRotationGrip/commit/c258db370cbff404fd91d66aae48c018645ca7f4))
- **Breaking:** Use refactored OnBuildUtil ([`17dbab8`](https://github.com/JanSharp/VRCRotationGrip/commit/17dbab84b8bb6bad192d67607a5f45c8cd000356))
- Migrate to VRChat Creator Companion ([`9ae838c`](https://github.com/JanSharp/VRCRotationGrip/commit/9ae838cf1d6280c64c607559fb3ae9967b52bd99), [`78b73b6`](https://github.com/JanSharp/VRCRotationGrip/commit/78b73b6816612602b04daafeb4097351f087c01a), [`4d5a41d`](https://github.com/JanSharp/VRCRotationGrip/commit/4d5a41deec90b17ac11aa3d3458cb8f78133d8e9))

## [1.0.0] - 2022-08-19

### Added

- Snap RotationGrips before entering play mode or building, using OnBuildUtil ([`bb402d6`](https://github.com/JanSharp/VRCRotationGrip/commit/bb402d6df1af7e28d51cea9d660b6ea2e4669353))
- Add syncing for everyone including late joiners ([`a00edb5`](https://github.com/JanSharp/VRCRotationGrip/commit/a00edb557a82918a931b649bd2ac457717f69f66), [`22c503d`](https://github.com/JanSharp/VRCRotationGrip/commit/22c503d20369928c129d079625a71c3a7a2f3bd2), [`b8c9292`](https://github.com/JanSharp/VRCRotationGrip/commit/b8c92929b7a9cbabb727ba03e89f4d7ab251155d), [`10e0cea`](https://github.com/JanSharp/VRCRotationGrip/commit/10e0cea91798d5106b2a9353e5478e30d2904e5c), [`3c8a8a1`](https://github.com/JanSharp/VRCRotationGrip/commit/3c8a8a16c9a7b8ab5128af654f53d24e15cf7acf))
- Add restriction to one axis ([`fecbc90`](https://github.com/JanSharp/VRCRotationGrip/commit/fecbc909b77f4c577f9b65fb925a873787300ab8))
- Support rotating tilted objects ([`aedf0b7`](https://github.com/JanSharp/VRCRotationGrip/commit/aedf0b77bcaee85c35222bb5d54c982a24d56364))
- Add Snap in line editor feature ([`6fa5762`](https://github.com/JanSharp/VRCRotationGrip/commit/6fa57625f8f2a3c7f559d723bca23177c3380731), [`3e4a918`](https://github.com/JanSharp/VRCRotationGrip/commit/3e4a918dc3f5f01b50506814b41a39e2a23d1771))
- Add maximum rotation deviation from original rotation ([`5c88825`](https://github.com/JanSharp/VRCRotationGrip/commit/5c88825191e78c650f77d59423b35467a18ee0a1), [`b713043`](https://github.com/JanSharp/VRCRotationGrip/commit/b7130434d42b4987ffae9ab0cf468296ee2fc4f7))
- Use hand bone position in VR ([`154f21e`](https://github.com/JanSharp/VRCRotationGrip/commit/154f21ee2a11095f36bb6ea436724d1cd64a0b38))
- Add snapping of the pickup objects in line with the targeted/rotated object ([`a00edb5`](https://github.com/JanSharp/VRCRotationGrip/commit/a00edb557a82918a931b649bd2ac457717f69f66), [`d5611e2`](https://github.com/JanSharp/VRCRotationGrip/commit/d5611e20804c9924e71dcfdb79c0091d7b485c5f))
- Add custom inspector ([`7f579f1`](https://github.com/JanSharp/VRCRotationGrip/commit/7f579f166a143111a46671833119ca33814382e3))
- Add rotation lerping ([`ca735f0`](https://github.com/JanSharp/VRCRotationGrip/commit/ca735f0575c217e670cd2e389651a67abcdfb11d))
- Add RotationGrip script ([`a00edb5`](https://github.com/JanSharp/VRCRotationGrip/commit/a00edb557a82918a931b649bd2ac457717f69f66))

<!-- RotationGrip_v1.1.0 -->
<!-- RotationGrip_v1.0.0 -->

[1.1.4]: https://github.com/JanSharp/VRCRotationGrip/releases/tag/v1.1.4
[1.1.3]: https://github.com/JanSharp/VRCRotationGrip/releases/tag/v1.1.3
[1.1.2]: https://github.com/JanSharp/VRCRotationGrip/releases/tag/v1.1.2
[1.1.1]: https://github.com/JanSharp/VRCRotationGrip/releases/tag/v1.1.1
[1.1.0]: https://github.com/JanSharp/VRCRotationGrip/releases/tag/RotationGrip_v1.1.0
[1.0.0]: https://github.com/JanSharp/VRCRotationGrip/releases/tag/RotationGrip_v1.0.0
