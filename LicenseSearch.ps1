param(
    [parameter(mandatory=$true)] [string] $rootDir="",
    [parameter(mandatory=$true)] [string] $buildDir=""
)

if ($rootDir[$rootDir.Length-1] -eq "\")
{ $pacpagesRoot = $rootDir+"packages\" }
else
{$pacpagesRoot = "$rootDir\packages\"}

#cd $rootDir

if (Test-Path $pacpagesRoot)
{
    Write-Host "Looking for license files in nuget packages"    
    $items = Get-ChildItem -Path $pacpagesRoot -Filter "license.*" -Recurse
    foreach ($item in $items)
    {
        Write-Host "Found "$item.Name
        # smaz root
        $p = $item.Directory.FullName.ToLower().Replace($pacpagesRoot.ToLower(),"")
        $r = $p.Split('\')[0]
        $targetDir = [System.IO.Path]::Combine($buildDir,$r)
        if (!(Test-Path $targetDir))
        {
            New-Item $targetDir -ItemType Directory
        }
        Write-Host "Copying $($item.Name) to `"$targetDir`""
        Copy-Item -Path $item.FullName -Destination $targetDir
    }
}

Write-Host "License processiong done"







