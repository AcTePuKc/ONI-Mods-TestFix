make todo for this:
- Scan the repo for reflection in hot paths (done by this script).
- Triage: HOT (per-tick/UI) vs COLD (init/config).
- For HOT, replace reflection with cached delegates; keep behavior identical.
- Work in small PR-sized batches; re-scan after each batch.
- Acceptance: zero HOT reflection left; no per-call allocations.

---
# Task: U57 animation API update
- Regenerate the U57 publicized ONI assemblies (notably `Assembly-CSharp_public.dll`) using a local
  workstation with the ONI toolchain, then commit the refreshed binaries to `src/lib`.
- Rebuild the solution (including PLib) against the new API and update any
  `KAnimControllerBase.Play` invocations if the overload changed.
- Launch ONI and trigger a minion reaction to confirm the updated animation path runs without
  exceptions.

---
# Task: Replace hot-path reflection with cached delegates (ONI mods)
(See performance brief; scope must stay tight.)

## Find candidates (current run)
HOT entries first under each project.

## Project: Sgt_Imalas-Oni-Mods
### Sgt_Imalas-Oni-Mods\AkiTrueTiles_SkinSelectorAddon\Patches\TrueTilePatches.cs
- **HOT** @ L97 — `var m_Add = t_TileAssets.GetMethod("Add");`
- **HOT** @ L98 — `var postfix = typeof(TileAssets_Add_Patch).GetMethod("Postfix");`

### Sgt_Imalas-Oni-Mods\ClusterTraitGenerationManager\Dlc2Patches.cs
- **HOT** @ L143 — `yield return typeof(OrbitalResearchDatabankConfig).GetMethod(name);`
- **HOT** @ L144 — `yield return typeof(ResearchDatabankConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\MeteorMigration\Patches.cs
- **HOT** @ L68 — `//                typeof(WorldContainer).GetField("m_seasonIds", BindingFlags.NonPublic \| BindingFlags.Instance).SetValue(__instance, new List<string>());`

### Sgt_Imalas-Oni-Mods\OniRetroEdition\BlockTileRendererPatch.cs
- **HOT** @ L32 — `//                var m_MatchesDef = typeof(BlockTileRenderer).GetMethod("MatchesDef", BindingFlags.NonPublic \| BindingFlags.Static);`
- **HOT** @ L33 — `//                var m_MatchesElement = typeof(BlockTileRenderer_GetConnectionBits_Patch).GetMethod("MatchesElement", new Type[]`

### Sgt_Imalas-Oni-Mods\Robo Rockets\Patches\RoboRocketPatches.cs
- **HOT** @ L291 — `int worldRefID = (int)typeof(ClustercraftExteriorDoor).GetField("targetWorldId", BindingFlags.NonPublic \| BindingFlags.Instance).GetValue(__instance);`

### Sgt_Imalas-Oni-Mods\Rockets-TinyYetBig\Patches\BugfixPatches.cs
- **HOT** @ L134 — `harmony.Patch(OxidizerTank_Set_UserMaxCapacity_Patch_IncorporatedFromStockBugFix.TargetMethod(), postfix: new HarmonyMethod(postfixMethod));`

### Sgt_Imalas-Oni-Mods\Rockets-TinyYetBig\Patches\BuildingDefPatches\CargoBayConfig_Patches.cs
- **HOT** @ L31 — `yield return typeof(LiquidCargoBaySmallConfig).GetMethod(name);`
- **HOT** @ L33 — `yield return typeof(SolidCargoBayClusterConfig).GetMethod(name);`
- **HOT** @ L34 — `yield return typeof(SolidCargoBaySmallConfig).GetMethod(name);`
- **HOT** @ L54 — `yield return typeof(LiquidCargoBaySmallConfig).GetMethod(name);`
- **HOT** @ L56 — `yield return typeof(SolidCargoBayClusterConfig).GetMethod(name);`
- **HOT** @ L57 — `yield return typeof(SolidCargoBaySmallConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\SetStartDupes\Patches.cs
- **HOT** @ L1856 — `GameObject parentToScale = __instance.containerParent;// (GameObject)typeof(CharacterSelectionController).GetField("containerParent", BindingFlags.NonPublic \| BindingFlags.Instance).GetValue(__instance);`
- **HOT** @ L1857 — `CharacterContainer prefabToScale = __instance.containerPrefab; //(CharacterContainer)typeof(CharacterSelectionController).GetField("containerPrefab", BindingFlags.NonPublic \| BindingFlags.Instance).GetValue(__instance);`

### Sgt_Imalas-Oni-Mods\3GuBsVisualFixesNTweaks\Patches\BuildingConfig_Patches.cs
- COLD @ L28 — `yield return typeof(LiquidConditionerConfig).GetMethod(name);`
- COLD @ L29 — `yield return typeof(LiquidPumpConfig).GetMethod(name);`
- COLD @ L30 — `yield return typeof(LiquidMiniPumpConfig).GetMethod(name);`
- COLD @ L45 — `yield return typeof(SteamTurbineConfig2).GetMethod(name);`
- COLD @ L46 — `yield return typeof(AutoMinerConfig).GetMethod(name);`
- COLD @ L47 — `yield return typeof(VerticalWindTunnelConfig).GetMethod(name);`
- COLD @ L49 — `yield return typeof(RocketInteriorGasInputConfig).GetMethod(name);`
- COLD @ L50 — `yield return typeof(RocketInteriorGasOutputConfig).GetMethod(name);`
- COLD @ L51 — `yield return typeof(RocketInteriorLiquidInputConfig).GetMethod(name);`
- COLD @ L52 — `yield return typeof(RocketInteriorLiquidOutputConfig).GetMethod(name);`
- COLD @ L53 — `yield return typeof(RocketInteriorSolidOutputConfig).GetMethod(name);`
- COLD @ L54 — `yield return typeof(RocketInteriorSolidInputConfig).GetMethod(name);`
- COLD @ L55 — `yield return typeof(RocketInteriorPowerPlugConfig).GetMethod(name);`
- COLD @ L98 — `yield return typeof(LadderConfig).GetMethod(name);`
- COLD @ L99 — `yield return typeof(LadderFastConfig).GetMethod(name);`
- COLD @ L100 — `yield return typeof(FirePoleConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\3GuBsVisualFixesNTweaks\Patches\DoubleAnimFix_Patches.cs
- COLD @ L42 — `yield return typeof(ApothecaryConfig).GetMethod(name);`
- COLD @ L43 — `yield return typeof(GlassForgeConfig).GetMethod(name);`
- COLD @ L44 — `yield return typeof(GourmetCookingStationConfig).GetMethod(name);`
- COLD @ L45 — `yield return typeof(MetalRefineryConfig).GetMethod(name);`
- COLD @ L46 — `yield return typeof(SuitFabricatorConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\_WorldGenStateCapture\WorldStateData\WorldPOIs\WorldPOI_OnSpawn_Patches.cs
- COLD @ L146 — `yield return typeof(HeadquartersConfig).GetMethod(name);`
- COLD @ L149 — `yield return typeof(WarpConduitSenderConfig).GetMethod(name);`
- COLD @ L150 — `yield return typeof(WarpConduitReceiverConfig).GetMethod(name);`
- COLD @ L153 — `yield return typeof(MassiveHeatSinkConfig).GetMethod(name);`
- COLD @ L156 — `yield return typeof(TemporalTearOpenerConfig).GetMethod(name);`
- COLD @ L159 — `yield return typeof(GravitasPedestalConfig).GetMethod(name);`
- COLD @ L178 — `yield return typeof(GeneShufflerConfig).GetMethod(name);`
- COLD @ L181 — `yield return typeof(SapTreeConfig).GetMethod(name);`
- COLD @ L184 — `yield return typeof(WarpPortalConfig).GetMethod(name);`
- COLD @ L185 — `yield return typeof(WarpReceiverConfig).GetMethod(name);`
- COLD @ L188 — `yield return typeof(PropSurfaceSatellite1Config).GetMethod(name);`
- COLD @ L189 — `yield return typeof(PropSurfaceSatellite2Config).GetMethod(name);`
- COLD @ L190 — `yield return typeof(PropSurfaceSatellite3Config).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\BawoonFwiend\BawoongiverWorkable.cs
- COLD @ L107 — `//    var RandomizeMethod = VaricolouredBalloonsHelperType.GetMethod("RandomizeArtistBalloonSymbolIdx", BindingFlags.Instance \| BindingFlags.NonPublic);`

### Sgt_Imalas-Oni-Mods\BrokenRocketInteriorPortFix\Patches.cs
- COLD @ L110 — `yield return typeof(RocketInteriorGasInputPortConfig).GetMethod(name);`
- COLD @ L111 — `yield return typeof(RocketInteriorGasOutputPortConfig).GetMethod(name);`
- COLD @ L112 — `yield return typeof(RocketInteriorLiquidInputPortConfig).GetMethod(name);`
- COLD @ L113 — `yield return typeof(RocketInteriorLiquidOutputPortConfig).GetMethod(name);`
- COLD @ L114 — `yield return typeof(RocketEnvelopeWindowTileConfig).GetMethod(name);`
- COLD @ L115 — `yield return typeof(RocketWallTileConfig).GetMethod(name);`
- COLD @ L135 — `yield return typeof(RocketInteriorGasInputPortConfig).GetMethod(name);`
- COLD @ L136 — `yield return typeof(RocketInteriorGasOutputPortConfig).GetMethod(name);`
- COLD @ L137 — `yield return typeof(RocketInteriorLiquidInputPortConfig).GetMethod(name);`
- COLD @ L138 — `yield return typeof(RocketInteriorLiquidOutputPortConfig).GetMethod(name);`
- COLD @ L139 — `yield return typeof(RocketEnvelopeWindowTileConfig).GetMethod(name);`
- COLD @ L140 — `yield return typeof(RocketWallTileConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\Cheese\CheeseRats\GroneHogPatches.cs
- COLD @ L78 — `internal static MethodBase TargetMethod()`

### Sgt_Imalas-Oni-Mods\DuperyFixed\Source\PersonalityOutline.cs
- COLD @ L107 — `//    PropertyInfo targetProperty = outlineType.GetProperty(srcProp.Name);`

### Sgt_Imalas-Oni-Mods\GoodByeFrostByte\Patches.cs
- COLD @ L137 — `yield return typeof(JetSuitConfig).GetMethod(name);`
- COLD @ L138 — `yield return typeof(LeadSuitConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\MeteorMigration\Patches.cs
- COLD @ L76 — `//                        typeof(WorldContainer).GetField("m_seasonIds", BindingFlags.NonPublic \| BindingFlags.Instance).SetValue(__instance,new List<string>(Data.seasons));`

### Sgt_Imalas-Oni-Mods\OniRetroEdition\ModPatches\MeterPatches.cs
- COLD @ L28 — `yield return typeof(FarmStationConfig).GetMethod(name);`
- COLD @ L29 — `yield return typeof(CompostConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\OniRetroEdition\ModPatches\ShockWormPatches.cs
- COLD @ L83 — `internal static MethodBase TargetMethod()`

### Sgt_Imalas-Oni-Mods\OniRetroEdition\Patches.cs
- COLD @ L152 — `yield return typeof(MeshTileConfig).GetMethod(name);`
- COLD @ L153 — `yield return typeof(GasPermeableMembraneConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\Rockets-TinyYetBig\Patches\BugfixPatches.cs
- COLD @ L145 — `internal static MethodBase TargetMethod()`

### Sgt_Imalas-Oni-Mods\Rockets-TinyYetBig\Patches\BuildingDefPatches\CargoBayConfig_Patches.cs
- COLD @ L27 — `yield return typeof(GasCargoBayClusterConfig).GetMethod(name);`
- COLD @ L28 — `yield return typeof(GasCargoBaySmallConfig).GetMethod(name);`
- COLD @ L30 — `yield return typeof(LiquidCargoBayClusterConfig).GetMethod(name);`
- COLD @ L50 — `yield return typeof(GasCargoBayClusterConfig).GetMethod(name);`
- COLD @ L51 — `yield return typeof(GasCargoBaySmallConfig).GetMethod(name);`
- COLD @ L53 — `yield return typeof(LiquidCargoBayClusterConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\Rockets-TinyYetBig\Patches\BuildingDefPatches\RocketInteriorPort_Patches.cs
- COLD @ L31 — `yield return typeof(RocketInteriorGasInputPortConfig).GetMethod(name);`
- COLD @ L32 — `yield return typeof(RocketInteriorGasOutputPortConfig).GetMethod(name);`
- COLD @ L33 — `yield return typeof(RocketInteriorLiquidInputPortConfig).GetMethod(name);`
- COLD @ L34 — `yield return typeof(RocketInteriorLiquidOutputPortConfig).GetMethod(name);`
- COLD @ L58 — `yield return typeof(RocketInteriorGasInputPortConfig).GetMethod(name);`
- COLD @ L59 — `yield return typeof(RocketInteriorGasOutputPortConfig).GetMethod(name);`
- COLD @ L60 — `yield return typeof(RocketInteriorLiquidInputPortConfig).GetMethod(name);`
- COLD @ L61 — `yield return typeof(RocketInteriorLiquidOutputPortConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\Rockets-TinyYetBig\Patches\SpaceStationPatches\SpaceStationPatches.cs
- COLD @ L728 — `yield return typeof(RailGunPayload.StatesInstance).GetMethod("Travel");`
- COLD @ L729 — `yield return typeof(RailGunPayload.StatesInstance).GetMethod("Launch");`

### Sgt_Imalas-Oni-Mods\Rockets-TinyYetBig\TODO\__LegacyContentToSortOut\_ModuleConfig\ModuleConfigurationPatches.cs
- COLD @ L39 — `//                    //MethodInfo method = type.GetMethod("CreateBuildingDef");`

### Sgt_Imalas-Oni-Mods\RonivansLegacy_ChemicalProcessing\Patches\Disease_Patches.cs
- COLD @ L37 — `yield return typeof(ZombieSpores).GetMethod(name,AccessTools.all);`
- COLD @ L38 — `yield return typeof(SlimeGerms).GetMethod(name, AccessTools.all);`
- COLD @ L39 — `yield return typeof(FoodGerms).GetMethod(name, AccessTools.all);`

### Sgt_Imalas-Oni-Mods\RonivansLegacy_ChemicalProcessing\Patches\HPA\ConduitCapacityDescriptor_Patches.cs
- COLD @ L31 — `yield return typeof(SolidConduitBridgeConfig).GetMethod(name, AccessTools.all);`
- COLD @ L32 — `yield return typeof(LiquidConduitBridgeConfig).GetMethod(name, AccessTools.all);`
- COLD @ L33 — `yield return typeof(GasConduitBridgeConfig).GetMethod(name, AccessTools.all);`
- COLD @ L35 — `yield return typeof(SolidConduitConfig).GetMethod(name, AccessTools.all);`
- COLD @ L37 — `yield return typeof(LiquidConduitConfig).GetMethod(name, AccessTools.all);`
- COLD @ L38 — `yield return typeof(LiquidConduitRadiantConfig).GetMethod(name, AccessTools.all);`
- COLD @ L39 — `yield return typeof(InsulatedLiquidConduitConfig).GetMethod(name, AccessTools.all);`
- COLD @ L41 — `yield return typeof(GasConduitConfig).GetMethod(name, AccessTools.all);`
- COLD @ L42 — `yield return typeof(GasConduitRadiantConfig).GetMethod(name, AccessTools.all);`
- COLD @ L43 — `yield return typeof(InsulatedGasConduitConfig).GetMethod(name, AccessTools.all);`
- COLD @ L47 — `yield return PlasticGasConduitConfig.GetMethod(name, AccessTools.all);`
- COLD @ L50 — `yield return PlasticLiquidConduitConfig.GetMethod(name, AccessTools.all);`

### Sgt_Imalas-Oni-Mods\RotatableRadboltStorage\Patches.cs
- COLD @ L224 — `yield return typeof(HighEnergyParticleRedirectorConfig).GetMethod(name);`
- COLD @ L225 — `yield return typeof(HighEnergyParticleSpawnerConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\SaveGameModLoader\Patches.cs
- COLD @ L660 — `var ViewRootFinder = typeof(LoadScreen).GetField("colonyViewRoot", BindingFlags.NonPublic \| BindingFlags.Instance);`

### Sgt_Imalas-Oni-Mods\SaveGameModLoader\SyncViewScreen.cs
- COLD @ L74 — `var CloseModScreenMethod = typeof(ModsScreen).GetMethod("Exit", BindingFlags.NonPublic \| BindingFlags.Instance);`
- COLD @ L101 — `var CloseModScreenMethod = typeof(ModsScreen).GetMethod("Exit", BindingFlags.NonPublic \| BindingFlags.Instance);`

### Sgt_Imalas-Oni-Mods\SetStartDupes\UI\UnityCarePackageSelectorScreen.cs
- COLD @ L191 — `var ArtifactPackagesInstance = ArtifactCarePackages_ArtifactImmigration_Type.GetField("Instance", BindingFlags.NonPublic \| BindingFlags.Static).GetValue(null);`
- COLD @ L208 — `var ModifiersInstance = BioInks_ImmigrationModifier_Type.GetProperty("Instance").GetValue(null, null);`

### Sgt_Imalas-Oni-Mods\SkillsInfoScreen\ModIntegration_CleanHud.cs
- COLD @ L71 — `var m_GetConfigInstance = CleanHUD_Options.GetProperty("Opts", BindingFlags.FlattenHierarchy \| BindingFlags.Static \| BindingFlags.Public );`

### Sgt_Imalas-Oni-Mods\SkinEffects\Patches.cs
- COLD @ L45 — `yield return typeof(CornerMouldingConfig).GetMethod(name);`
- COLD @ L46 — `yield return typeof(CrownMouldingConfig).GetMethod(name);`

### Sgt_Imalas-Oni-Mods\UtilLibs\AccessControlPermissionCompat.cs
- COLD @ L49 — `var property = accessControlType.GetProperty(`
- COLD @ L55 — `var getter = property.GetGetMethod(true);`
- COLD @ L124 — `var property = accessControlType.GetProperty(`

### Sgt_Imalas-Oni-Mods\UtilLibs\UtilMethods.cs
- COLD @ L20 — `foreach (var p in s.GetType().GetProperties().Where(p => !p.GetGetMethod().GetParameters().Any()))`


## Project: ONI_Mods_byPether
### ONI_Mods_byPether\DiseasesExpanded\Patches\DiseasesExpanded_Patches_Rust.cs
- **HOT** @ L137 — `var originalWantsOilChange = oilMonitorType.GetMethod(nameof(BionicOilMonitor.WantsOilChange));`
- **HOT** @ L138 — `var postfixWantsOilChange = typeof(BionicOilMonitor_WantsOilChange_Patch)?.GetMethod("Postfix");`
- **HOT** @ L148 — `var originalHasDecentAmountOfOil = oilMonitorType.GetMethod(nameof(BionicOilMonitor.HasDecentAmountOfOil));`
- **HOT** @ L149 — `var postfixHasDecentAmountOfOil = typeof(BionicOilMonitor_HasDecentAmountOfOil_Patch)?.GetMethod("Postfix");`

### ONI_Mods_byPether\DiseasesExpanded\Patches\DiseasesExpanded_Patches_Traits.cs
- **HOT** @ L44 — `public static MethodBase TargetMethod()`

### ONI_Mods_byPether\MoreLogicPorts\MoreLogicPorts_Patches.cs
- **HOT** @ L26 — `MethodInfo patchDef = typeof(CreateBuildingDef_Patch).GetMethod(nameof(CreateBuildingDef_Patch.Postfix));`
- **HOT** @ L27 — `MethodInfo patchConf = typeof(ConfigureBuildingTemplate_Patch).GetMethod(nameof(ConfigureBuildingTemplate_Patch.Postfix));`

### ONI_Mods_byPether\RoomsExpanded\Patches\RoomsExpanded_Patches.cs
- **HOT** @ L38 — `var prefix = typeof(RoomTypes_Constructor_Patch)?.GetMethod("Prefix");`
- **HOT** @ L39 — `var postfix = typeof(RoomTypes_Constructor_Patch)?.GetMethod("Postfix");`

### ONI_Mods_byPether\ConveyorRailDisplay\ManualPatching.cs
- COLD @ L23 — `MethodInfo method = classType.GetMethod(methodName);`

### ONI_Mods_byPether\DietVariety\VarietyMonitor.cs
- COLD @ L184 — `var field = type.GetField(memberName, bindingFlags);`
- COLD @ L194 — `var property = type.GetProperty(memberName, bindingFlags);`
- COLD @ L224 — `var field = type.GetField(memberName, bindingFlags);`
- COLD @ L232 — `var property = type.GetProperty(memberName, bindingFlags);`
- COLD @ L591 — `var invoke = delegateType.GetMethod("Invoke");`
- COLD @ L612 — `var handlerMethod = typeof(VarietyMonitor).GetMethod(methodName, BindingFlags.Instance \| BindingFlags.NonPublic \| BindingFlags.Public);`

### ONI_Mods_byPether\MoreLogicPorts\MoreLogicPorts_Patches.cs
- COLD @ L40 — `MethodInfo origDef = config.GetMethod(LogPorts.BUILDING_DEF_NAME);`
- COLD @ L41 — `MethodInfo origConf = config.GetMethod(ConfigsToPatch[config]);`


## Project: src
### src\AzeLib\Attributes\AMonoBehaviour.cs
- COLD @ L36 — `fieldInfo.SetValue(this, component);`

### src\AzeLib\Buildings\BuildingPrefabAttributeHelper.cs
- COLD @ L208 — `var property = buildingDef.GetType().GetProperty("BuildingComplete", BindingFlags.Public \| BindingFlags.NonPublic \| BindingFlags.Instance);`
- COLD @ L230 — `var property = buildingDef.GetType().GetProperty("PrefabID", BindingFlags.Public \| BindingFlags.NonPublic \| BindingFlags.Instance);`
- COLD @ L235 — `var nameProperty = prefabId.GetType().GetProperty("Name", BindingFlags.Public \| BindingFlags.Instance);`

### src\AzeLib\Extensions\TranspilerExt.cs
- COLD @ L33 — `if (!CallsTargetMethod(instruction, toRemove))`
- COLD @ L63 — `private static bool CallsTargetMethod(CodeInstruction instruction, MethodInfo toRemove)`

### src\AzeLib\Localization\LocStringTreeBuilder.cs
- COLD @ L12 — `typeof(Localization).GetMethod(`
- COLD @ L20 — `typeof(Localization).GetMethod(`

### src\BetterInfoCards\Converters\ConverterManager.cs
- COLD @ L157 — `var method = typeof(ConverterManager).GetMethod(nameof(AddConverter)).MakeGenericMethod(type);`

### src\BetterInfoCards\Export\ExportWidgets.cs
- COLD @ L45 — `static MethodBase TargetMethod()`

### src\BetterInfoCards\Info\InfoCardWidgets.cs
- COLD @ L58 — `var field = type.GetField(name, flags);`

### src\BetterInfoCards\Process\ModifyHits.cs
- COLD @ L19 — `static MethodBase TargetMethod()`

### src\BetterInfoCards\Tweaks\ChangeStatusItemOverlays.cs
- COLD @ L11 — `static MethodBase TargetMethod()`

### src\BetterInfoCards\Util\Options.cs
- COLD @ L75 — `var property = typeof(Options).GetProperty(nameof(InfoCardBackgroundColor), flags);`

### src\DevLoader\DevLoader\LiveLoader.cs
- COLD @ L230 — `methodInfo.Invoke(target, array);`

### src\DevLoader\DevLoader\LoaderFilterPatch.cs
- COLD @ L11 — `private static MethodBase TargetMethod()`

### src\DevLoader\DevLoader\ModLoadFilterPatch.cs
- COLD @ L11 — `private static MethodBase TargetMethod()`
