# Drone Testbed - Unity Interface
This is the Unity interface for the HyForm drone design testbed. It pairs with a server backend available here: https://github.com/hyform/drone-testbed-server

## Quick setup guide

1) Download and install Unity 2018.4.12f1
2) Configure the auth.txt 
3) Open Unity -> Open Project -> select this folder

This folder includes the Unity project for the designer, operational planner, and business 
planner interfaces. The designer interface is stored as a scene located in Assets\Projects\designtool. The 
operational planner and business planner interfaces are stored as a scene located in Assets\Projects\plantool. 
To open either interface, open the project in Unity by selecting this directory. Then, within Unity, open the 
scene based on the above paths. Before, you run either interface, make sure your auth.txt file 
is configured properly.

auth.txt file :

first line : url of your hosted server instance
second line : user name
third line : user password
fourth line : user team
fifth line : business view boolean for the planning tool
sixth line : boolean that logs user actions to the central server
seventh line : include AI tools

ex.
https://www.server.foo/
user-11
userpassword
Hyform Team 1
true
false
true

Directory structure :
Assets/Projects/designtool : the location of the scene and scripts for the designer tool
Assets/Projects/designtool/Scripts/localEvaluation : the location of helper classes for the local physics evaluation for the designer tool
Assets/Projects/materials : materials used by the designer and planner tools
Assets/Projects/misc : GUI helpers used by both the designer and planner tools
Assets/Projects/plantool : the location of the scene and scripts for the planner tool
Assets/Projects/shared : client and web-based interface helpers for Unity builds
Assets/Projects/sounds : sounds used by the designer and planner tools
Assets/Projects/webservice : data objects and web service interface helpers to save vehicles, plans, and scenarios 
Assets/Resources : toolbar icons

Designer Scene Hierarchy:
Main Camera
Directional Light
GUI : the main UAVDesigner.cs script gets attached to this on start up
restapi : store the script to load/send objects to the central server
graphicsandcomponents : Unity objects for the vehicle assembly (motors, foils, labels ...)
testplatform : test platform shown after the evaluation and for drone evaluation results and trajectories
sounds
StartupGameObject : setup scripts for the scene
panels : all GUI overlay displays for toolboxes and confirmation panels
EventSystem
teststand : similar to testplatform, it is used during the evaluation to detect any collisions

Planner Scene Hierarchy:
Main Camera
Directional Light
restapi : store the script to load/send objects to the central server
groundcube : ground in the scene where the main script is attached at start up
EventSystem
directionobjects : N, E, S, W letters on the ground
StartupGameObject : setup scripts for the scene
guiobjects : Unity objects for the planning interface (houses, labels ...)
panels : all GUI overlay displays for toolboxes and confirmation panels
sounds

## Citing HyForm™
If you use HyForm in your own research, please cite it using this paper:

B. Song, N.F. Soria Zurita, G. Zhang, G. Stump, C. Balon, S.W. Miller, M. Yukish, J. Cagan, and C. McComb (2020). "Toward Hybrid Teams: A Platform To Understand Human-Computer Collaboration During the Design of Complex Engineered Systems." International Design Conference. [(pdf)](https://engrxiv.org/n7u95)

## Funding
This material is based upon work supported by the Defense Advanced Research Projects Agency through cooperative agreement N66001-17-1-4064. Any opinions, findings, and conclusions or recommendations expressed in this repository are those of the contributors and do not necessarily reflect the views of the sponsors.
