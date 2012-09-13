properties {
	# Update this manually for each new version
	$pack_version = "1.0.1.0"

	$base_dir = resolve-path .
	$lib_dir = join-path $base_dir "pack\lib"
	$nuget_tool = join-path $base_dir ".nuget\nuget.exe"

	# Previously, package versions and git-tags had to be in sync, but this is problematic in the long run
	# due to git's separate commits for branch and tags
	#$pack_version = (git describe --tags --abbrev=0).Replace("v", "")

	$pack_author = "Joakim Larsson"
	$pack_copyright = "Copyright © Joakim Larsson 2012"

	# specify projects 
	$prevalence = @{
		id="Kiwi.Prevalence"; 
		title="Kiwi.Prevalence"; 
		description = "Prevalence library for .NET";
		project_dir = (join-path $base_dir Kiwi.Prevalence);
		project = (join-path $base_dir Kiwi.Prevalence\Kiwi.Prevalence.csproj);
		nuspec = (join-path $base_dir Kiwi.Prevalence\Kiwi.Prevalence.Nuspec);
	}
	$projects = ($prevalence)

	# specify solutions to build
	$net40 = @{ sln = (join-path $base_dir "Kiwi.Prevalence.sln"); out = (join-path $base_dir "pack\lib\net40") }
	$solutions = ($net40)
}

#$framework = "4.0"
Framework("4.0")


task default -depends pack

task clean {
	remove-item -force -recurse $lib_dir -ErrorAction SilentlyContinue
}

task update-assembly-info -depends clean {
	$projects | foreach { (Generate-Assembly-Info $_) }
}

task build -depends update-assembly-info {
	$solutions | foreach { msbuild $_.sln /target:"Build" /verbosity:quiet /nologo /p:Platform="Any CPU" /p:Configuration="Release" /p:OutDir=$($_.out)\ }
}

task pack -depends build {
	$projects | foreach { `
		& $nuget_tool pack $_.nuspec -p "id=$($_.id);version=$pack_version;title=$($_.title);author=$pack_author;description=$($_.description);copyright=$pack_copyright;libdir=..\pack\lib"
	}
}


function Generate-Assembly-Info
{
	param($project)

	$asmInfo = "using System;
using System.Reflection;
using System.Runtime.InteropServices;
[assembly: CLSCompliantAttribute(true)]
[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyTitleAttribute(""$($project.title)"")]
[assembly: AssemblyDescriptionAttribute(""$($project.description)"")]
[assembly: AssemblyCompanyAttribute(""$pack_author"")]
[assembly: AssemblyProductAttribute(""$($project.id) $version"")]
[assembly: AssemblyCopyrightAttribute(""$pack_copyright"")]
[assembly: AssemblyVersionAttribute(""$pack_version"")]
[assembly: AssemblyInformationalVersionAttribute(""$pack_version"")]
[assembly: AssemblyFileVersionAttribute(""$pack_version"")]
[assembly: AssemblyDelaySignAttribute(false)]
"

	$file = join-path $project.project_dir "Properties\assemblyinfo.cs"
	Write-Host "Generating assembly info file: $file"
	Write-Output $asmInfo > $file
}
