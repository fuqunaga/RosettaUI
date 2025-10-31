# Changelog

## [2.0.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.11.0...ga.fuquna.rosettaui-v2.0.0) (2025-10-31)

### BREAKING CHANGES
* Integrated the `RosttaUI.UIToolkit` package into the main `RosttaUI` package. 
  > **Note:** Users upgrading from v1.x must remove the old `RosttaUI.UIToolkit` package to avoid conflicts.

### Features
* Undo/Redo.
* Nested dropdown menu.

### Changed
* Updated the `AnimationCurveEditor` with an Inspector-like user interface.


## [ga.fuquna.rosettaui-v1.11.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.10.0...ga.fuquna.rosettaui-v1.11.0) (2025-04-16)


### Features

* Update UI.ScrollView to treat received size as MaxWidth/MaxHeight ([b392fc7](https://github.com/fuqunaga/RosettaUI/commit/b392fc79eff694787e998ffb990f3c1e7acfd57d))

## [ga.fuquna.rosettaui-v1.10.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.9.3...ga.fuquna.rosettaui-v1.10.0) (2025-04-10)


### Features

* change default list item label to first string of class ([#38](https://github.com/fuqunaga/RosettaUI/issues/39)) ([6901462](https://github.com/fuqunaga/RosettaUI/commit/6901462069d6f841be0a5da6ea79c0dce2301321))
* added UICustom.RegisterPropertyAttributeFunc() ([c83364a](https://github.com/fuqunaga/RosettaUI/commit/c83364a350e7d83430d7dbf06661d1d6c719f333))
* SetFlexBasis, SetFlexWrap ([ad64e40](https://github.com/fuqunaga/RosettaUI/commit/ad64e4078c6200dfd2f67817c69a4ebfa513eab7))

## [ga.fuquna.rosettaui-v1.9.3](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.9.2...ga.fuquna.rosettaui-v1.9.3) (2025-03-03)


### Bug Fixes

* typo ([251aea7](https://github.com/fuqunaga/RosettaUI/commit/251aea7474e19272a0967288ac08c048f896ec17))

## [ga.fuquna.rosettaui-v1.9.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.9.1...ga.fuquna.rosettaui-v1.9.2) (2025-03-03)


### Bug Fixes

* Add keywords to package.json to get hits in npmjs search ([31e8ead](https://github.com/fuqunaga/RosettaUI/commit/31e8ead43ba3fd52be865345cf5c4dd339c49aa5))

## [ga.fuquna.rosettaui-v1.9.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.9.0...ga.fuquna.rosettaui-v1.9.1) (2025-02-18)


### Bug Fixes

* view fix bug etc. ([cfb9f48](https://github.com/fuqunaga/RosettaUI/commit/cfb9f4808c5d5e2a50066dac0331ecaadda91e33))

## [ga.fuquna.rosettaui-v1.9.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.8.0...ga.fuquna.rosettaui-v1.9.0) (2025-02-17)


### Bug Fixes

* change cb.SetGlobal to mat.Set, tiny bug, code format ([d7b41bc](https://github.com/fuqunaga/RosettaUI/commit/d7b41bcf56b132117c3eb0ab539556b2a04878d5))


### Features

* animation curve copy and paste support ([e4883a7](https://github.com/fuqunaga/RosettaUI/commit/e4883a772d90e4d32c8020f54d46efa32d5b9115))
* fit curve, snap points ([eb645c6](https://github.com/fuqunaga/RosettaUI/commit/eb645c65ce0306f474ca944d24bbbe45565fbad4))
* point mode, tangent mode ([84ba70f](https://github.com/fuqunaga/RosettaUI/commit/84ba70f687114730512c32c965164c2f55aee315))
* render graph in curve editor ([d1f4d06](https://github.com/fuqunaga/RosettaUI/commit/d1f4d0630af1aaa7bcfacffff10f9b0b69589811))
* zoom each axis ([2bc13e7](https://github.com/fuqunaga/RosettaUI/commit/2bc13e7b672ecef48e605e42498167068aaca7f3))

## [ga.fuquna.rosettaui-v1.8.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.7.4...ga.fuquna.rosettaui-v1.8.0) (2024-09-11)


### Bug Fixes

* ClipBoardParse_Generic error when same name but different type ([fdbd36e](https://github.com/fuqunaga/RosettaUI/commit/fdbd36eee3d88a823be9cbd2b1e8429867c4f527))
* The AddRemove menu is not displayed when the List is fixedSize. ([ca01c8a](https://github.com/fuqunaga/RosettaUI/commit/ca01c8a6fce35ad1f9077e606bde50f5a053480e))


### Features

* Add ListViewOption.suppressAutoIndent ([0df33d2](https://github.com/fuqunaga/RosettaUI/commit/0df33d2b544c59f52b06cdd8f752da8629e0c04a))
* add UI.PopupMenuButton() ([7ee2ded](https://github.com/fuqunaga/RosettaUI/commit/7ee2ded79cb0d1c8502557a4532e9d5c19cd449e))
* **experimental:** Element.SetFlexGrow()/SetFlexShrink() ([7896dff](https://github.com/fuqunaga/RosettaUI/commit/7896dff9ccdcbcd4316bea8bbf68275f6204b473))
* MouseButton can now be specified in UI.Popup(). ([d9121d1](https://github.com/fuqunaga/RosettaUI/commit/d9121d1cdde334c69e46296f4f40563dcabb8ed6))

## [ga.fuquna.rosettaui-v1.7.4](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.7.3...ga.fuquna.rosettaui-v1.7.4) (2024-08-28)


### Bug Fixes

* Error in copy and paste menu when right clicking on a type without parameterless constructor ([dec40c4](https://github.com/fuqunaga/RosettaUI/commit/dec40c472f59c89697220b67ecca4b0fa1c3411f))

## [ga.fuquna.rosettaui-v1.7.3](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.7.2...ga.fuquna.rosettaui-v1.7.3) (2024-08-27)


### Bug Fixes

* Error when right-clicking on a nested list ([090bae6](https://github.com/fuqunaga/RosettaUI/commit/090bae6c9d86a16042ecf41653ce1dfe3e67b241))

## [ga.fuquna.rosettaui-v1.7.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.7.1...ga.fuquna.rosettaui-v1.7.2) (2024-08-27)


### Bug Fixes

* can not paste enum ([0a0e607](https://github.com/fuqunaga/RosettaUI/commit/0a0e607d1e53226067adb83d616cdeb70f355a7c))

## [ga.fuquna.rosettaui-v1.7.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.7.0...ga.fuquna.rosettaui-v1.7.1) (2024-08-23)


### Bug Fixes

* build error ([21ccc69](https://github.com/fuqunaga/RosettaUI/commit/21ccc695ceed5a62d6204d9f919be37b383b33b0))

## [ga.fuquna.rosettaui-v1.7.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.6.0...ga.fuquna.rosettaui-v1.7.0) (2024-08-22)


### Features

* Supports Unity6
* Right-click Copy and paste

## [ga.fuquna.rosettaui-v1.6.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.5.4...ga.fuquna.rosettaui-v1.6.0) (2024-02-28)


### Bug Fixes

* incorrect behavior of ListView when the reference of List is changed. ([454a185](https://github.com/fuqunaga/RosettaUI/commit/454a185d67769e34c20af1fface1e11ce9b3d36c))


### Features

* add UI.WindowLauncherTabs ([01e5ebc](https://github.com/fuqunaga/RosettaUI/commit/01e5ebcf29b5bd6cd3aa911fd33ce755b1d367dc))

## [ga.fuquna.rosettaui-v1.5.4](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.5.3...ga.fuquna.rosettaui-v1.5.4) (2024-02-22)


### Bug Fixes

* target unity version 2021.3 > 2022.3 ([2231b41](https://github.com/fuqunaga/RosettaUI/commit/2231b4170f04cf8ef1fa6e9673307a3781fc2b8d))

## [ga.fuquna.rosettaui-v1.5.3](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.5.2...ga.fuquna.rosettaui-v1.5.3) (2024-02-22)


### Bug Fixes

* add GradientHelper ([8906ba2](https://github.com/fuqunaga/RosettaUI/commit/8906ba23fd3ac12def54675b8a8cd6fc73e407e5))

## [ga.fuquna.rosettaui-v1.5.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.5.1...ga.fuquna.rosettaui-v1.5.2) (2023-12-27)


### Bug Fixes

* Null Excepion when IElementCreator returns null ([62f76a8](https://github.com/fuqunaga/RosettaUI/commit/62f76a8d496f8c5faeb49c6ccb65843cabb01734))

## [ga.fuquna.rosettaui-v1.5.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.5.0...ga.fuquna.rosettaui-v1.5.1) (2023-12-05)


### Bug Fixes

* Binder.Set() of IElementCreator's parent is not called ([bfc8604](https://github.com/fuqunaga/RosettaUI/commit/bfc8604d50ea3de1369e63ff03413a8b7ae16dfd))

## [ga.fuquna.rosettaui-v1.5.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.4.2...ga.fuquna.rosettaui-v1.5.0) (2023-09-11)


### Bug Fixes

* changed OrderBy() position ([68af2a9](https://github.com/fuqunaga/RosettaUI/commit/68af2a965106c328d861161dfc802b2759d214c3))
* fields with HideInInspector are no longer registered to the uiTargetDictionary ([beab198](https://github.com/fuqunaga/RosettaUI/commit/beab1985b856a913c2f22ab1c71a1bb658d478e0))


### Features

* support for header and tooltip attributes ([0422b4e](https://github.com/fuqunaga/RosettaUI/commit/0422b4e42f71adbc6e7a28a5b42aa7c78bf10455))
* support for HideInInspectorAttr, mutil attributes, attribute order ([16b9a76](https://github.com/fuqunaga/RosettaUI/commit/16b9a76f1a93db129bff6b726b000f8e5e243396))

## [ga.fuquna.rosettaui-v1.4.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.4.1...ga.fuquna.rosettaui-v1.4.2) (2023-06-21)


### Bug Fixes

* Changes to the value of items in the list were not notified outside the UI. ([306ea86](https://github.com/fuqunaga/RosettaUI/commit/306ea864348b18d3f331feeadfe5872740151fcb))

## [ga.fuquna.rosettaui-v1.4.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.4.0...ga.fuquna.rosettaui-v1.4.1) (2023-05-24)


### Bug Fixes

* Error when null is assigned to an element in UI.List() ([ce91f53](https://github.com/fuqunaga/RosettaUI/commit/ce91f534593eba5ed6db7257227b6135eda850b1))
* label error when assigning a new instance to IElementCreator ([c8beef7](https://github.com/fuqunaga/RosettaUI/commit/c8beef7963ad7a913cd1178ed820faaf970a5d2a))
* UI.List() element is an IElementCreator, it does not follow if the reference is changed. ([8021700](https://github.com/fuqunaga/RosettaUI/commit/802170036ad7cc15d06bc6d90ee19d75c20e7ec3))

## [ga.fuquna.rosettaui-v1.4.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.3.1...ga.fuquna.rosettaui-v1.4.0) (2023-05-22)


### Features

* UI.Dropdown() supports string value ([0a3b28e](https://github.com/fuqunaga/RosettaUI/commit/0a3b28e34aa355df500b066018eadf76a615ba85))

## [ga.fuquna.rosettaui-v1.3.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.3.0...ga.fuquna.rosettaui-v1.3.1) (2023-05-19)


### Bug Fixes

* Window generated outside of a window launcher reopens when RosettaUIRoot is enabled even if the window is closed. ([a72a7bf](https://github.com/fuqunaga/RosettaUI/commit/a72a7bf0f0c4afa324fc83fba1f146e9977dd642))

## [ga.fuquna.rosettaui-v1.3.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.2.0...ga.fuquna.rosettaui-v1.3.0) (2023-05-16)


### Features

* Suppression of keyboard events at TextField input is now supported for NewInputSystem and not for LegacyInputSystem ([c6aa7b9](https://github.com/fuqunaga/RosettaUI/commit/c6aa7b9fada0c7a3480b24ef36f08ada7f44e06e))

## [ga.fuquna.rosettaui-v1.3.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.2.0...ga.fuquna.rosettaui-v1.3.0) (2023-05-15)


### Features

* Suppression of keyboard events at TextField input is now supported for NewInputSystem and not for LegacyInputSystem ([c6aa7b9](https://github.com/fuqunaga/RosettaUI/commit/c6aa7b9fada0c7a3480b24ef36f08ada7f44e06e))

## [ga.fuquna.rosettaui-v1.2.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.3...ga.fuquna.rosettaui-v1.2.0) (2023-02-27)


### Bug Fixes

* Circuler reference guard does not work on non-development build ([7eb341c](https://github.com/fuqunaga/RosettaUI/commit/7eb341cea41f0bca75b42a150cd6cbd6e4ecf165))
* compile error when Api Compatibility Level == .NET framework ([5400b7b](https://github.com/fuqunaga/RosettaUI/commit/5400b7b6687649561c0635dc96f3c13f3364059d))


### Features

* add UI.Clickable() ([12459b6](https://github.com/fuqunaga/RosettaUI/commit/12459b698fe7faf9a8ac97a592a54fa417a916ca))

## [ga.fuquna.rosettaui-v1.1.3](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.2...ga.fuquna.rosettaui-v1.1.3) (2023-01-30)


### Bug Fixes

* Invalid cast exception when calling UI.Field() with a class that inherits T registered with UICustom.RegisterElementCreationFunc<T>(). ([431cceb](https://github.com/fuqunaga/RosettaUI/commit/431cceb9f956bde87b10033443c14fd19951576d))

## [ga.fuquna.rosettaui-v1.1.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.1...ga.fuquna.rosettaui-v1.1.2) (2023-01-27)


### Bug Fixes

* Fold and normal items did not align in a UI.List() without a header. ([63ba1c8](https://github.com/fuqunaga/RosettaUI/commit/63ba1c820978bf68d8187dc7f8315b9d22b83747))

## [ga.fuquna.rosettaui-v1.1.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.0...ga.fuquna.rosettaui-v1.1.1) (2023-01-27)


### Bug Fixes

* error passing class with public readonly field to UI.FIeld() ([6b4b3fe](https://github.com/fuqunaga/RosettaUI/commit/6b4b3fec02d8be777e5ef870876a1399476a5ba8))

## [ga.fuquna.rosettaui-v1.1.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.2...ga.fuquna.rosettaui-v1.1.0) (2023-01-24)


### Features

* [#6](https://github.com/fuqunaga/RosettaUI/issues/6) FieldOption.delayInput. This can delay updating the value in UI.Field(). ([508862f](https://github.com/fuqunaga/RosettaUI/commit/508862f5f3446b11e7f33de1d15f860b743b1ac2))
* Open/Close/SetOpenFlag methods can now be called on Elements, not just OpenClsoeBaseElement. ([cf9eabd](https://github.com/fuqunaga/RosettaUI/commit/cf9eabd2b9cf915082817868a551f5a428aca96d))
* WindowElement.SetClosable() ([82971ec](https://github.com/fuqunaga/RosettaUI/commit/82971ecf70246cdae12c7016d02d329acd236e85))

## [ga.fuquna.rosettaui-v1.0.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.1...ga.fuquna.rosettaui-v1.0.2) (2022-11-22)


### Bug Fixes

* version up manually to try to avoid npm publish error ([835205a](https://github.com/fuqunaga/RosettaUI/commit/835205acc2bbddcd7d57fd4979a46206093edf39))

## [ga.fuquna.rosettaui-v1.0.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.0...ga.fuquna.rosettaui-v1.0.1) (2022-11-22)


### Bug Fixes

* circular reference checks from type-based to reference-based [#11](https://github.com/fuqunaga/RosettaUI/issues/11) ([479bd7e](https://github.com/fuqunaga/RosettaUI/commit/479bd7e91a4239c867ea4c11b4d7fa31e5082169))
* List always pass through circular reference detection [#12](https://github.com/fuqunaga/RosettaUI/issues/12) ([991c90a](https://github.com/fuqunaga/RosettaUI/commit/991c90ae4b2aaf363f47e15f3c0cb3cc355251b6))


## [0.4.3] - 2022-10-11
### Changed
- The signature of UICustom.RegisterElementCreationFunc() / ElementCreationFuncScope() has changed  
Fix that ElementCreationFunc cannot follow when instance changes

## [0.4.2] - 2022-09-21
### Fixed
- No label composite field error.  
ex. `UI.Field(null, () => vector2);`

## [0.4.1] - 2022-07-11
### Changed
- ExpressionUtility.CreateLabelString() doesn't shows static property's class name

### Fixed
- UI.FieldIfObjectFound(), UI.DynamicElementIfObjectFound() Element is created and immediately destroyed repeatedly
- Indexer will be readonly and strange label. e.g. UI.Field(() => list[0])

## [0.4.0] - 2022-07-04
### Added
- UI.Toggle()
- UI.Tabs()
- UI.Lazy()
- UIEditor.ObjectField()

### Changed
- Class names are no longer displayed on static member label.  
e.g.  `UI.Field(() => staticMember)` label: `Class.staticMember` > `staticMember`

## [0.3.2] - 2022-06-03
### Added
- WindowElement.SetPosition()

### Changed
- WindowLauncher ignores the first indent to align the label(same as Fold)

## [0.2.2] - 2022-05-13
### Changed
- Element.Set* method for style supports null to set default value now.

## [0.1.2] - 2022-05-13
### Changed
- IElementCreater and UICustom.CreationFunc are now received a LabelElement.

## [0.0.0] - 2022-04-12
- 🎉 first release!
