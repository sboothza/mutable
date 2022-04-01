# Mutable

### Description

This package contains a mutable string implementation.

Built with .NET 5.0 - no other dependencies.

Basically, it allows direct modification of the object, which makes manipulation much faster.

It exposes a signature very similar to the System.String
It should be possible to drop in with minimal changes.

Version 1.0 - Not quite complete or fully tested yet.

Preliminary performance testing indicates +- 300% improvement when searching and manipulating

### Installation

Recommended to use nuget: `Install-Package Mutable`

Or build from source - Compatible with VS or Rider.

Command line: `dotnet build --configuration Release Mutable.csproj`

### Disclaimer

Go wild - use it, plagiarise, whatever.
No warranties of any kind are provided, including fitness for a particular purpose, blah, blah, blah.
