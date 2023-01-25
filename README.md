# RosettaUI

Code-based UI library for development menu for Unity

![](Documentation~/2023-01-25-15-48-22.png)


<div style="display: flex; align-items: start">
<div style="min-width: 450px">

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

</div>
<img src=Documentation~/simple.gif width=400px/>
</div>


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


# How to use

1. Put `Packages/RosettaUI - UIToolkit/RosettaUIRootUIToolkit.prefab` in the Hierarchy
1. Write code to generate `Element` instance
1. Call `RosettaUIRoot.Build(Element)` to generate the actual UI ( [Example] )

[Example]: Assets/Example/ExampleSimple.cs

Examples are available in this repository.
I recommend downloading and checking it out.


# Functions

## UI.Field()

<div style="display: flex; align-items: start; max-width: 1000px">
<img src="Documentation~/field.gif" width=45%/>
<img src="Documentation~/2023-01-25-16-36-00.png" width=55% />
</div>


## UI.Slider()

<div style="display: flex; align-items: start; max-width: 1000px">
<img src="Documentation~/2023-01-25-16-41-59.png" width=50%/>
<img src="Documentation~/2023-01-25-16-56-56.png" width=50%/>
</div>

## UI.MinMaxSlider()

<div style="display: flex; align-items: start; max-width: 1000px">
<img src="Documentation~/2023-01-25-17-05-28.png" width=50% />
<img src="Documentation~/2023-01-25-17-07-45.png" width=55% />
</div>


## UI.List()

<div style="display: flex; align-items: start; max-width: 1000px">
<img src="Documentation~/2023-01-25-17-11-06.png" width=30% />
<img src="Documentation~/2023-01-25-17-25-46.png" width=38% />
</div>

## Layout elements

<div style="display: flex; align-items: start; max-width: 1000px">
<img src="Documentation~/2023-01-25-17-26-32.png" width=35% />
<img src="Documentation~/2023-01-25-17-27-30.png" width=50% />
</div>

## And more...
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


# See also

[PrefsGUI](https://github.com/fuqunaga/PrefsGUI) - Accessors and GUIs for persistent preference values using a JSON file
