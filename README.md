### [Install from VS Marketplace](https://marketplace.visualstudio.com/items?itemName=typeguard.quicktype-vs)

<br />

![](media/quicktype-logo.svg)

[![Build status](https://build.appcenter.ms/v0.1/apps/494bd498-b124-49e5-894e-2f093e06d45b/branches/master/badge)](https://install.appcenter.ms/orgs/quicktype/apps/quicktype-xcode/distribution_groups/Xcode%20Testers)
[![Join us in Slack](http://slack.quicktype.io/badge.svg)](http://slack.quicktype.io/)

`quicktype` infers types from sample JSON data, then outputs strongly typed models and serializers for working with that data in Swift, Objective-C, C++, Java and more. This extension adds native `quicktype` support to Xcode 9.

![](media/demo.gif)

## Visual Studio extension

This extension provides the "Paste JSON as Code" command in the Tools window.  Simply copy some JSON into your clipboard, add a new source file (C#, C++, TypeScript) and invoke the command.

## Building

Clone [quicktype](https://github.com/quicktype/quicktype) and do

    npm run pkg

Then copy `bin/windows/quicktype.exe` into the `Resources` directory in this repository and build with Visual Studio.
