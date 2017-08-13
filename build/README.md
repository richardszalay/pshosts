PsHosts still supports PowerShell 3 (.NET 4.0), which requires that the build 
process runs on Windows. As such, CI is configured as such:

1. Appveyor builds, runs unit tests and PowerShell tests, and publishes the 
   module to the Appveyor project feed
1. Travis CI locates the Appveyor build by commit and waits for it to complete. 
   It then installs the module from the Appveyor project feed and runs the PowerShell tests
