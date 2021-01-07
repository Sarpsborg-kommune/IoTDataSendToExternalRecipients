# IoTDataSendToExternalRecipients

> :warning: The Azure function uses .NET Core 3.1. Azure functions is not implemented for .NET 5.
> Note that the libraries/packages (declared in the .csproj) file must use versions compatible
> with .NET Core 3.1

> :information*source: I've started to work under the assumption that code should be
> self-explanatory. The code should not contain things that is so \_clever* that an explanation is
> needed. You would probably not remember or understad what you have done after 1 year. Such code if
> used will be explained ;-)

## Explanation

This Azure function (library?) contains various (in the future?) Azure Functions that sends data to
various external recipients (as of Dec. 2020, this is only EnergyManager).
A function in this library is a HTTP trigger that is used by StreamAnalytics service.

## Installation

Eventually the function will be deployed from GGitHub directly to Azure. You must add the following
Azure Environment variables `IoTHubEndpointEM`.

The following procedure must be used to deploy the function(s):

```bash
# git pull https://github.com/Sarpsborg-kommune/IoTDataSendToExternalRecipients.git
# cd IoTDataSendToExternalRecipients
#  func azure functionapp publish {functioappname}
```
