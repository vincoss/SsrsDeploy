
### Generate proxy service

cd {service Directory path}

"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\wsdl.exe" -out:Reportingservice2010Proxy.cs http://localhost:80/ReportServer/ReportService2010.asmx

### Manual service add

1.	Delete your old Service Reference
2.	Right click on References. The "Add Service Reference" dialog comes up.
3.	Do not enter the WSDL URL now, instead: Click on the "Advanced" button at the bottom left.
4.	The "Service Reference Settings" dialog comes up.
5.	At the bottom left, click the "Add Web Reference" button.
6.	Now enter the URL for the WSDL. (for me that was servername/ReportServer/ReportService2010.asmx)
7.	Click the small arrow on the right, it will take its sweet time to load.
8.	Name the web reference, I used "ReportingService2010WebReference", but ReportingService2010" probably works just as well.
9.	Click "Add Reference"
10.	In your code, update your using statements to "using .ReportingService2010WebReference (or whatever name you picked)