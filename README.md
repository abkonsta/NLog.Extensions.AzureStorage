NLog target for Azure Storage Tables
====================================================

This package allows you to set up an NLog target to store logs in Azure table storage.

How to use
==========
- Download package from <a href="https://www.nuget.org/packages/NLog.Extensions.AzureTableStorage/">the nuget site</a>.
- Set up a your connection string in your app.config, web.config, or the cloud configuration (cscfg) file. 
For developmet this can simply be

`````xml
...
<appSettings>
  <add key="StorageAccountConnectionString" value="UseDevelopmentStorage=true" />
</appSettings>
...
`````

A real Azure storage account connection string will look something like this:

`````xml
<appSettings>
  <add 
    key="MyStorageAccount" 
    value="DefaultEndpointsProtocol=https;AccountName=mystoageaccountname;AccountKey=xggxgx[...]gagsae==" />
</appSettings>
`````
- Edit your NLog configuration file to have at least the following information:

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
        connectionStringKey="StorageAccountConnectionString" 
        tableName="TempAzureTableStorageTargetTestsLogs" />
  </targets>
  
  <rules>
    <!-- set up a rule to log to the azure storage target! -->
    <logger name="*" minlevel="Trace" writeTo="AzureTableStorage" />
  </rules>
</nlog>
`````

In this config file, the following parameters can be used to configure your target:

- **name** is the name of your target.
- **xsi:type** must be 'AzureTableStorage'.
- **PartitionKey** is a string which contains macros and string literals, more on that below.
- **RowKey** uses the same syntax as **PartitionKey**.
- **ConnectionStringKey** is the key in your app.config/web.config where the connection string is.
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
- **${machine}** will be replaced by the value of Environment.Machine.
- **${descticks}** will be replaced by the number of ticks *remaining* till <a href="https://msdn.microsoft.com/en-us/library/system.datetime.maxvalue(v=vs.110).aspx">DateTime.MaxValue</a>.

How to View Logs?
=================

I have found that the "Cloud Explorer" built into Microsoft Visual Studio 2015 is sufficient for most of my log searching needs. Think hard about what you are going to use as PartitionKey and RowKey.

When you browse your logs in the Cloud Explorer, the data will look something like this:

![Cloud Explorer Screenshot](screenshot.png?raw=true "Cloud Explorer Screenshot")

Other ways to access table storage:

- <a href="http://www.cloudportam.com/">Cloud Portam</a>
- <a href="http://azurestorageexplorer.codeplex.com/">Azure Storage Explorer</a>

Running Tests
=============

Before running tests on you local machine, make sure Azure Storage Emulator is running.
I think that I need to change the tests to have a setup/teardown executed before each individual unit test. I believe that multiple records run into each other when the tests are all run as a batch.

About NLog Targets
==================

For more info about NLog targets and how to use it, refer to <a href="https://github.com/nlog/NLog/wiki/How%20to%20write%20a%20Target">How To Write a Target</a>.
