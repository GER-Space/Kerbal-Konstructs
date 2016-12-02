#/usr/bin/bash
#
#

cd /c/Git/Kerbal-Konstructs

VERSION=`grep -i AssemblyFileVersion src/Properties/AssemblyInfo.cs  | cut -d "\"" -f 2`


FILENAME="Kerbal-Konstructs-${VERSION}.zip"

if test -e $FILENAME ; then 
	rm -f  Kerbal-Konstructs-${VERSION}.zip
fi

7z a -r  $FILENAME GameData


