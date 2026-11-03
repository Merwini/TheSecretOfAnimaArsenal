# Combat Extended compatibility assembly

Add Combat Extended compatibility C# files in this project directory. The main Arsenal project references no CE code; this project references Arsenal and CE.

The project targets RimWorld 1.6 (`net48`) and 1.7 (`net10.0`). Both targets compile the same compatibility source files, referencing the matching Arsenal and game assemblies. Their outputs and intermediate files are separated by version:

| Game version | Framework | Release output |
| --- | --- | --- |
| 1.6 | net48 | `CE/1.6/Assemblies/TheSecretOfAnimaArsenal.CE.dll` |
| 1.7 | net10.0 | `CE/1.7/Assemblies/TheSecretOfAnimaArsenal.CE.dll` |

The existing XML patches remain in `CE/Defs` and `CE/Patches`. Each assembly folder loads only when CE is active under the matching game version.

Use **Build CE Release 1.6**, **Build CE Release 1.7**, or **Build CE Release** in VS Code. From the mod root, build both targets with:

```powershell
dotnet build Source/TheSecretOfAnimaArsenal.CE/TheSecretOfAnimaArsenal.CE.csproj --configuration Release
```

To select one version, add `--framework net48` or `--framework net10.0`. The matching Arsenal assembly is built automatically. Referenced dependency DLLs are not copied into the CE output folders.

The default `CombatExtendedDir` uses this Steam installation's Workshop folder. For each target, reference discovery first checks `<CombatExtendedDir>/<game version>/Assemblies/CombatExtended.dll`, then falls back to `<CombatExtendedDir>/Assemblies/CombatExtended.dll` for CE installations with a shared root assembly folder. The installed 1.6 DLL currently supplies that root fallback; the 1.7 target assumes the future CE API remains compatible with the compatibility source.

When using separate CE installations, provide version-specific DLL paths:

```powershell
dotnet build Source/TheSecretOfAnimaArsenal.CE/TheSecretOfAnimaArsenal.CE.csproj --configuration Release "-p:CombatExtended16Dll=C:/CE-1.6/Assemblies/CombatExtended.dll" "-p:CombatExtended17Dll=C:/CE-1.7/Assemblies/CombatExtended.dll"
```

You can also override `CombatExtendedDir` for another CE root folder. `CombatExtendedDll` remains available as a direct override for the selected build; it takes precedence over discovery and the version-specific properties. Set `RimWorldDir` if the 1.7 game assemblies are installed elsewhere.

**Build Release** continues to build the main mod without requiring CE. **Build All Release** builds the solution, including both main-mod targets and both CE targets. When CE 1.7 is available, rebuilding against its versioned folder or setting `CombatExtended17Dll` requires no project, task, or load-folder changes.
