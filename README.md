# TableUI
An R&D effort to explore how we can integrate physical input into digital AEC work.
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
│   ├── detection/                              # Python program for detection of AruCo Markers
│   │   ├── .env/
│   │   │   └── ..
│   │   ├── __pycache__/
│   │   │   └── ..
│   │   ├── calibration/
│   │   │   └── ..
│   │   ├── key/
│   │   │   └── ..
│   │   ├── marker-gen/
│   │   │   └── ..
│   │   ├── tests/
│   │   │   └── ..
│   │   ├── adapter.py                          # Should be delected
│   │   ├── camera.py                           # Camera object that uses OpenCV to take frames from an attached camera
│   │   ├── factory.py                          # Factory object to create every Marker object we'll be looking for
│   │   ├── main.py                             # Main file to run the application
│   │   ├── marker.py                           # Marker object that holds all knowledge of where that marker currently is
│   │   ├── repository.py                       # Repository object that throws our data to the outside world (wherever we're gonna be sending data)
│   │   └── repoStrategy.py                     # RepoStrategy objects that determine where we're gonna throw that data (currently to Firebase or a UDP message to a localhost port)
│   ├── grasshopper/
│   │   └── TableUiAdapter                      # A Visual Studio Project where the adapter Grasshopper component will exist (potential for more Grasshopper components)
│   │       ├── bin/
│   │       │   └── ..
│   │       ├── obj/
│   │       │   └── ..
│   │       ├── Properties/
│   │       │   └── ..
│   │       ├── GH_AsyncComponent.cs            # Speckle's Async Component that runs a component on a separate thread
│   │       ├── TableUidapter.csproj            # This is the C# project file that organizes the packages and versions being used
│   │       ├── TableUiAdapterInfo.cs           # This file holds the data for the project
│   │       ├── TestAutoUpdateComponent.cs      # This is a test component to see if we can make a component re-run itself in Grasshopper (YES!)
│   │       ├── UdpReceiveUpdateComponent.cs    # This Grasshopper component updates whenever there's a new UDP message to port 5005 and outputs the Marker data
│   │       └── WorkerInstance.cs
│   ├── TableLib/                               # A class library that contains all objects for receiving UDP messages and giving that data to assign to models
│   │   ├── bin/
│   │   │   └── ..
│   │   ├── obj/
│   │   │   └── ..
│   │   ├── Invoker.cs                          # Invoker Singleton object that organizes the other classes together and retains a persistant memory of markers
│   │   ├── Marker.cs                           # Marker object to hold data on currently detected Markers
│   │   ├── Parser.cs                           # Parser object that takes a JSON string and outputs a list of Marker objects if it was a valid format
│   │   ├── Repository.cs                       # Repository object that interacts with the outside world (listens for UDP currently)
│   │   └── TableLib.csproj                     # C# project file that organizes packages and versions
│   ├── ui/
│   │   └── ..
│   ├── visualizer/
│   │   └── ..
│   └── TableUI.sln
├── tests/                                      # All tests live in this folder
│   └── TableLibTests/                          # Unit tests for TableLib
│       ├── bin/
│       │   └── ..
│       ├── obj/
│       │   └── ..
│       ├── FunctionTests.cs                    # Unit tests to test out ideas
│       ├── ObjectTests.cs                      # Unit tests for individual objects (TODO separate for each object)
│       ├── TableLibTests.csproj
│       ├── UdpTests.cs                         # Unit tests involving UDP messages, but not class objects
│       └── Usings.cs
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