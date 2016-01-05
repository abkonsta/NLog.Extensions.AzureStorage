NLog target for Azure Storage Tables
====================================================

This package allows you to set up an NLog target to store logs in Azure table storage.

Changes for version 1.1.4
=========================

Merged pull request from arangas which added the ${level} macro.

Changes for version 1.1.3.2
===========================

**NOTE: Upgrading from 1.1.0 to 1.1.3.2 will break your configuration files!**

Previous versions required the developer to store the azure storage connection string in app.config, web.config or cloud config, and then assign the key to the 'ConnectionStringKey' property of the NLog target.

The new release replaces the 'ConnectionStringKey' property with the 'ConnectionString' property. This allows us to either specify the connection string outright in NLog.config (thus not needing another config file), or to intitialize it on the fly, for example in global.asax, from an environment-dependent config setting.

How to use
==========

- Download package from <a href="https://www.nuget.org/packages/NLog.Extensions.AzureTableStorage/">the nuget site</a>.
- Set up your NLog.config file:

`````xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  
  <!-- include this assembly as an NLog extension -->
  <extensions>
    <add assembly="NLog.Extensions.AzureTableStorage"/>
  </extensions>
  
  <!-- set up a an azure storage table target -->
  <targets>
    <target name="AzureTableStorage" 
        xsi:type="AzureTableStorage" 
        PartitionKey="${date}.${logger}" 
        RowKey="${ticks}.${guid}"
        ConnectionString="UseDevelopmentStorage=true" 
        tableName="TempAzureTableStorageTargetTestsLogs" />
  </targets>
  
  <rules>
    <!-- set up a rule to log to the azure storage target! -->
    <logger name="*" minlevel="Trace" writeTo="AzureTableStorage" />
  </rules>
</nlog>
`````

A real Azure storage account connection string will look something like this:

`````xml
...
  <target ...
  ConnectionString="DefaultEndpointsProtocol=https;AccountName=igdevpdf;AccountKey=xxxxxxx==" 
  ... />
...  
`````

In this config file, the following parameters can be used to configure your target:

- **name** is the name of your target.
- **xsi:type** must be 'AzureTableStorage'.
- **PartitionKey** is a string which contains macros and string literals, more on that below.
- **RowKey** uses the same syntax as **PartitionKey**.
- **ConnectionString** is the Azure storage connection string. Make it an empty string if you are planning to initialize it at runtime.
- **tableName** is the name of the azure storage table. If the table does not exist, it will automatically be created.

Obviously, PartitionKey and RowKey cannot just be left as constants, we need to be able to vary them.
This package allows you to use the following macros to format your partition and row keys:

- **${date}** will be replaced with an 8 symbol date (e.g. 20150926).
- **${time}** will be replaced with a 6 digit time (e.g. 221110).
- **${ticks}** will be replaced with a 19 digit number of <a href="https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx">ticks</a>.
- **${longdate}** will be replaced with a 20 symbol long date which includes year, month, day, hour, minutes, seconds and millionths of a second.
- **${micros}** will be replaced with a 6 digit number of millionths of a second (microseconds).
- **${guid}** will be replaced with a new random GUID.
- **${logger}** will be replaced by the name of the current class logger.
- **${level}** will be replaced by the log level. Also supports ${level:uppercase=true}.
- **${machine}** will be replaced by the value of Environment.Machine.
- **${descticks}** will be replaced by the number of ticks *remaining* till <a href="https://msdn.microsoft.com/en-us/library/system.datetime.maxvalue(v=vs.110).aspx">DateTime.MaxValue</a>.

Configuring the Target Dynamically
==================================

You can initialize the target's connection string programmatically, for example, from Global.asax.cs:

`````c#
protected void Application_Start()
{
  var azureStorageTarget = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureStorage");
  azureStorageTarget.ConnectionString = "actual conn. string // or pull from web.config";
  LogManager.ReconfigExistingLoggers();
}
`````

If you do that, you may want to set the 'ConnectionString' property of the AzureTableStorage target in NLog.config to an empty string. 
That will prevent you from accidentally logging something to an un-intended location before you initialized it.


How to View Logs?
=================

I have found that the "Cloud Explorer" built into Microsoft Visual Studio 2015 is sufficient for most of my log searching needs. Think hard about what you are going to use as PartitionKey and RowKey.

When you browse your logs in the Cloud Explorer, the data will look something like this:

![Cloud Explorer Screenshot](screenshot.png?raw=true "Cloud Explorer Screenshot")

Other ways to access table storage:

- <a href="http://www.cloudportam.com/">Cloud Portam</a>
- <a href="http://azurestorageexplorer.codeplex.com/">Azure Storage Explorer</a>

What if I Run Into Errors
=========================

If you deploy your application and you are seeing this error:

```
No connection could be made because the target machine actively refused it 127.0.0.1:10002 
```

it means that after resolving your configuration, the AzureTableStorage target's ConnectionString property is still set
to "UseDevelopmentStorage=true" and the process is trying to connect to a (non-existent) Windows Azure emulator on your
production web server.


Running Unit Tests
==================

Before running tests on you local machine, make sure Azure Storage Emulator is running.
I think that I need to change the tests to have a setup/teardown executed before each individual unit test. I believe that multiple records run into each other when the tests are all run as a batch.

About NLog Targets
==================

For more info about NLog targets and how to use it, refer to <a href="https://github.com/nlog/NLog/wiki/How%20to%20write%20a%20Target">How To Write a Target</a>.
