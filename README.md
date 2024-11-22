# Miranda Optikey Plugin

A plugin that allows the output from the [Miranda calibration tool](https://codeberg.org/eyes-on-disabilities/miranda-eye-tracking-screen-calibrator) to be used with [Optikey](https://github.com/Optikey/Optikey/).
This project is a fork from https://github.com/kmcnaught/SmoothMousePlugin.

## Dependencies

The project depends on:
- System libraries: `System.Reactive.Linq` and `WindowsBase`
- Optikey's provided DLLs: `JuliusSweetland.OptiKey.Contracts` and `PointSourceUtils` which gives us `Time.HighResolutionUtcNow` from `JuliusSweetland.Optikey.Static`. The latter library also contains graphics-related utilities such as `DipScalingFactorX` and `PrimaryScreenHeightInPixels` which may be useful for eye-trackers that report their positions relative to screen dimensions. 

## Building this repo

-  Clone the repo from github:
`git clone https://github.com/eyes-on-disabilities/miranda-optikey-plugin.git`
- Open `MirandaOptikey.sln` in Visual Studio
- Load the libraries `System.Reactive.Linq` and `WindowsBase` (run `NuGet\Install-Package Rx-Linq -Version 2.2.5` in the NuGet CLI)
- Build for x64

## Testing the resulting plugin locally

Put the entirety of the `Release` folder into `%APPDATA%\Optikey\OptiKey\EyeTrackerPlugins`. You can name the folder anything you want. 

Now in Optikey's Management console, the plugin will appear in the list of points sources as well as in the Plugin Search window.

## Publishing your own plugin 

- Fork this repo
- Replace the implementation with your own eye tracker integration
- After compiling your code, zip up the contents of your `Release` folder to use as a release asset
- Create a release on github, with your zipped folder as an asset
- Add the topic `optikey-plugin` to your repo and your plugin will now be discovered by Optikey's Plugin Search Wizard. 

