Param($projectFile, $buildNum)

# Modified from https://blog.johnnyreilly.com/2017/04/setting-build-version-using-appveyor.html

$content = [IO.File]::ReadAllText($projectFile)

$regex = new-object System.Text.RegularExpressions.Regex (
		'([\d]+.[\d]+.[\d](-[a-zA-Z\.\-0-9]*)?)<\/Version>', 
         [System.Text.RegularExpressions.RegexOptions]::MultiLine)

$version = $null
$match = $regex.Match($content)
echo $match
if(!$match.Success) { throw 'No version found' }

$version = $match.groups[1].value
echo "version = $version"

if (![string]::IsNullOrEmpty($match.groups[2]))
{
	# found a pre-release indicator
	# insert "+[build_number]" before closing Version tag
	$version = "$version+build.$buildNum"
echo "version now $version"
	$content = $regex.Replace($content, "$version</Version>")

	# update csproj file
	[IO.File]::WriteAllText($projectFile, $content)
}

echo "updating AppVeyor version"

# update AppVeyor build
Update-AppveyorBuild -Version $version
