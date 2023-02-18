# SolutionFilterBuilder

## A DotNet tool to create a solution filter form a Visual Studio solution, controlled by MSBuild project properties. 
[![Build](https://github.com/tom-englert/SolutionFilterBuilder/actions/workflows/build.yml/badge.svg)](https://github.com/tom-englert/SolutionFilterBuilder/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/SolutionFilterBuilder)](https://www.nuget.org/packages/SolutionFilterBuilder)

Easily create a solution filter to avoid loading more than necessary, and update your filter whenever your solution changes.

This tool uses MSBuild logic to control which projects should be part of your filter, giving you a high grade of flexibility.

All projects containing a property `IncludeInSolutionFilter` set to `true` are included in the specified filter, along with all their referenced projects.
A property `SolutionFilterName` is provided to be able to write MSBuild conditions based on the filter name.

You can mark individual projects by adding the `IncludeInSolutionFilter` property to the project file:
    
- Include the project and all references in any filter:
```xml
    <IncludeInSolutionFilter>true</IncludeInSolutionFilter>
```    

- Include the project only if the filter name is `Setup.slnf`:
```xml
    <IncludeInSolutionFilter Condition="'$(SolutionFilterName)'=='Setup'">true</IncludeInSolutionFilter>
```

You can include several projects by convention by adding conditional properties in the Directory.Build.targets file:
    
- Include all test projects in `Test.slnf`:
```xml
    <IncludeInSolutionFilter Condition="'$(IncludeInSolutionFilter)'=='' AND $(IsTestProject) AND '$(SolutionFilterName)'=='Test'">true</IncludeInSolutionFilter>
```
    
- Include all projects ending with `Something` in `Something.slnf`:
```xml
    <_IsSomeProject>$(MSBuildProjectName.ToUpperInvariant().EndsWith("SOMETHING"))</_IsSomeProject>
    <IncludeInSolutionFilter Condition="'$(IncludeInSolutionFilter)'=='' AND $(_IsSomeProject) AND '$(SolutionFilterName)'=='Something'">true</IncludeInSolutionFilter>
```
    
- Include all executables in `Apps.slnf`:
```xml
     <IncludeInSolutionFilter Condition="'$(IncludeInSolutionFilter)'=='' AND '$(OutputType)'=='Exe' AND '$(IsTestProject)'!='true' AND '$(SolutionFilterName)'=='Apps'">true</IncludeInSolutionFilter>
   ```


## Usage
```
build-slnf [options]
```
#### Options
```
  -i, --input  <input>              The path to the solution file that is the source for the filter.
                                    If no solution is specified, it tries to find a single one in the current directory.
  -o, --output <output> (REQUIRED)  The name of the solution filter that is created.
                                    An existing file will be overwritten without confirmation.
  --version                         Show version information
  -?, -h, --help                    Show help and usage information
```
#### Sample 
Build a solution filter `Test.sln` from the solution in the current directory.
```
build-slnf -o Test 
```





