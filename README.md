# RosettaUI

[![npm version](https://badge.fury.io/js/ga.fuquna.rosettaui.svg)](https://badge.fury.io/js/ga.fuquna.rosettaui)
[![openupm](https://img.shields.io/npm/v/ga.fuquna.rosettaui?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/ga.fuquna.rosettaui/)

Code-based UI library for development menu for Unity

<img src="https://github.com/user-attachments/assets/4313a51f-e319-457b-a227-a0caf4d0f908" />



<table>
<tr>
<td>
    
```csharp
public class ExampleSimple : MonoBehaviour
{
    public string stringValue;
    public float floatValue;
    public int intValue;
    public Color colorValue;

    
    void Start()
    {
        var root = GetComponent<RosettaUIRoot>();
        root.Build(CreateElement());
    }

    Element CreateElement()
    {
        return UI.Window(nameof(ExampleSimple),
            UI.Page(
                UI.Field(() => stringValue),
                UI.Slider(() => floatValue),
                UI.Row(
                    UI.Field(() => intValue),
                    UI.Button("+", () => intValue++),
                    UI.Button("-", () => intValue--)
                ),
                UI.Field(() => colorValue)
            )
        );
    }
}
```
</td>
<td>
    
![simple](https://github.com/user-attachments/assets/16f31cf9-5608-4acc-8629-6c22bc8ef261)

</td>
</tr>

</table>

# Installation

This package uses the [scoped registry] feature to resolve package
dependencies. 

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html


**Edit > ProjectSettings... > Package Manager > Scoped Registries**

Enter the following and click the Save button.

```
"name": "fuqunaga",
"url": "https://registry.npmjs.com",
"scopes": [ "ga.fuquna" ]
```
![](Documentation~/2022-04-12-17-29-38.png)


**Window > Package Manager**

Select `MyRegistries` in `Packages:`

<img src="Documentation~/2022-04-12-17-40-26.png" width=35%>

Select `RosettaUI - UI ToolKit` and click the Install button
![](Documentation~/2022-04-12-18-04-29.png)


### Input System(optional)

RosettaUI recommends using Input System.  
See [Tips](#disable-keyboard-input-when-typing-in-ui).

Install according to the official documentation.  
https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Installation.html

# How to use

1. Put `Packages/RosettaUI - UIToolkit/RosettaUIRootUIToolkit.prefab` in the Hierarchy
1. Write code to generate `Element` instance
1. Call `RosettaUIRoot.Build(Element)` to generate the actual UI ( [Example] )

[Example]: Assets/Example/ExampleSimple.cs

Examples are available in this repository.
I recommend downloading and checking it out.


# Functions

## UI.Field()

![](Documentation~/field.gif)
![](Documentation~/2023-01-25-16-36-00.png)



## UI.Slider()

![](Documentation~/2023-01-25-16-41-59.png)
![](Documentation~/2023-01-25-16-56-56.png)


## UI.MinMaxSlider()

![](Documentation~/2023-01-25-17-05-28.png)
![](Documentation~/2023-01-25-17-07-45.png)

## UI.List()

![](Documentation~/2023-01-25-17-11-06.png)
![](Documentation~/2023-01-25-17-25-46.png)

## Layout elements


![](Documentation~/2023-01-25-17-26-32.png)
![](Documentation~/2023-01-25-17-27-30.png)


## And more!
Please check the [Examples](Assets/Scenes)

# Enviroment

| Platform | Support           |
| -------- | ----------------- |
| Windows  | ✔                 |
| Mac      | Maybe(not tested) |
| Linux    | Maybe(not tested) |
| IL2CPP   | Suspended         |

| UI Library | Support      |
| ---------- | ----------- |
| UI Toolkit | ✔           |
| UGUI       | Suspended   |
| IMGUI      | Not planned |


# Tips

## Disable keyboard input when typing in UI

When using InputSystem, set `RosettaUIRoot.disableKeyboardInputWhileUITyping=true (default)` to disable keyboard input while typing in UI.
```csharp
// false while typing in UI
if ( Keyboard.current[Key.A].wasPressedThisFrame )
{
    // do something
}
```

For LegacyInputSystem, refer to `RosettaUIRoot.WillUseKeyInputAny()`.
```csharp
if ( !RosettaUIRoot.WillUseKeyInputAny() && Input.GetKeyDown(KeyCode.A) )
{
    // do something
}
```


# See also

[PrefsGUI](https://github.com/fuqunaga/PrefsGUI) - Accessors and GUIs for persistent preference values using a JSON file
