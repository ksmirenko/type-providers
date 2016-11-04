This dummy project is used for debugging the type providers.

To debug:

1. Reference `TypeProviders` from this console application (already done)
2. In project properties (Visual Studio) of `TypeProviders`, in Debug tab, for Debug configuration set the following:
	* Start external program: `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe` or your custom path to `devenv.exe`, if different
	* Command line arguments: `"<absolute path to>\type-providers\TypeProviders\DebugHelper\DebugHelper.fsproj"`
	* Working directory:
`<absolute path to>\type-providers\TypeProviders\DebugHelper\`

Full instructions are located [here](http://apollo13cn.blogspot.ru/2012/12/debug-f-type-provider.html).
