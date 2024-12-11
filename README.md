<a href="#">
    <img src="https://img.shields.io/badge/.NET%20Standard-2.1-blue" alt=".NET Standard 2.1">
</a>
<a href="./LICENSE">
    <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="MIT">
</a>
<a href="https://github.com/thorstenalpers/Finance.NET/actions/workflows/dotnet-build-and-test.yml">
    <img src="https://github.com/thorstenalpers/Finance.NET/actions/workflows/dotnet-build-and-test.yml/badge.svg" alt="Nightly Tests">
</a>
<a href="https://www.nuget.org/packages/Finance.NET">
    <img src="https://img.shields.io/nuget/v/Finance.NET.svg" alt="NuGet Version">
</a>
<a href="https://www.nuget.org/packages/Finance.NET">
    <img src="https://img.shields.io/nuget/dt/Finance.NET.svg" alt="NuGet Downloads">
</a>
<a href="https://github.com/thorstenalpers/Finance.NET" target="new">
    <img border="0" src="https://img.shields.io/github/stars/thorstenalpers/Finance.NET.svg?style=social&label=Star&maxAge=60" alt="Star this repo">
</a>


## Finance.NET

This is a .NET 2.1 library designed to interact with financial data from various APIs, including:

==> kostenlos als Hauptpunkt

* **Yahoo! Finance**: Retrieve stock, forex, and cryptocurrency data.
* **Xetra**: Access real-time stock market data from the Xetra exchange.
* **DataHub.io**: Integrate with open financial data sources for market insights.
* **Alpha Vantage**: Retrieve stock, forex, and cryptocurrency data.

#### Features:
* Yahoo! Financial
  * Fetch real-time financial data from multiple APIs.
  * Support for stock, forex, and crypto data.
  * Easy integration with .NET applications.
  * free
* Xetra.com
  * Fetch real-time financial data from multiple APIs.
  * Support for stock, forex, and crypto data.
  * Easy integration with .NET applications.
  * free
* Datahub.io
  * Fetch real-time financial data from multiple APIs.
  * Support for stock, forex, and crypto data.
  * Easy integration with .NET applications.
  * free
* Alpha Vantage
  * Fetch real-time financial data from multiple APIs.
  * Historische täglichen Kurse, Kurs-Datum ist nach Börsenschluss, (inkl. )
  * Support for stock, forex, and crypto data.
  * Requires an API key, [get it for free](https://www.alphavantage.co/)

#### How do I get started?

First, configure AutoMapper to know what types you want to map, in the startup of your application:

```csharp
var configuration = new MapperConfiguration(cfg => 
{
    cfg.CreateMap<Foo, FooDto>();
    cfg.CreateMap<Bar, BarDto>();
});
// only during development, validate your mappings; remove it before release
#if DEBUG
configuration.AssertConfigurationIsValid();
#endif
// use DI (http://docs.automapper.org/en/latest/Dependency-injection.html) or create the mapper yourself
var mapper = configuration.CreateMapper();
```
Then in your application code, execute the mappings:

```csharp
var fooDto = mapper.Map<FooDto>(foo);
var barDto = mapper.Map<BarDto>(bar);
```

Check out the [getting started guide](https://automapper.readthedocs.io/en/latest/Getting-started.html). When you're done there, the [wiki](https://automapper.readthedocs.io/en/latest/) goes in to the nitty-gritty details. If you have questions, you can post them to [Stack Overflow](https://stackoverflow.com/questions/tagged/automapper) or in our [Gitter](https://gitter.im/AutoMapper/AutoMapper).

### Where can I get it?

Install from the package manager console:

```
PM> Install-Package Finance.NET
```
Or from the .NET CLI as:
```
dotnet add package Finance.NET
```

### 🚀 Hire me

Do you have awesome software product ideas you would like to build? Do you need a great software engineer with over 10 years of experience on your team to help with different software projects? 
You are in the right place! As a team oriented software engineer, I can build variants of software products from the ground up. Front-end Engineer, Backend-Engineer, and DevOps Engineer.
See some of my works here to get an idea of how wide the scope I can cover. 
Please get in touch with me via the following email if you are interested in the software development contracting service I provide: ThorstenAlpers@web.de

