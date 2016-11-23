This project is used for debugging and testing the type providers.

To debug, in project properties (Visual Studio) of TypeProviders, in Debug tab, for Debug configuration set the following:
	* Start external program: `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe` or your custom path to `devenv.exe`, if different
	* Command line arguments: `"<absolute path to>\type-providers\Testfield\Testfield\Testfield.fsproj"`
	* Working directory: `<absolute path to>\type-providers\Testfield\Testfield\`

After that, when you press F5 (Debug) from TypeProviders project, VS will automatically build the type providers, start a new instance of VS, attach to a new VS process and open Testfield in it.

Full instructions are located [here](https://sergeytihon.wordpress.com/tag/type-providers/) (Tip #5).
