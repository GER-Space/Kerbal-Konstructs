MODNAME		:= KerbalKonstructs
KSPDIR		:= ${HOME}/ksp/KSP_linux
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA  := ${GAMEDATA}/${MODNAME}

UI_IMAGES := \
	ui_background.png	\
	ui_button.png		\
	ui_checkmark.png	\
	ui_dropdown.png		\
	ui_leftturn.png		\
	ui_rightturn.png	\
	ui_tabdesel.png		\
	ui_tabempty.png		\
	ui_tabsel.png		\
	ui_toggle_off.png	\
	ui_toggle_on.png	\
	$e

UI_CFG := \
	DefaultSkin.cfg		\
	$e

all: ${ICONS} ${FLAGS} ${MISC} ${UI_IMAGES}

info:
	@echo "Extraplanetary Launchpads Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

ui_background.png: ui_background.svg
	inkscape --export-width=32 --export-height=32 --export-type=png -o $@ $^

ui_button.png: ui_button.svg
	inkscape --export-width=16 --export-height=16 --export-type=png -o $@ $^

ui_checkmark.png: ui_checkmark.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_dropdown.png: ui_dropdown.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_leftturn.png: ui_leftturn.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_rightturn.png: ui_rightturn.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabdesel.png: ui_tabdesel.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabempty.png: ui_tabempty.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabsel.png: ui_tabsel.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_toggle_off.png: ui_toggle_off.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_toggle_on.png: ui_toggle_on.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

clean:
	rm -f ${ICONS} ${FLAGS} ${MISC} ${GUI_ICONS} ${UI_IMAGES}

install: all
	mkdir -p ${MODGAMEDATA}/UI
	cp ${UI_CFG} ${UI_IMAGES} ${MODGAMEDATA}/UI

.PHONY: all clean install
