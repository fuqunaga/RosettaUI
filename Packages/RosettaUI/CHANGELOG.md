# [ga.fuquna.rosettaui-v1.3.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.2.0...ga.fuquna.rosettaui-v1.3.0) (2023-05-15)


### Features

* Suppression of keyboard events at TextField input is now supported for NewInputSystem and not for LegacyInputSystem ([c6aa7b9](https://github.com/fuqunaga/RosettaUI/commit/c6aa7b9fada0c7a3480b24ef36f08ada7f44e06e))

# [ga.fuquna.rosettaui-v1.2.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.3...ga.fuquna.rosettaui-v1.2.0) (2023-02-27)


### Bug Fixes

* Circuler reference guard does not work on non-development build ([7eb341c](https://github.com/fuqunaga/RosettaUI/commit/7eb341cea41f0bca75b42a150cd6cbd6e4ecf165))
* compile error when Api Compatibility Level == .NET framework ([5400b7b](https://github.com/fuqunaga/RosettaUI/commit/5400b7b6687649561c0635dc96f3c13f3364059d))


### Features

* add UI.Clickable() ([12459b6](https://github.com/fuqunaga/RosettaUI/commit/12459b698fe7faf9a8ac97a592a54fa417a916ca))

# [ga.fuquna.rosettaui-v1.1.3](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.2...ga.fuquna.rosettaui-v1.1.3) (2023-01-30)


### Bug Fixes

* Invalid cast exception when calling UI.Field() with a class that inherits T registered with UICustom.RegisterElementCreationFunc<T>(). ([431cceb](https://github.com/fuqunaga/RosettaUI/commit/431cceb9f956bde87b10033443c14fd19951576d))

# [ga.fuquna.rosettaui-v1.1.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.1...ga.fuquna.rosettaui-v1.1.2) (2023-01-27)


### Bug Fixes

* Fold and normal items did not align in a UI.List() without a header. ([63ba1c8](https://github.com/fuqunaga/RosettaUI/commit/63ba1c820978bf68d8187dc7f8315b9d22b83747))

# [ga.fuquna.rosettaui-v1.1.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.1.0...ga.fuquna.rosettaui-v1.1.1) (2023-01-27)


### Bug Fixes

* error passing class with public readonly field to UI.FIeld() ([6b4b3fe](https://github.com/fuqunaga/RosettaUI/commit/6b4b3fec02d8be777e5ef870876a1399476a5ba8))

# [ga.fuquna.rosettaui-v1.1.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.2...ga.fuquna.rosettaui-v1.1.0) (2023-01-24)


### Features

* [#6](https://github.com/fuqunaga/RosettaUI/issues/6) FieldOption.delayInput. This can delay updating the value in UI.Field(). ([508862f](https://github.com/fuqunaga/RosettaUI/commit/508862f5f3446b11e7f33de1d15f860b743b1ac2))
* Open/Close/SetOpenFlag methods can now be called on Elements, not just OpenClsoeBaseElement. ([cf9eabd](https://github.com/fuqunaga/RosettaUI/commit/cf9eabd2b9cf915082817868a551f5a428aca96d))
* WindowElement.SetClosable() ([82971ec](https://github.com/fuqunaga/RosettaUI/commit/82971ecf70246cdae12c7016d02d329acd236e85))

# [ga.fuquna.rosettaui-v1.0.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.1...ga.fuquna.rosettaui-v1.0.2) (2022-11-22)


### Bug Fixes

* version up manually to try to avoid npm publish error ([835205a](https://github.com/fuqunaga/RosettaUI/commit/835205acc2bbddcd7d57fd4979a46206093edf39))

# [ga.fuquna.rosettaui-v1.0.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.0...ga.fuquna.rosettaui-v1.0.1) (2022-11-22)


### Bug Fixes

* circular reference checks from type-based to reference-based [#11](https://github.com/fuqunaga/RosettaUI/issues/11) ([479bd7e](https://github.com/fuqunaga/RosettaUI/commit/479bd7e91a4239c867ea4c11b4d7fa31e5082169))
* List always pass through circular reference detection [#12](https://github.com/fuqunaga/RosettaUI/issues/12) ([991c90a](https://github.com/fuqunaga/RosettaUI/commit/991c90ae4b2aaf363f47e15f3c0cb3cc355251b6))


# [0.4.3] - 2022-10-11
### Changed
- The signature of UICustom.RegisterElementCreationFunc() / ElementCreationFuncScope() has changed  
Fix that ElementCreationFunc cannot follow when instance changes

# [0.4.2] - 2022-09-21
### Fixed
- No label composite field error.  
ex. `UI.Field(null, () => vector2);`

# [0.4.1] - 2022-07-11
### Changed
- ExpressionUtility.CreateLabelString() doesn't shows static property's class name

### Fixed
- UI.FieldIfObjectFound(), UI.DynamicElementIfObjectFound() Element is created and immediately destroyed repeatedly
- Indexer will be readonly and strange label. e.g. UI.Field(() => list[0])

# [0.4.0] - 2022-07-04
### Added
- UI.Toggle()
- UI.Tabs()
- UI.Lazy()
- UIEditor.ObjectField()

### Changed
- Class names are no longer displayed on static member label.  
e.g.  `UI.Field(() => staticMember)` label: `Class.staticMember` > `staticMember`

# [0.3.2] - 2022-06-03
### Added
- WindowElement.SetPosition()

### Changed
- WindowLauncher ignores the first indent to align the label(same as Fold)

# [0.2.2] - 2022-05-13
### Changed
- Element.Set* method for style supports null to set default value now.

# [0.1.2] - 2022-05-13
### Changed
- IElementCreater and UICustom.CreationFunc are now received a LabelElement.

# [0.0.0] - 2022-04-12
- 🎉 first release!
