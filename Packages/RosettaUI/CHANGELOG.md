# [ga.fuquna.rosettaui-v1.0.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.0...ga.fuquna.rosettaui-v1.0.1) (2022-11-22)


### Bug Fixes

* circular reference checks from type-based to reference-based [#11](https://github.com/fuqunaga/RosettaUI/issues/11) ([479bd7e](https://github.com/fuqunaga/RosettaUI/commit/479bd7e91a4239c867ea4c11b4d7fa31e5082169))
* List always pass through circular reference detection [#12](https://github.com/fuqunaga/RosettaUI/issues/12) ([991c90a](https://github.com/fuqunaga/RosettaUI/commit/991c90ae4b2aaf363f47e15f3c0cb3cc355251b6))

# [ga.fuquna.rosettaui-v1.0.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui-v1.0.0...ga.fuquna.rosettaui-v1.0.1) (2022-11-22)


### Bug Fixes

* circular reference checks from type-based to reference-based [#11](https://github.com/fuqunaga/RosettaUI/issues/11) ([479bd7e](https://github.com/fuqunaga/RosettaUI/commit/479bd7e91a4239c867ea4c11b4d7fa31e5082169))
* List always pass through circular reference detection [#12](https://github.com/fuqunaga/RosettaUI/issues/12) ([991c90a](https://github.com/fuqunaga/RosettaUI/commit/991c90ae4b2aaf363f47e15f3c0cb3cc355251b6))

# RosettaUI

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
