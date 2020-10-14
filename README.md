# UI Manager


[![openupm](https://img.shields.io/npm/v/com.brunomikoski.uimanager?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.brunomikoski.uimanager/) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/177397001d74494a9ec54031a428c8dc)](https://www.codacy.com/manual/badawe/ScriptableObjectCollection?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=badawe/ScriptableObjectCollection&amp;utm_campaign=Badge_Grade)

[![](https://img.shields.io/github/followers/brunomikoski?label=Follow&style=social)](https://github.com/brunomikoski) [![](https://img.shields.io/twitter/follow/brunomikoski?style=social)](https://twitter.com/brunomikoski)


*UI Manager* it's a UI/GUI/Windows solution for Unity with layers approach, It's built to support the most variety of games with complex demands and expensive UI.


## Features
 - Automatically Instantiation and manage of windows based on Layers
 - Built on [ScriptableObject Collection](https://github.com/brunomikoski/ScriptableObjectCollection/), so you have quick access by code/inspector of your Windows and features.
 - Transitions support, you can quickly define a series of transition per window, and quickly create and reuse new ones.
 - Quickly access window events
 - Fully events system for interacting with window.
 - Full Window Interfaces for quickly develop windows components.
 - 2 Layers Behaviours, `Exclusive`: will close the current open window before opening a new one, `Additive`: will open the new window on top
 - Groups Support, setup your windows based on groups (Gameplay / Meta / Shop) and quickly load / unload specific groups to free memory  
 - Script Based, not messying around trying to see where a window reference is.

## How to use?
 - UI Manager needs 4 different Scriptable Object Collections to work WindowIDs, LayerIDs, GroupIDs and Transitions, you should be able to create it by the `Tools / UI Manager / Initial Setup`
 - Every GUI that you want to display on the system, needs his own unique identifier (WindowIDs) right now the system comes with the following options:
`PrefabBasedWindowID` and `AddressablesBasedWindowID` if you have the AddressableSystem package on your project.


## FAQ
 - How do I create a new Window?
    - To create a new Window you need both a new `WindowID` and a Prefab, after creating a new one on the `WindowIDs` and define the target settings, you just need to assign the `Prefab` reference into the WindowID to work.
- How do I access the Windows Manager?
    - The windows manager its a pure solution for working with UI, so I doesn't come with a Singleton/Service Locator/DI System, feel free to use whatever your project needs.
    - After the initial setup is done, you can use the static generated file generated out of `WindowIDs` to interact with windows, ex.: `AllWindows.MainMenu.Open();`     
- How can I access the events?
    - You can subscribe to any window event by: `windowManager.SubscribeToWindowEvent(WindowEvent targetEvent, WindowID windowID, Action callback)` and `windowManager.SubscribeToAnyWindowEvent(WindowEvent targetEvent, Action<Window> callback)
    - Available Window Events: `OnWillOpen, OnOpened, OnWillClose, OnClosed, OnLostFocus, OnGainFocus`
    - You can also subscribe to specific transitions `windowsManager.SubscribeToTransitionEvent(AllWindows.MainMenu, AllWindows.Shopping, OnEnterShopFromMainMenu);` 
- I want some component of a Window to do something when the window is open
    - You just need your child component of the window to implement one of the WindowInterfaces: `IOnAfterWindowOpen` 
- I want to create a new transition
    - Just select the Transitions Collection and create a new type by using the Add Menu, you can do the transition that you want. 
     
## System Requirements
Unity 2018.4.0 or later versions


## Installation

### OpenUPM
The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.brunomikoski.uimanager
```

### Manifest
You can also install via git URL by adding this entry in your **manifest.json**
```
"com.brunomikoski.uimanager": "https://github.com/brunomikoski/UIManager.git"
```

### Unity Package Manager
```
from Window->Package Manager, click on the + sign and Add from git: https://github.com/badawe/ScriptableObjectCollection.git
```

## License TL:DR
- You can freely use Scriptable Object Collection in both commercial and non-commercial projects
- You can redistribute verbatim copies of the code, along with any readme files and attributions
- You can modify the code only for your own (company/studio) use and you cannot redistribute modified versions outside your own company/studio (but you can send pull requests to me)

