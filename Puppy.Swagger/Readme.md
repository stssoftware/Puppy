﻿![Logo](favicon.ico)
# Puppy.Swagger
> Project Created by [**Top Nguyen**](http://topnguyen.net)

- Support use `[Description]` attribute to make description for response object property.
- Support custom example for Request and Response
	+ [SwaggerRequestExample]: 
	[SwaggerRequestExample(typeof(TestModel), typeof(TestRequestModelExample))]. TestModelExample is class implement IExamplesProvider.
	```csharp
	public class TestRequestModelExample : IExamplesProvider
	{
		public object GetExamples()
		{
			return new TestModel
			{
				Lang = "en-GB",
				Currency = "GBP",
				Address = new AddressModel
				{
					Address1 = "1 Gwalior Road",
					Locality = "London",
					Country = "GB",
					PostalCode = "SW15 1NP"
				},
				Items = new[]
				{
					new ItemModel
					{
						ItemId = "ABCD",
						ItemType = ItemType.Product,
						Price = 20,
						Quantity = 1,
						RestrictedCountries = new[] { "US" }
					}
				}
			};
		}
	}
	```
	+ [SwaggerResponseExample]: Need use both [SwaggerResponse] and [SwaggerResponseExample], they use `StatusCodes` to map sample and response object type.
		* [SwaggerResponse(StatusCodes.Status200OK, typeof(TestResponseModel))]
		* [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TestResponseModelExample))].TestResponseModelExample is class implement IExamplesProvider.
	```csharp
	[SwaggerResponse(StatusCodes.Status200OK, Type = typeof(AccessTokenModel))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AccessTokenSample))]
	```
- See more [how to generate request and response example](https://github.com/mattfrear/Swashbuckle.AspNetCore.Examples)

# How to Setup

## Enable Project Documentation xml file.
![Project Property](Assets/ProjectProperty.png)
1. Right click on your Web API web project, select `Properties` or use can use shortcut `Alt + Enter`
2. Change to `Build` tab
3. Let select Configuration: `All Configurations` to make the setting apply for all configuration.
4. [Optional] you can forse all configuration run with `x64` bit (it better x86 or any CPU if your project always run on x64 machine) by select `x64` in `Platform Target` dropdown list.
5. [Optional] add `Suppress warnings` codes: `1701;1702;1705;1591`, if make project stop warning add `XML comment block` for all `Action` when you enable `Documentation` xml file.
6. Let enable `Documentation` xml by select checkbox `XML document file`, Visual Studio will auto add the path of xml file is `<Your Output Path>\<Your Name Space>.xml` but please change to `<Your Name Space>.xml`. You can change it if you want or just keep it to make `Puppy.Api Document` work well.
7. Try to build project and see your documentation xml file generated in the path config by above step, right click on the file and select `Property` then make the file `always copy` to out put folder (it help the deploy package always have descrption for all API) or you can do it in .csproj

**Other way is copy code below and put into your `.cspoj`**
```xml
    <!-- Generate Documentation XML -->

    <!-- Propert Group Config in Visual Studio < 15.3 -->
    <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
        <DocumentationFile><your aseembly name>.xml</DocumentationFile>
        <OutputPath>.\bin\</OutputPath>
        <NoWarn>1701;1702;1705;1591</NoWarn>
        <PlatformTarget>x64</PlatformTarget>
    </PropertyGroup>

    <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
        <PlatformTarget>x64</PlatformTarget>
        <OutputPath>.\bin\</OutputPath>
        <DocumentationFile><your aseembly name>.xml</DocumentationFile>
        <NoWarn>1701;1702;1705;1591</NoWarn>
    </PropertyGroup>

    <!-- Propert Group Config in Visual Studio >= 15.3 -->
    <PropertyGroup>
        <Configurations>Debug;Release</Configurations>
        <Platforms>AnyCPU;x86;x64</Platforms>
        <PlatformTarget>x64</PlatformTarget>
        <OutputPath>.\bin\</OutputPath>
        <DocumentationFile><your aseembly name>.xml</DocumentationFile>
        <NoWarn>1701;1702;1705;1591</NoWarn>
    </PropertyGroup>

    <!-- Copy Output folder -->
    <!--<CopyToOutputDirectory>Always/PreserveNewest</CopyToOutputDirectory>-->
    <ItemGroup>
        <!-- Documentation XML -->
        <Content Update="<your aseembly name>.xml">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </Content>
    </ItemGroup>
```

## Config
- Add config section to `appsettings.json`
- If you not have custom setting in appsettings.json, `default setting` will be apply.

```javascript
"ApiDocument": {
    // Api Document User Interface endpoint, start by "/". Default is "/developers"
    "ApiDocumentUiUrl": "/developers",

    // Json Viewer User Interface endpoint, start by "/". Default is "/developers/json-viewer"
    "JsonViewerUiUrl": "/developers/json-viewer",

    // Api Document Html Title: Name of API Document in HTML
    "ApiDocumentHtmlTitle": "API Document",

    // Access Key Query Param: name of parameter to check can access api document or not
    "AccessKeyQueryParam": "key",

    // Access Key: access key to check with AccessKeyQueryParam - empty is allow annonymous
    "AccessKey": "",

    // Un-authorize message when user access api document with not correct key.
    // Default is "You don't have permission to view API Document, please contact your administrator."
    "UnAuthorizeMessage": "You don't have permission to view API Document, please contact your administrator.",

    // Authenticate Token Prefix
    "AuthTokenKeyPrefix": "Bearer",

    // Api Document Url: relative URL with start and end by "/", be careful this may replace or be replace by MVC route
    "ApiDocumentUrl": "/.well-known/api-configuration/",

    // Api Document Name/Version: the path of document URL
    "ApiDocumentName": "latest",

    // Api Document Json File: the file name of json define api.
    "ApiDocumentJsonFile": "all.json",

    // => SO API JSON DOC Endpoint is "/.well-known/api-configuration/latest/all.json"

    // Api Contact Information
    "Contact": {
      "Name": "",
      "Url": "",
      "Email": ""
    },

    // Enable describe all enums as string - default is true
    "IsDescribeAllEnumsAsString": true,

    // Enable discribe all parameter in camel case - default is true
    "IsDescribeAllParametersInCamelCase": true
  }
```

---

## Add Service
- If you use default documentation file path (the xml file in output folder and same name with the assembly).
- Then use `services.AddApiDocument(<your assembly>, <ConfigurationRoot instance>, <config section key name in appsettings.json>)`
- The below is sample to use `Puppy.Api Document` with custom documentation xml file path.

```csharp
// [API Document] Api Document
services.AddApiDocument(typeof(Startup).GetTypeInfo().Assembly, ConfigurationRoot, "ApiDocument")

// With Puppy.Core
services.AddApiDocument(typeof(Startup).GetAssembly(), ConfigurationRoot, "ApiDocument");
```

- If you setup custom xml file path for your Documentation xml file, then use `services.AddApiDocument(<your  documentation xml absolute file path>, <ConfigurationRoot instance>, <config section key name in appsettings.json>)`
- The below is sample to use `Puppy.ApiDocument` with custom documentation xml file path.
```csharp
// [API Document] Api Document
services.AddApiDocument(Path.Combine(Directory.GetCurrentDirectory(), "Documentation.xml"), ConfigurationRoot, "ApiDocument");
```

- If you setup custom xml file path. Remember to add in your .csproj `always copy` for your xml file to make sure it in publish package when you `Publish`.
- This below is sample to config **copy always** to output folder.

```xml
<None Update="<your documentation file relative path>.xml">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

---

## Use Application Builder

```csharp
// [API Document] Api Document
app.UseApiDocument();
```