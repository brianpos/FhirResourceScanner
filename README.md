# FhirResourceScanner

## Overview

## Parameters

| Parameter | Description |
| --- | --- |
| -u, --user | Required. This name will be listed in the Test Report output |
| -f, --file | A filename to an xml/json file to load |
| --url | The fhir query URL to execute to load the bundle/resource to process |
| -a, --auth | A value to populate into the Authorization header of a fhir server (via the --url parameter) when connecting to extract resource content |
| -v, --ver | (Default: STU3) This can be either DSTU2, STU3 or R4 |
| -r, --report | (Default: http://sqlonfhir-stu3.azurewebsites.net/fhir) Publish the results to this server location (use 'none' if you don't want to publish the results) |
| --help | Display the help screen |
| --version | Display version information |

## Usage

Example:
  > dotnet.exe FhirResourceScanner.dll --file c:\temp\example-bundle.xml --user "Brian Postlethwaite"
  > dotnet.exe FhirResourceScanner.dll --file c:\temp\example-bundle.json --user David
  > dotnet.exe FhirResourceScanner.dll --report none --user "Brian Postlethwaite" --url http://sqlonfhir-stu3.azurewebsites.net/fhir/Encounter
  > dotnet.exe FhirResourceScanner.dll --auth "Bearer asfdasdfsf==" --user "Brian Postlethwaite" --url http://sqlonfhir-stu3.azurewebsites.net/fhir/Patient?active=true
