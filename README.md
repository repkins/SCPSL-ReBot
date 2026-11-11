# SCP: Secret Laboratory ReBot

Adds goal-oriented AI to dummies to game server running on Unity Engine. Rewrite of previous SCPSL bot plugin.

## Features

 - **Complete rewrite** into data-oriented design to enable better CPU utilization.
 - AI of this plugin gets added to every dummy.
 - **GOAP inspired decision making system** which enables performing actions **optimally** using action path-finding algorithm.
 - Supports _currently_ the following roles:
   - **SCP 049**
   - **SCP 106**
   - **SCP 939**
   - **SCP 096**
   - **SCP 173**
 - Support for **other roles** will be added later.
 - Supported SCP roles **terminates enemies**, attacking sensed enemies. **Finds enemies** by visiting different rooms.
 - **Opens doors**, **uses emergency door overrides**, **uses elevators** and **jumps** to try reach destinations.
 - **Built-in mesh-based navigation system** with in-game navigation mesh editor (legacy from previous project).
 - Due to design of legacy built-in navigation system, plugin patches facility generator to **use standard room connectors**.

## Dependencies

 - Harmony 2.3

## Installation

 1. Download plugin archive from releases.
 2. Put contents of the archive to the relevant LabAPI plugins sub-directory.
