# hooper
Aaron Hooper's Trace File Generator
The goal of this project is to create a tool that can take a map region 
exported from OpenStreetMap and create an MMTS-style vehicular trace file.

*** INSTRUCTIONS TO USERS ***
To run the tool, type the following

    mono VGT.exe map.osm 100
    mv trace.txt trace100.csv

*** COMMENTS TO USERS ***
You must include the .exe after the VGT 
You can change the 100 to any number of vehicles in the system
The duration of the simulation will always be 2000 seconds
The github repo is https://github.com/vanetprivacy/hooper.git
to tweak this code on your own, create cloud9 account and install mono, 
  microsoft's linux version of visual studio, then clone the repo

*** INSTRUCTIONS TO PROGRAMMER ***
The following things need to be done but aren't yet...
create 10% of cars at beginning and other 90% distributed over 2000 sec
create 10% of vehicles inside and 90% on edge/border
