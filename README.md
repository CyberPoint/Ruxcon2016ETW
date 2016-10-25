
#Make ETW Greate Again - Ruxcon 2016
This is a repo of our Proof of Concept (POC) code for our Event Tracing for Widows (ETW) 
presentation at Ruxcon 2016. This code was written to demonstrate the concepts we 
discussed in our presentation and is NOT intended for "production" use. Though we welcome 
feedback, comments and questions this code is provided "as-is" without support.

For feedback/questions please contact us at: 
srt [at] cyberpointllc.com


The Following POCs are included

##ETW USB Keylogger
A tool demonstrating how ETW can be leveraged for logging keystrokes from USB keyboards 
including clear-text credentials for Windows logins.

##WinINet SSL Sniffing
A tool that captures all data that passes through the WinINet library, encrypted or otherwise. 
Since WinINet is the library used for transmitting HTTP/HTPS data in IE, Edge, as well as *most* 
applications in the Windows App store this potentially allows for the capture passwords, user 
sessions, cookies, and any other private data. 

##Ransomware Detection with ETW
A short demo showcasing ETW's ability for "signatureless" ransomware detection. This technique 
utilizes the Windows Kernel provider to detect multiple versions of ransomware without signatures 
and is based purely on heuretics provided by ETW. 

##Requirements
All examples require the TraceEvent library from Microsoft

#License 
All code code in the repo is subject to GPL v3. See the LICENSE file

