# [ga.fuquna.rosettaui.uitoolkit-v1.2.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.2.0...ga.fuquna.rosettaui.uitoolkit-v1.2.1) (2023-03-13)


### Bug Fixes

* Infinite width of PrefixLabel when closing and reopening a Window in Unity2022. ([230d510](https://github.com/fuqunaga/RosettaUI/commit/230d510e18af45f68f8adf3623036858e6544791))

# [ga.fuquna.rosettaui.uitoolkit-v1.2.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.1.1...ga.fuquna.rosettaui.uitoolkit-v1.2.0) (2023-02-27)


### Bug Fixes

* **uitoolkit:** DynamicElement now sends RequestResizeWindowEvent on rebuild ([8b64fb3](https://github.com/fuqunaga/RosettaUI/commit/8b64fb371ac1c972bcfd881d84bafee431c3ad14))


### Features

* add UI.Clickable() ([12459b6](https://github.com/fuqunaga/RosettaUI/commit/12459b698fe7faf9a8ac97a592a54fa417a916ca))

# [ga.fuquna.rosettaui.uitoolkit-v1.1.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.1.0...ga.fuquna.rosettaui.uitoolkit-v1.1.1) (2023-02-06)


### Bug Fixes

* Window width could not be reduced after toggling Fold On/Off. ([2786049](https://github.com/fuqunaga/RosettaUI/commit/27860491c0a2f6f651b6868ef6bc337eff50b8a8))

# [ga.fuquna.rosettaui.uitoolkit-v1.1.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.0.2...ga.fuquna.rosettaui.uitoolkit-v1.1.0) (2023-01-24)


### Bug Fixes

* Image is now aligned in the upper left corner. ([e2c8aa5](https://github.com/fuqunaga/RosettaUI/commit/e2c8aa5a249a50569bcaf448d83b57e51ba3bee6))
* ListView reorderable handle uss ([fc8d095](https://github.com/fuqunaga/RosettaUI/commit/fc8d0958ecc3140d4e8567494a91db071f84671e))


### Features

* [#6](https://github.com/fuqunaga/RosettaUI/issues/6) FieldOption.delayInput. This can delay updating the value in UI.Field(). ([508862f](https://github.com/fuqunaga/RosettaUI/commit/508862f5f3446b11e7f33de1d15f860b743b1ac2))
* WindowElement.SetClosable() ([82971ec](https://github.com/fuqunaga/RosettaUI/commit/82971ecf70246cdae12c7016d02d329acd236e85))

# [ga.fuquna.rosettaui.uitoolkit-v1.0.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.0.1...ga.fuquna.rosettaui.uitoolkit-v1.0.2) (2022-11-22)


### Bug Fixes

* follow Rosettaui 1.0.2 ([d92f6a3](https://github.com/fuqunaga/RosettaUI/commit/d92f6a33b7137f51e3abe43b3b2fed3380d664fd))

# [ga.fuquna.rosettaui.uitoolkit-v1.0.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.0.0...ga.fuquna.rosettaui.uitoolkit-v1.0.1) (2022-11-22)


### Bug Fixes

* [#2](https://github.com/fuqunaga/RosettaUI/issues/2), [#4](https://github.com/fuqunaga/RosettaUI/issues/4) Decimal points are not entered correctly when mouse dragging in some cultures ([17f7f7a](https://github.com/fuqunaga/RosettaUI/commit/17f7f7a0558ad6648ba65580f584753926c678e9))


### Performance Improvements

* Window now fixes style.width after initial layout to improve layout performance. [#5](https://github.com/fuqunaga/RosettaUI/issues/5) [#9](https://github.com/fuqunaga/RosettaUI/issues/9) [#13](https://github.com/fuqunaga/RosettaUI/issues/13) ([27564e1](https://github.com/fuqunaga/RosettaUI/commit/27564e17d4a58ac7554f63ec314c6dde83e7ce4d))