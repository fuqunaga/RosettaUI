# RosettaUI

Code-based GUI library for development menus for Unity

![](Documentation~/2022-04-12-17-18-14.png)



<table>
<td width=450>

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

![](Documentation~/simple.gif)

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

![](Documentation~/2022-04-12-17-40-26.png)

Select `RosettaUI - UI ToolKit` and click the Install button
![](Documentation~/2022-04-12-18-04-29.png)


# How to use

1. Put `Packages/RosettaUI - UIToolkit/RosettaUIRootUIToolkit.prefab` in the Hierarychy
1. Write code to generate `Element` instance
1. Call `RosettaUIRoot.Build(Element)` to generate the actual UI ( [Example] )

[Example]: Assets/Example/ExampleSimple.cs

Examples are available in this repository.
I recommend downloading and checking it out.


# Functions

### UI.Field()
![](Documentation~/field.gif)


### UI.Slider()
![](Documentation~/2022-04-12-18-46-17.png)

### UI.MinMaxSlider()
![](Documentation~/2022-04-12-18-49-48.png)


### Layout elements
![](Documentation~/2022-04-12-18-55-52.png)

### And more
Please check the [Examples](Assets/Example/Common)

# Enviroment

| Platform | Status            |
| -------- | ----------------- |
| Windows  | ✔                 |
| Mac      | Maybe(not tested) |
| Linux    | Maybe(not tested) |
| IL2CPP   | Suspended         |

| UI Library | Status      |
| ---------- | ----------- |
| UI Toolkit | ✔           |
| UGUI       | Suspended   |
| IMGUI      | Not planned |


# See also

[PrefsGUI](https://github.com/fuqunaga/PrefsGUI) - Accessors and GUIs for persistent preference values using a JSON file