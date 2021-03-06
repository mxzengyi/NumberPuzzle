#!/bin/sh
 
#工程名#
projectname=LuckySix 
 
 
pth=`pwd`
#UNITY程序的路径#
SDK=iphoneos
UNITY_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity

echo "CompileEnv:"
echo $pth
echo $SDK
echo $UNITY_PATH
echo $ScriptingDefineSymbols 
 
rm -rf result/*.ipa
mkdir -p result
mkdir -p log
 
 
#将unity导出成xcode工程#
$UNITY_PATH -batchmode -projectPath $pth -executeMethod ProjectBuild.BuildForIPhone -scriptingdefinesymbols $ScriptingDefineSymbols -quit -logFile $pth/log/ios.log
echo "End Build Unity to XCodeProject"

xcodeprojpath=$pth/$projectname

echo "Start Build XCodeProject"
#开始生成ipa#
cd $xcodeprojpath
xcodebuild -sdk $SDK clean
xcodebuild -sdk $SDK


echo "compile success!!!!!!!!!!!"


xcrun -sdk $SDK PackageApplication -v $xcodeprojpath/build/Release-iphoneos/*.app -o $pth/result/$projectname.ipa
 
echo "ipa生成完毕"