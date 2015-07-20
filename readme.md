# Json.Net Stubble Extensions

[![Build status](https://img.shields.io/appveyor/ci/Romanx/stubble-extensions-jsonnet.svg?style=flat-square)](https://ci.appveyor.com/project/Romanx/stubble-extensions-jsonnet)
[![Coverage Status](https://img.shields.io/coveralls/StubbleOrg/Stubble.Extensions.JsonNet.svg?style=flat-square)](https://coveralls.io/r/StubbleOrg/Stubble.Extensions.JsonNet)

This repository contains easy to use extensions for interacting with Newtonsoft Json.Net.

To use this just include it in your project by downloading the dll from the release section,
or preferably including it from Nuget.org through the badge above.

Example Usage:
```csharp
var builder = new StubbleBuilder().AddJsonNet().Build();
```

It's as simple as that, the package contains an Extension method for the StubbleBuilder adding
in the ValueGetters required to handle JTokens.
