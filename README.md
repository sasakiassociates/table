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
│   ├── detection/
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
│   │   ├── adapter.py
│   │   ├── camera.py
│   │   ├── factory.py
│   │   ├── main.py
│   │   ├── marker.py
│   │   ├── repository.py
│   │   └── repoStrategy.py
│   ├── grasshopper/
│   │   └── TableUiAdapter
│   │       ├── bin/
│   │       │   └── ..
│   │       ├── obj/
│   │       │   └── ..
│   │       ├── Properties/
│   │       │   └── ..
│   │       ├── GH_AsyncComponent.cs
│   │       ├── TableUidapter.csproj
│   │       ├── TableUiAdapterInfo.cs
│   │       ├── TestAutoUpdateComponent.cs
│   │       ├── UdpReceiveUpdateComponent.cs
│   │       └── WorkerInstance.cs
│   ├── library/
│   │   ├── bin/
│   │   │   └── ..
│   │   ├── obj/
│   │   │   └── ..
│   │   ├── Invoker.cs
│   │   ├── Marker.cs
│   │   ├── Parser.cs
│   │   ├── Repository.cs
│   │   ├── TableLib.csproj
│   │   └── UdpListener.cs
│   ├── ui/
│   │   └── ..
│   ├── visualizer/
│   │   └── ..
│   └── TableUI.sln
├── tests/
│   └── TableLibTests/
│       ├── bin/
│       │   └── ..
│       ├── obj/
│       │   └── ..
│       ├── FunctionTests.cs
│       ├── ObjectTests.cs
│       ├── TableLibTests.csproj
│       ├── UdpTests.cs
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