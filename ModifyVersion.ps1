Param($projectFile, $buildNum)

# Modified from https://blog.johnnyreilly.com/2017/04/setting-build-version-using-appveyor.html

$content = [IO.File]::ReadAllText($projectFile)

$regex = new-object System.Text.RegularExpressions.Regex ('()([\d]+.[\d]+.[\d]+)(.[\d]+)((-[a-zA-Z\.\-0-9]*)?<\/Version>)', 
         [System.Text.RegularExpressions.RegexOptions]::MultiLine)

$version = $null
$match = $regex.Match($content)
echo $match
if($match.Success) {
    # from "1.0.0.0" this will extract "1.0.0"
    $version = $match.groups[1].value
}

# suffix build number onto $version. eg "1.0.0.15"
$version = "$version.$buildNum"

# update "1.0.0.0" to "$version"
$content = $regex.Replace($content, '${2}' + $version + '${4}')

# update csproj file
[IO.File]::WriteAllText($projectFile, $content)

# update AppVeyor build
Update-AppveyorBuild -Version $version