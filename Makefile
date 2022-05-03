pkg_name := "Svelto.ECS"

nuget_pack:
	make nuget_clean
	mkdir temp temp/bin temp/bin/debug temp/bin/release

	# Build for Debug
	dotnet pack /p:PackageVersion=1.0.0 -o temp/bin/debug com.sebaslab.svelto.ecs/Svelto.ECS.csproj -c Debug
	unzip "temp/bin/debug/${pkg_name}.1.0.0.nupkg" -d temp/bin/debug
	cp temp/bin/debug/lib/netstandard2.0/Svelto.ECS.dll temp/bin/debug

	# Build for Release
	dotnet pack /p:PackageVersion=1.0.0 -o temp/bin/release com.sebaslab.svelto.ecs/Svelto.ECS.csproj -c Release
	unzip "temp/bin/release/${pkg_name}.1.0.0.nupkg" -d temp/bin/release
	cp temp/bin/release/lib/netstandard2.0/Svelto.ECS.dll temp/bin/release

	# Compile into nuget
	dotnet pack /p:PackageVersion=2.0.0 -o . com.sebaslab.svelto.ecs/Svelto.ECS.csproj -c NugetPack
	# make nuget_clean

nuget_clean:
	rm -rf temp