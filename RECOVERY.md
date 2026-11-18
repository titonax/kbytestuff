# Recovery from the owner's saved 2.1.1.0 executables

The original fork `titonax/Wand-Enhancer` returns HTTP 451 (DMCA). Its custom workflows and full source history were not recoverable through GitHub. The owner supplied the last downloaded offline and update-notifications executables for static inspection, without executing either application.

| Variant | Bytes | SHA256 | Embedded build stamp |
| --- | ---: | --- | --- |
| offline | 2053120 | `20ab85e490588b88c777d10ca3826456556a2015fe13a201b486cf3c4edd16db` | `f8bb40a` |
| update-notifications | 2071040 | `e816eade14c36c95c9c75d35f959d8bdab3962788dc5095eb4cf98f68916acab` | `e623547` |

Both declare assembly version 2.1.1.0 and .NET Framework 4.8, x64. The update variant contains the old fork release endpoints `https://api.github.com/repos/titonax/Wand-Enhancer/releases/latest` and `/releases?per_page=20`. The current notifier uses the new fork identity instead.

## Reconstructed files

- `WandEnhancer/Core/EnhancerConfig.cs`
- `WandEnhancer/Core/Patching/Shared/ElectronFuseWire.cs`
- `WandEnhancer/Core/Patching/Shared/DiskFusePatch.cs`
- `WandEnhancer/Patches/pro-account-reducer.js`
- `WandEnhancer/Patches/pro-subscription.js`

ILSpy command line 11.2.0.10050 reconstructed the three C# types with the CSharp7_3 output option. The results for these three types are byte-for-byte identical when separately decompiled from both executables. The two JavaScript files were extracted as embedded resources, not recreated from guesses. Original variable names, formatting and comments cannot be recovered from release CIL; these C# files are reconstructed source, not claimed to be the originals.

The recovered implementation comes from 2.1.1.0 and is combined with the available upstream 2.2.1.0 tree. Compiling and running the project's regression gates is required to establish compatibility. Passing those gates does not establish compatibility with every current Wand installation. No original Wand installation or proprietary extracted bundle was used in this recovery.

## Static evidence

REA artifact inspection: offline `ev_acae545aa865dc4313e080b43ecd230aba636facd03da1a834f991793a4ac9a8`; update `ev_ef856116b152f5297404d702368605379eb58e3a36e7ffca64146d939b6a2904`. Selected-member evidence `ev_4408d40adfd2d03416bcddc4b50e2e81d94c15186ab333dc1b5a33388ce3d733` observes all three recovered types, the notifier and constants in the update binary. REA metadata inspection did not execute either target; C# reconstruction is a separate ILSpy inference.
