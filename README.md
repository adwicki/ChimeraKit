# ChimeraKit
I've often found myself in a situation where I have to solve a small-ish problem. First, the solution
might be a simple powershell script, which eventually becomes too large and is then transformed into 
a .NET application.

This has resulted in quite a few (potentially super messy) projects which all _kinda_ work but all 
apply different ways to solve similar issues such as cli parsing. Naturally, this has also resulted
in copying code from previous projects.

ChimeraKit is an approach to bring order to this mess by providing a framework that hosts various
independent modules. It allows to quickly add new modules that already support the bare
minimum required setup such as cli parsing and module-specific configuration.

## Adding a Module to the Kit
A new module can be initialized by running:

```powershell
.\createNewModule.ps1 -ModuleName <NewModule>
```

The following optional parameters are available:
- `-Description` initializes the module description
- `-SkipBuild` prevents a build from being triggered automatically

In order to properly work with the Kit, modules should be defined in the namespace `ChimeraKit.Module.*`.

## Overall Configuration
ChimeraKit can be configured via the `appsettings.json` files in `.\ChimeraKit.Host`. The 
environment variable `CHIMERAKIT_ENV` allows to use different configurations, depending on the 
environment.

The following configurations are shared between all modules or required for the `ChimeraKit.Host`
application:

```json
{
  "Core": {
    "ModuleRoot": "modules\\",
    "AvailableModules": [
      {
        "ModulePath": "ChimeraKit.Module.ExamplePrepend",
        "ModuleName": "ChimeraKit.Module.ExamplePrepend"
      }
    ]
  }
}
```

### ModuleRoot and AvailableModules
This configuration section specifies the available modules for the kit. The `ModuleRoot` points to 
the directory, where the module dlls (and their dependencies) are stored. The solution is configured 
such that building it with the `Release` configuration will put the host application in the 
`/<root>/release/` directory and all modules in the subdirectory `/<root>/release/modules/`. Also, 
creating a module with the `.\createNewModule.ps1` script, will automatically update the configuration
to include the new module.

## Running a Module
A module can be run by invoking the `ChimeraKit.Host.exe` and passing the `ModuleName` and (potential) cli 
arguments expected by the module. For example:

```powershell
.\ChimeraKit.Host.exe ExamplePrepend -i input -p prefix
```

Running the host application without any module name will show you a list of available modules.

## Module: ExamplePrepend
This is an example module which demonstrates the usage of ChimeraKit. It defines a simple 
_module only_ service `IExamplePrependService`, which itself makes use of the shared 
`IExampleCapitalizationService`.

### Configuration
A module can have its own configuration which is defined in a configuration section in the 
`appsettings.json` file of `ChimeraKit.Host`:

```json
{
  "ExamplePrepend": {
    "SeparationCharacter": "_"
  }
}
```

### Running
Run the module as follows:

```powershell
# long param names
.\ChimeraKit.Host.exe ExamplePrepend --Input input --Prefix longer_
# short param names
.\ChimeraKit.Host.exe ExamplePrepend -i input -p longer_
```
