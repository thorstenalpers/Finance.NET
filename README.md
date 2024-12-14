
<a href="https://github.com/thorstenalpers/Finance.NET" target="_blank">
  <img src="./src/banner.png" width="400"> 
</a>

---


<a href="#">
    <img src="https://img.shields.io/badge/.NET%20Standard-2.1-blue" alt=".NET Standard 2.1">
</a>
<a href="./LICENSE">
    <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="MIT">
</a>
<a href="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-ci.yml">
    <img src="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-ci.yml/badge.svg" alt="Tests">
</a>
<a href="https://coveralls.io/github/thorstenalpers/Finance.NET?branch=develop">
    <img src="https://coveralls.io/repos/github/thorstenalpers/Finance.NET/badge.svg?branch=develop" alt="NuGet Version">
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

<br />

Finance .NET is designed to interact with financial data from various APIs, including:

==> kostenlos als Hauptpunkt

* **Yahoo! Finance**: Retrieve stock, forex, and cryptocurrency data.
* **Xetra**: Access real-time stock market data from the Xetra exchange.
* **DataHub.io**: Integrate with open financial data sources for market insights.
* **Alpha Vantage**: Retrieve stock, forex, and cryptocurrency data.


## Test Status

<table style="font-size: small;">
  <thead>
    <tr>
      <th>Provider</th>
      <th>Interface</th>
      <th style="text-align: left;">Status</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Alpha Vantage</td>
      <td>IAlphaVantageService</td>
      <td style="text-align: left;">
        <a href="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-alphavantage.yml">
          <img src="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-alphavantage.yml/badge.svg" >
        </a>
      </td>
    </tr>   
    <tr>
      <td>Datahub.io</td>
      <td>IDatahubIoService</td>
      <td style="text-align: left;">
        <a href="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-datahubio.yml">
          <img src="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-datahubio.yml/badge.svg" >
        </a>
      </td>
    </tr>
    <tr>
      <td>Yahoo! Finance</td>
      <td>IYahooFinanceService</td>
      <td style="text-align: left;">
        <a href="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-yahoo.yml">
          <img src="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-yahoo.yml/badge.svg" >
        </a>
      </td>
    </tr>
    <tr>
      <td>Xetra</td>
      <td>IXetraService</td>
      <td style="text-align: left;">
        <a href="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-xetra.yml">
          <img src="https://github.com/thorstenalpers/Finance.NET/actions/workflows/tests-xetra.yml/badge.svg" >
        </a>
      </td>
    </tr>   
  </tbody>
</table>

#### Features
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

#### Let's get started

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
// use DI (https://docs.automapper.org/en/latest/Dependency-injection.html) or create the mapper yourself
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



## Table of Contents

1. [Documentation](https://github.com/cake-build/cake#documentation)
2. [Contributing](https://github.com/cake-build/cake#contributing)
3. [Get in touch](https://github.com/cake-build/cake#get-in-touch)
4. [License](https://github.com/cake-build/cake#license)


## Code of Conduct

Thinking about contributing to Finance.NET? That's awesome! Your effort is truly appreciated. Here’s how you can get started:

* Fork the repository.
* Create a dedicated branch for your work.
* Implement your feature or fix the bug.
* Remember to include unit tests.
* Submit a pull request.
