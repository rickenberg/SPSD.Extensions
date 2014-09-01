###############################################################################
# SharePoint Solution Deployer (SPSD)
# Version          : 14.0.0.0
# Url              : 
# Creator          : Bernd Rickenberg
###############################################################################

# TODO: More description here...


#region Invoke-Add-SpFile
	# Desc: Uploads the specified file(s) to a SharePoint document library
    function Deploy($parameters, [System.Xml.XmlElement]$data, [string]$extId, [string]$extensionPath) {
        import-module $extensionPath\SPSD.Extensions.Client.dll

		# Add-SPFile
		$deployItems = $data.SelectNodes("//AddSPFileItem")
		foreach($item in $deployItems) {
			Log -message "Parameters:" -indent -type $SPSD.LogTypes.Normal
				Log -message ("SiteUrl: "+ $item.SiteUrl) -type $SPSD.LogTypes.Normal
				Log -message ("RelativeDocLibUrl: "+ $item.RelativeDocLibUrl) -type $SPSD.LogTypes.Normal
				Log -message ("SourcePath: "+ $item.SourcePath) -type $SPSD.LogTypes.Normal
				Log -message ("NoOverwrite: "+ $item.NoOverwrite) -type $SPSD.LogTypes.Normal
				Log -message ("AddUpdateOnly: "+ $item.AddUpdateOnly) -type $SPSD.LogTypes.Normal
			LogOutdent

			# implement your extension here
			$noOverwrite = [System.Convert]::ToBoolean($item.NoOverwrite)
			$addUpdateOnly = [System.Convert]::ToBoolean($item.AddUpdateOnly)
			$sourcePath = [System.IO.Path]::Combine($extensionPath, $item.SourcePath)

			Get-Item $sourcePath | Add-SPFile -SiteUrl $item.SiteUrl -RelativeDocLibUrl $item.RelativeDocLibUrl -NoOverwrite $noOverwrite -AddUpdateOnly $addUpdateOnly -Username $item.Username -Password $item.Password
		}

		# Add-SPWorkflowDefinition
		$deployItems = $data.SelectNodes("//AddSPWorkflowDefinitionItem")
		foreach($item in $deployItems) {
			Log -message "Parameters:" -indent -type $SPSD.LogTypes.Normal
				Log -message ("SiteUrl: "+ $item.SiteUrl) -type $SPSD.LogTypes.Normal
				Log -message ("SourcePath: "+ $item.SourcePath) -type $SPSD.LogTypes.Normal
				Log -message ("Id: "+ $item.Id) -type $SPSD.LogTypes.Normal
				Log -message ("DisplayName: "+ $item.DisplayName) -type $SPSD.LogTypes.Normal
				Log -message ("Publish: "+ $item.Publish) -type $SPSD.LogTypes.Normal
			LogOutdent

			# implement your extension here
			$publish = [System.Convert]::ToBoolean($item.Publish)
			$sourcePath = [System.IO.Path]::Combine($extensionPath, $item.SourcePath)

			Get-Item $sourcePath |`
			Add-SPWorkflowDefinition`
				-SiteUrl $item.SiteUrl`
				-Publish $publish`
				-DisplayName $item.DisplayName`
				-Id $item.Id`
				-RestrictToType $item.RestrictToType`
				-RestrictToListName $item.RestrictToListName
		}
    }
#endregion
