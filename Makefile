pkg_name := "Svelto.ECS"

nuget_pack:
	make nuget_clean
	mkdir temp temp/com.sebaslab.svelto.common temp/com.sebaslab.svelto.ecs temp/bin temp/bin/debug temp/bin/release
	dotnet new sln -n ${pkg_name} -o temp/
	cp -r Svelto.Common/* temp/com.sebaslab.svelto.common
	cp -r Svelto.ECS/* temp/com.sebaslab.svelto.ecs
	dotnet sln temp/${pkg_name}.sln add temp/com.sebaslab.svelto.common/Svelto.Common.csproj
	dotnet sln temp/${pkg_name}.sln add temp/com.sebaslab.svelto.ecs/Svelto.ECS.csproj
	# Build for Debug
	dotnet pack /p:Version=1.0.0 -o temp/bin/debug temp/com.sebaslab.svelto.ecs/Svelto.ECS.csproj -c Debug
	unzip temp/bin/debug/Svelto.ECS.1.0.0.nupkg -d temp/bin/debug
	cp temp/bin/debug/lib/netstandard2.0/Svelto.ECS.dll temp/bin/debug
	# Build for Release
	dotnet pack /p:Version=1.0.0 -o temp/bin/release temp/com.sebaslab.svelto.ecs/Svelto.ECS.csproj -c Release
	unzip temp/bin/release/Svelto.ECS.1.0.0.nupkg -d temp/bin/release
	cp temp/bin/release/lib/netstandard2.0/Svelto.ECS.dll temp/bin/release
	# Compile into nuget
	dotnet pack /p:PackageVersion=2.0.0 -o . temp/com.sebaslab.svelto.ecs/Svelto.ECS.csproj -c Custom
	make nuget_clean

nuget_clean:
	rm -rf upm-preparator temp