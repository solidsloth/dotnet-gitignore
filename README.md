# dotnet-gitignore

This is a simple [dotnet tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) that [extends](https://docs.microsoft.com/en-us/dotnet/core/tools/extensibility) the dotnet cli to generate a .gitignore file for Visual Studio/.NET projects.

## Purpose

I like to use the dotnet cli to create projects while working with VSCode and it bugged me that I need to track down a .gitignore file every time I wanted to create a new project.

## What it Does

The tool downloads the official [Visual Studio .gitignore](https://github.com/github/gitignore/blob/master/VisualStudio.gitignore) from GitHub. It also caches a copy in your appdata folder in the event that you are offline.

## Install

To install use the dotnet cli tool install command:

```cmd
dotnet tool install -g dotnet-gitignore
```

This will install the tool globally. See [this page](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install) for more information.

## Usage

The tool installs as a dotnet cli extension and can be run using:

```cmd
dotnet gitignore
```

Currently the tool only accepts a single argument: the path where to .gitignore should be generated. If no path is specified, then the current working directory is used.

```cmd
dotnet gitignore ../
```

## Update

```cmd
dotnet tool update -g dotnet-gitignore
```

## Uninstall

```cmd
dotnet tool uninstall -g dotnet-gitignore
```
