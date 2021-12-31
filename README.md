# PsHosts

PsHosts is a [PowerShell](https://github.com/PowerShell/PowerShell) Module that provides Cmdlets for manipulating the local hosts file on Windows, Linux, and macOS. Supports tab completion for hostnames.

All destructive commands support `-whatif` and honor original formatting wherever possible.

![PsHosts in action](img/demo.gif?raw=true)

## Installation

Windows 10, Linux, and macOS users can simply install the module using the command below. For Windows 8.1 and below, [PsGet](http://psget.net/) can be used with the same command.

    PS C:\> Install-Module PsHosts

To install manually, download a release and unzip to $home\Documents\WindowsPowerShell\Modules\PsHosts

## Usage

The Cmdlets use the noun `HostEntry` and support the following verbs:

* Get
* Test
* Add
* Set
* Remove
* Enable
* Disable

General things to remember:

* **All destructive Cmdlets require admin elevation**
* The first parameter is always the hostname, which supports tab completion
* All Cmdlets support pipelining
* All destructive Cmdlets support `-whatif`

With that in mind, here are some samples:

```PowerShell
# List all entries
Get-HostEntry

# List matching entries
Get-HostEntry *.local
Get-HostEntry mysite.local

# Test if an entry exists
Test-HostEntry mysite.local

# Add a localhost entry
Add-HostEntry mysite.local -Loopback

# Add a specific entry
Add-HostEntry mysite.local 192.168.1.1

# Change an entry's IP address
Set-HostEntry mysite.local 127.0.0.2

# Add a comment
Set-HostEntry mysite.local -Comment Excellent

# Rename a host
Get-HostEntry mysite.local | Set-HostEntry mysite2.local

# Disable (comment out) entries
Disable-HostEntry mysite.local

# Enable (uncomment) entries
Enable-HostEntry mysite.local

# Remove entries
Remove-HostEntry mysite.local

# Remove matching entries
Remove-HostEntry *.local

# Disable all loopback entries
Get-HostEntry | ?{$_.IsLoopback} | Disable-HostEntry
```

## Build Status

### Master branch

|         OS - PS Version             |          Build Status        |
|-------------------------------------|------------------------------|
| AppVeyor (Windows - PS 5.1)         | [![m-av-image][]][m-av-site] |
| Travis CI (Linux - PS 6.0.0-Beta)  | [![m-tv-image][]][m-tv-site] |
| Travis CI (MacOS - PS 6.0.0-Beta)  | [![m-tv-image][]][m-tv-site] |

[m-av-image]: https://ci.appveyor.com/api/projects/status/cyaxgxjgwnmehyrg/branch/master?svg=true
[m-av-site]: https://ci.appveyor.com/project/richardszalay/pshosts/branch/master
[m-tv-image]: https://api.travis-ci.org/richardszalay/pshosts.svg?branch=master
[m-tv-image]: https://travis-ci.org/PowerShell/PowerShellGet.svg?branch
[m-tv-site]: https://travis-ci.org/richardszalay/pshosts

## Development

Building is supported on all platforms with PowerShell.

PsHosts uses [Invoke-Build](https://github.com/nightroman/Invoke-Build) for build automation. Tests can be run by running the following from the root of the repository:

```powershell
.\build.ps1 -Test
```

The solution is made up of a number of projects:

* RichardSzalay.Hosts - .NET Library containing core API for manipulating the hosts file
* RichardSzalay.Hosts.Tests - Unit tests for core API using [Machine.Specifications](https://github.com/machine/machine.specifications) (Mspec)
* RichardSzalay.Hosts.Powershell - PowerShell Cmdlets library (.NET)
* RichardSzalay.Hosts.Powershell.Tests - [Pester](https://github.com/pester/Pester) tests for the PowerShell Cmdlets
