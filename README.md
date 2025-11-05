# RosettaUI

[![npm version](https://badge.fury.io/js/ga.fuquna.rosettaui.svg)](https://badge.fury.io/js/ga.fuquna.rosettaui)
[![openupm](https://img.shields.io/npm/v/ga.fuquna.rosettaui?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/ga.fuquna.rosettaui/)

Code-based UI library for development menu for Unity

<img src="https://github.com/user-attachments/assets/4313a51f-e319-457b-a227-a0caf4d0f908" />

<br>
<br>

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
<td width="50%">
    
<img src="https://github.com/user-attachments/assets/16f31cf9-5608-4acc-8629-6c22bc8ef261" />

</td>
</tr>

</table>

<br>

### ✨ Features

- Undo/Redo surpport (runtime only)
- Inspector-like visual editor for Gradient and AnimationCurve at runtime

<table>
    <tr>
        <td width="45%">
        <img src="https://github.com/user-attachments/assets/66e4ccff-5800-4ef1-a026-370370d36bcb" />
        </td>
        <td>
        <img src="https://github.com/user-attachments/assets/6f58a454-47e3-402f-8f99-1774ad8f0825" />
        </td>
    </tr>
</table>

- Inspector-compatible Copy and Paste

  ![copyandpaste](https://github.com/user-attachments/assets/2a2010c6-a5ce-4403-b7d0-81a974260cc9)


<br>

# 🔄 Ver1 → Ver2 Migration

Please remove the `RosettaUI.UIToolkit` package from the Package Manager.  
In Ver2, the `RosettaUI.UIToolkit` package is now included in the `RosettaUI` package.

<br>

# ⬇️ Installation

This package uses the [scoped registry] feature to resolve package
dependencies. 

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html


<br>

**Edit > ProjectSettings... > Package Manager > Scoped Registries**

Enter the following and click the Save button.

```
"name": "fuqunaga",
"url": "https://registry.npmjs.com",
"scopes": [ "ga.fuquna" ]
```

<br>

**Window > Package Manager**

Select `MyRegistries`> `fuqunaga` > `RosettaUI` and click the Install button

<br>

### Input System (optional)

RosettaUI recommends using Input System.  
See [Tips](#disable-input-when-ui-focused).

Install according to the official documentation.  
https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Installation.html


<br>

# 🚀How to use

1. Add `Packages/RosettaUI/UIToolkit/Runtime/RosettaUIRootUIToolkit.prefab` to the Hierarchy.
1. Write code to generate an `Element` instance.
1. Call `RosettaUIRoot.Build(Element)` to generate the actual UI (see [ExampleSimple]).


[ExampleSimple]: Assets/Example/Runtime/ExampleSimple.cs

👉 <b>[Examples](Assets/Scenes) are available in this repository.</b>  
We recommend downloading and checking it out.

<br>

# 💻 Environment

| Platform | Support            |
| -------- |--------------------|
| Windows  | ✔                  |
| Mac      | Maybe (not tested) |
| Linux    | Maybe (not tested) |
| IL2CPP   | Not supported      |

<br>

# 💡️ Tips

## Disable input when UI focused

During UI operations, input to the application is suppressed by replacing the keyboard, pointer, and mouse devices with dummies.

```csharp
// false while RosettaUI focused
if ( Keyboard.current[Key.A].wasPressedThisFrame )
{
    // do something
}
```

For LegacyInputSystem, refer to `RosettaUIRoot.IsFocused()`.
```csharp
if ( !RosettaUIRoot.IsFocused() && Input.GetKeyDown(KeyCode.A) )
{
    // do something
}
```

<br>

# 🔎 Related Libraries

[PrefsGUI](https://github.com/fuqunaga/PrefsGUI) - Accessors and GUIs for persistent preference values using a JSON file
