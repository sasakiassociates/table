# TableUI
An R&D effort to explore how we can leverage digital tools using physical input for AEC projects. Curently this is compatible with Rhino Grasshopper.
## Overview
This project has three main components: a `detector` (currently OpenCV), a `processor` to calculate the data we'd like to see, and a `visualizer`.
## Table of Contents
- [Directory Structure](#directory-structure)
- [Installation](#installation)
- [Detector](#detector)
- [Processor](#processor)
- [Visualizer](#visualizer)
- [Contributing](#contributing)
- [License](#license)
# Directory Structure
```
table/
├── .vs/
│   └── ..
├── .vscode/
│   └── ..
├── src/
│   ├── .vs/
│   ├── marker-generator/
│   │   ├── markers/                            # Folder containing images of all the ArUco markers in DICT_6x6_100 of ArUco
│   │   │   ├── marker1.png
│   │   │   ├── marker2.png
│   │   │   └── ...
│   │   └── createMarkers.py                    # Script to generate ArUco markers from a predefined dictionary
│   ├── sender/
│   │   ├── key/                                # Folder to contain the "firebase_table-key.json" file for locating/authenticating access to a Firebase Realtime Database
│   │   │   └── firebase_table-key.json         # TODO currently needs to be added by user (gitignored and ideally unique for each project). How can users upload this themselves?
│   │   ├── repository.py                       # Repository object that throws our data to the outside world (wherever we're gonna be sending data)
│   │   └── repoStrategy.py                     # RepoStrategy objects that determine where we're gonna throw that data (currently to Firebase or a UDP message to a localhost port)
│   ├── detector/                               # Python program for detection of AruCo Markers
│   │   ├── .env/
│   │   │   └── ..
│   │   ├── __pycache__/
│   │   │   └── ..
│   │   ├── calibration/
│   │   │   └── ..
│   │   ├── camera.py                           # Camera object that uses OpenCV to take frames from an attached camera
│   │   ├── factory.py                          # Factory object to create every Marker object we'll be looking for
│   │   ├── main.py                             # Main file to run the application
│   │   └── marker.py                           # Marker object that holds all knowledge of where that marker currently is
│   ├── receiver/
│   │   ├── bin/
│   │   │   └── ..
│   │   ├── obj/
│   │   │   └── ..
│   │   ├── Marker.cs                           # Marker object to hold data on currently detected Markers
│   │   ├── Parser.cs                           # Parser object that takes a JSON string and outputs a list of Marker objects if it was a valid format
│   │   ├── Repository.cs                       # Repository object that interacts with the outside world (listens for UDP currently)
│   │   └── TableUiReceiver.csproj
│   ├── grasshopper/
│   │   └── TableUiAdapter                      # A Visual Studio Project where the adapter Grasshopper component will exist (potential for more Grasshopper components)
│   │       ├── bin/
│   │       │   └── ..
│   │       ├── obj/
│   │       │   └── ..
│   │       ├── Properties/
│   │       │   └── ..
│   │       ├── TableUidapter.csproj            # This is the C# project file that organizes the packages and versions being used
│   │       ├── TableUiAdapterInfo.cs           # This file holds the data for the project
│   │       └── TableUIReceiver.cs    # This Grasshopper component updates whenever there's a new UDP message to port 5005 and outputs the Marker data
│   ├── ui/
│   │   └── ..
│   ├── visualizer/
│   │   └── ..
│   └── TableUI.sln
├── tests/                                      # All tests live in this folder
│   ├── TableLibTests/                          # Unit tests for TableLib
│   │   ├── bin/
│   │   │   └── ..
│   │   ├── obj/
│   │   │   └── ..
│   │   ├── FunctionTests.cs                    # Unit tests to test out ideas
│   │   ├── ObjectTests.cs                      # Unit tests for individual objects (TODO separate file for each object)
│   │   ├── TableLibTests.csproj
│   │   ├── UdpTests.cs                         # Unit tests involving UDP messages, but not class objects
│   │   └── Usings.cs
│   └── DetectionTests/                         # Python unit tests for the Detector
│       ├── __pycache__/
│       │   └── ..
│       ├── context.py                          # Lets the test scripts locate the Detector files using a relative path
│       ├── firebaseTests.py                    # Tests for sending data to a Firebase Database
│       └── udpTests.py                         # Tests for sending data via UDP messages
├── .gitignore
├── LICENSE
├── README.md
└── tableUI.code-workspace
```
# Installation
# Detector
# Processor
# Visualizer
# Contributing
# License