# Ransomware Detection

This is s POC for a "signatureless" detection of ransomware with the ETW. Using only the
windows Kernel provider we are able to detect a large number different ransomware families based 
only on heuristics generated from ETW. As our algorithm is heuristic-based we are able to 
generically detect most famalies of ransomware  and in most cases during our testing, we are
able to detect brand new samples without any previous knowledge of the sample in question.

##List of samples tested
cerber, chimera, ctb-locker, locky, hydracrypt, jigsaw, lockscreen, mobef, radamant, samsam, shade,
teslascrypt, torrentlocker, trucrypter, 7ev3n, coverton, kimcilware, petya

##Requirements
Admin privileges

##Limitations
Dynamic detection can have variable delay

