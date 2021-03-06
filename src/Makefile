MODNAME		:= KerbalKonstructs
KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA  := ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MODGAMEDATA}
CPLC_DIR	:= ${GAMEDATA}/CustomPreLaunchChecks

TARGETS		:= bin/${MODNAME}.dll

RESGEN2		:= resgen2
CSC 		:= csc
#CSCFLAGS	:= -optimize -warnaserror
CSCFLAGS	:= -highentropyva- -noconfig -nostdlib+ -t:library -optimize -warnaserror -debug -unsafe
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: ${TARGETS}

info:
	@echo "${MODNAME} Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    csc:        ${CSC}"
	@echo "    csc flags:  ${CSCFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

MOD_FILES := \
	Addons/Addons.cs \
	Addons/NavUtils/ILSConfig.cs \
	Addons/RemoteTech/RemoteTechAddon.cs \
	Addons/SDTest.cs \
	Addons/StageRecovery/StageRecovery.cs \
	Addons/StageRecovery/StageRecoveryWrapper.cs \
	Core/Config/ConfigParser.cs \
	Core/Config/ConfigUtil.cs \
	Core/Config/KKCustomParameters.cs \
	Core/KKMath.cs \
	Core/LaunchSites/LaunchSite.cs \
	Core/LaunchSites/LaunchSiteChecks.cs \
	Core/LaunchSites/LaunchSiteManager.cs \
	Core/LaunchSites/LaunchSiteParser.cs \
	Core/Log.cs \
	Core/MapDecals/DecalsDatabase.cs \
	Core/MapDecals/MapDecalInstance.cs \
	Core/MapDecals/MapDecalUtils.cs \
	Core/MapDecals/MapDecalsMap.cs \
	Core/SpaceCenters/CustomSpaceCenter.cs \
	Core/SpaceCenters/ScExtention.cs \
	Core/SpaceCenters/SpaceCenterManager.cs \
	Core/SquadStatics.cs \
	Core/StaticGroup/BuiltinCenters.cs \
	Core/StaticGroup/GroupCenter.cs \
	Core/StaticModels/ModelVariant.cs \
	Core/StaticModels/StaticModel.cs \
	Core/StaticObjects/InstanceUtil.cs \
	Core/StaticObjects/StaticDatabase.cs \
	Core/StaticObjects/StaticInstance.cs \
	Core/StaticObjects/StaticModule.cs \
	Core/StaticObjects/StaticModules/AdvTextures/AdvTextures.cs \
	Core/StaticObjects/StaticModules/AnimateOnSunRise/AnimateOnSunRise.cs \
	Core/StaticObjects/StaticModules/AnimateOnSunRise/AnimateOnSunRiseOld.cs \
	Core/StaticObjects/StaticModules/AnimateOnSunRise/OnSunRiseMaster.cs \
	Core/StaticObjects/StaticModules/AnimateOnTrigger/AnimateOnTrigger.cs \
	Core/StaticObjects/StaticModules/AudioPlayer/AudioPlayer.cs \
	Core/StaticObjects/StaticModules/AutoKourse/AutoKourse.cs \
	Core/StaticObjects/StaticModules/CallBack/KKCallBack.cs \
	Core/StaticObjects/StaticModules/CallBack/KKCallBackWorker.cs \
	Core/StaticObjects/StaticModules/Compat/AnimateOnClick.cs \
	Core/StaticObjects/StaticModules/Destructable/Destructable.cs \
	Core/StaticObjects/StaticModules/DishAnimate/DishAnimate.cs \
	Core/StaticObjects/StaticModules/FlagDecal/FlagDecal.cs \
	Core/StaticObjects/StaticModules/GrassColor/GrasColor.cs \
	Core/StaticObjects/StaticModules/GrassColor/GrassColor2.cs \
	Core/StaticObjects/StaticModules/GrassColor/GrassColorUtils.cs \
	Core/StaticObjects/StaticModules/GrassColor/TexturePreset.cs \
	Core/StaticObjects/StaticModules/OnClick/AnimateOnClick.cs \
	Core/StaticObjects/StaticModules/OnClick/GUIOnClick.cs \
	Core/StaticObjects/StaticModules/PadSmoke/PadSmoke.cs \
	Core/StaticObjects/StaticModules/RunwayPapi/RunwayPapi.cs \
	Core/StaticObjects/StaticModules/TileTextures/TileTextures.cs \
	Core/StaticObjects/StaticModules/Water/WaterSurface.cs \
	Editor/GroupEditor/GroupEditor.cs \
	Editor/GroupEditor/GroupItem.cs \
	Editor/GroupEditor/GroupItemView.cs \
	Editor/GroupEditor/GroupSelectorUI.cs \
	Editor/GroupEditor/GroupView.cs \
	Editor/InstanceEditor/EditorGUI.cs \
	Editor/InstanceEditor/FacilityEditor.cs \
	Editor/InstanceEditor/GrassColorPresetUI.cs \
	Editor/InstanceEditor/GrassColorPresetUI2.cs \
	Editor/InstanceEditor/GrassColorUI.cs \
	Editor/InstanceEditor/InstanceView.cs \
	Editor/InstanceEditor/InstanceItem.cs \
	Editor/InstanceEditor/InstanceItemView.cs \
	Editor/InstanceEditor/LaunchSiteEditor.cs \
	Editor/InstanceEditor/VariantSelector.cs \
	Editor/MapDecalEditor/MapDecalEditor.cs \
	Editor/MapDecalEditor/MapDecalItem.cs \
	Editor/MapDecalEditor/MapDecalItemView.cs \
	Editor/MapDecalEditor/MapDecalView.cs \
	Editor/Shared/DrawObject.cs \
	Editor/Shared/DrawTools.cs \
	Editor/Shared/EditorGizmo.cs \
	Editor/Shared/VectorRenderer.cs \
	Editor/KKEditorLogic.cs \
	Editor/ModelInfo.cs \
	Editor/ModelItem.cs \
	Editor/ModelItemView.cs \
	Editor/SpawnView.cs \
	Editor/StaticsEditorGUI.cs \
	ExternalAPI/API.cs \
	ExternalAPI/extLaunchSiteManager.cs \
	Facilities/Barracks/Barracks.cs \
	Facilities/Business/Business.cs \
	Facilities/FacilityManager.cs \
	Facilities/FuelTanks/FuelTanks.cs \
	Facilities/GroundStation/GroundStation.cs \
	Facilities/GroundStation/TrackingStationGUI.cs \
	Facilities/Hangar/Hangar.cs \
	Facilities/Hangar/HangarGUI.cs \
	Facilities/Hangar/HangarKSCGUI.cs \
	Facilities/KKFacility.cs \
	Facilities/KKFacilitySelector.cs \
	Facilities/LandingGuide/LandingGuideClasses.cs \
	Facilities/Merchant/Merchant.cs \
	Facilities/Merchant/MerchantGUI.cs \
	Facilities/Merchant/MerchantItem.cs \
	Facilities/Merchant/MerchantItemView.cs \
	Facilities/IBarracks.cs \
	Facilities/IProduction.cs \
	Facilities/ProductionGUI.cs \
	Facilities/RecoveryBase/RecoveryBase.cs \
	Facilities/Research/Research.cs \
	Facilities/StaffGUI.cs \
	Facilities/StaffItem.cs \
	Facilities/StaffItemView.cs \
	Facilities/Storage/Storage.cs \
	Facilities/Storage/StorageGUI.cs \
	Facilities/Storage/StorageItem.cs \
	Facilities/Storage/StorageItemView.cs \
	KerbalKonstructs.cs \
	Modules/Career/CareerEditor.cs \
	Modules/Career/CareerObjects.cs \
	Modules/Career/CareerScenario.cs \
	Modules/Career/CareerState.cs \
	Modules/Career/CareerUtils.cs \
	Modules/MapIcons/MapIcon.cs \
	Modules/MapIcons/MapIconDraw.cs \
	Modules/MapIcons/MapIconManager.cs \
	Modules/MapIcons/MapIconSelector.cs \
	Modules/RemoteNet/ConnectionManager.cs \
	Modules/RemoteNet/KKCommNetHome.cs \
	Properties/AssemblyInfo.cs \
	ResourceManager/IResourceContainer.cs \
	ResourceManager/PartResourceContainer.cs \
	ResourceManager/ResourceInfo.cs \
	ResourceManager/ResourceManager.cs \
	ResourceManager/ResourceSet.cs \
	ResourceManager/ResourceSetContainer.cs \
	ResourceManager/ResourceXferControl.cs \
	UI/AirRacing.cs \
	UI/BaseBossFlight.cs \
	UI/BaseManager.cs \
	UI/FacilityItem.cs \
	UI/FacilityItemView.cs \
	UI/FieldToggle.cs \
	UI/IconToggle.cs \
	UI/IncrementSize.cs \
	UI/InfoLine.cs \
	UI/InputLine.cs \
	UI/KKWindow.cs \
	UI/LandingGuide.cs \
	UI/LaunchsiteCategoryIcon.cs \
	UI/LaunchsiteFilter.cs \
	UI/LaunchsiteItem.cs \
	UI/LaunchsiteItemView.cs \
	UI/LaunchsiteSelectorGUI.cs \
	UI/Layout.cs \
	UI/ListView.cs \
	UI/Localization.cs \
	UI/MiniToggle.cs \
	UI/NavGuidanceSystem.cs \
	UI/PositionButtons.cs \
	UI/PositionEdit.cs \
	UI/PositionLine.cs \
	UI/ReferenceSystem.cs \
	UI/StackSize.cs \
	UI/StateButton.cs \
	UI/ToggleText.cs \
	UI/ToolbarController.cs \
	UI/TransferButtons.cs \
	UI/UIMain.cs \
	UI/ValueAdjuster.cs \
	UI/VectorDisplay.cs \
	UI/WindowManager.cs \
	UI2/Editor/EditorModeSelector.cs \
	UI2/Editor/Group/GroupEditorToolbar.cs \
	UI2/Editor/Instance/ColorSelector.cs \
	UI2/Editor/Instance/GrassEditor.cs \
	UI2/Editor/Instance/InstanceEditorToolbar.cs \
	UI2/Editor/Instance/ModelList.cs \
	UI2/Editor/Terrain/TerrainEditorToolbar.cs \
	UI2/KKStyle.cs \
	UI2/KKWindow2.cs \
	UI2/KSCManager.cs \
	UI2/WindowManager2.cs \
	Utilities/CameraController.cs \
	Utilities/ClassExtentions.cs \
	Utilities/DebugDrawer.cs \
	Utilities/Enum.cs \
	Utilities/GrasColorCam.cs \
	Utilities/KKGraphics.cs \
	Utilities/MiscUtils.cs \
	Utilities/Zip/Crc32Helper.Managed.cs \
	Utilities/Zip/PositionPreservingWriteOnlyStreamWrapper.cs \
	Utilities/Zip/ZipArchive.cs \
	Utilities/Zip/ZipArchiveEntry.Unity.cs \
	Utilities/Zip/ZipArchiveEntry.cs \
	Utilities/Zip/ZipArchiveMode.cs \
	Utilities/Zip/ZipBlocks.cs \
	Utilities/Zip/ZipCustomStreams.cs \
	Utilities/Zip/ZipFile.cs \
	Utilities/Zip/ZipFileExtensions.cs \
	Utilities/Zip/ZipHelper.cs \
	Utilities/Zip/ZipVersion.cs \
	$e

include KodeUI/KodeUI.inc

SYSTEM := \
	-lib:${MANAGED} \
	-r:${MANAGED}/mscorlib.dll \
	-r:${MANAGED}/System.dll \
	-r:${MANAGED}/System.Core.dll

KSP := \
	-r:Assembly-CSharp.dll \
	-r:Assembly-CSharp-firstpass.dll

UNITY := \
	-r:UnityEngine.dll \
	-r:UnityEngine.UI.dll \
	-r:UnityEngine.CoreModule.dll \
	-r:UnityEngine.PhysicsModule.dll \
	-r:UnityEngine.Physics2DModule.dll \
	-r:UnityEngine.IMGUIModule.dll \
	-r:UnityEngine.AnimationModule.dll \
	-r:UnityEngine.AssetBundleModule.dll \
	-r:UnityEngine.TextRenderingModule.dll \
	-r:UnityEngine.InputLegacyModule.dll \
	-r:UnityEngine.AudioModule.dll \
	-r:UnityEngine.ParticleSystemModule.dll \
	-r:UnityEngine.UIModule.dll \
	-r:UnityEngine.ImageConversionModule.dll \
	$e

CPLC := \
	-lib:${CPLC_DIR} \
	-r:CustomPreLaunchChecks.dll

bin/${MODNAME}.dll: ${MOD_FILES} ${KodeUI}
	@mkdir -p bin
	${CSC} ${CSCFLAGS} ${SYSTEM} ${KSP} ${UNITY} ${CPLC} -out:$@ $^

clean:
	rm -f ${TARGETS} bin/*.pdb
	test -d bin && rmdir bin || true

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}

.PHONY: all clean install
