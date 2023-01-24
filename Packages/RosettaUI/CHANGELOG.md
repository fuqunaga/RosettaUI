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
