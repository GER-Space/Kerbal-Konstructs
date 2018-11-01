#/usr/bin/bash
#
#

cd /c/Git/Kerbal-Konstructs

VERSION=`grep -i AssemblyFileVersion src/Properties/AssemblyInfo.cs  | cut -d "\"" -f 2`


MAJOR=`echo ${VERSION} | cut -d "." -f 1 `
MINOR=`echo ${VERSION} | cut -d "." -f 2 `
PATCH=`echo ${VERSION} | cut -d "." -f 3 `
BUILD=`echo ${VERSION} | cut -d "." -f 4 `


if (test $1 != "") ; then
  
VERSION=${VERSION}_$1
fi

FILENAME="Kerbal-Konstructs-${VERSION}.zip"



if test -e $FILENAME ; then 
	rm -f  Kerbal-Konstructs-${VERSION}.zip
fi


sed -i "s/MAJOR\":./MAJOR\":$MAJOR/" GameData/KerbalKonstructs/KerbalKonstructs.version
sed -i "s/MINOR\":./MINOR\":$MINOR/" GameData/KerbalKonstructs/KerbalKonstructs.version
sed -i "s/PATCH\":./PATCH\":$PATCH/" GameData/KerbalKonstructs/KerbalKonstructs.version
sed -i "s/BUILD\":.+/BUILD\":$BUILD/" GameData/KerbalKonstructs/KerbalKonstructs.version


/c/Program\ Files/7-Zip/7z.exe a -r  $FILENAME GameData


