Param($projectFile, $buildNum)

# Modified from https://blog.johnnyreilly.com/2017/04/setting-build-version-using-appveyor.html

$content = [IO.File]::ReadAllText($projectFile)

$regex = new-object System.Text.RegularExpressions.Regex (
		'(([\d]+.[\d]+.[\d])(-[a-zA-Z\.\-0-9]*)?)<\/Version>', 
         [System.Text.RegularExpressions.RegexOptions]::MultiLine)

$version = $null
$match = $regex.Match($content)
echo $match
if(!$match.Success) { throw 'No version found' }

$version = $match.groups[1].value
echo "version = $version"

$numericPartOfVersion = $match.groups[2].value
echo "numericPartOfVersion = $numericPartOfVersion"

$fileVersion = "$numericPartOfVersion.$buildNum"
echo "fileVersion = $fileVersion"

if ([string]::IsNullOrEmpty($match.groups[3]))
{
	# found a release indicator
	# append buildNum and timestamp to the version as metadata
	$buildTimestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
	$version = "$version+build.$buildNum-$buildTimestamp"
	echo "release version detected: updated with buildNum to $version"
}
else
{
	# found a pre-release indicator
	# append buildNum to the pre-release portion of the version
	$version = "$version-build.$buildNum"
	echo "pre-release version detected: updated with buildNum to $version"
}

$content = $regex.Replace($content, "$version</Version><FileVersion>$fileVersion</FileVersion>")

# update csproj file
[IO.File]::WriteAllText($projectFile, $content)

echo "updating AppVeyor version"

# update AppVeyor build
Update-AppveyorBuild -Version $version
