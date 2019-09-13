## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.

mkdir c:\winsdktemp

$client = new-object System.Net.WebClient
$client.DownloadFile("https://go.microsoft.com/fwlink/p/?linkid=870807","c:\winsdktemp\winsdksetup.exe")

Start-Process -Wait "c:\winsdktemp\winsdksetup.exe" "/features OptionId.UWPCpp /q"