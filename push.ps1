.\nuget.exe update -self
$dir = "C:\test_code"
$file = dir *.nupkg |
  Sort-Object { 
        [Version] $(if ($_.BaseName -match "(\d+.){3}\d+") { 
                        $matches[0]
                    } 
                    else { 
                        "0.0.0.0"
                    })  
    } | select -last 1 -ExpandProperty Name

./nuget.exe push -Verbosity detailed -source nuget.org $file
