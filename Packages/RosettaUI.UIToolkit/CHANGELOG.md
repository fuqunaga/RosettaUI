# [ga.fuquna.rosettaui.uitoolkit-v1.4.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.4.0...ga.fuquna.rosettaui.uitoolkit-v1.4.1) (2023-06-13)


### Bug Fixes

* a bug that caused Elements in DynamicElement, etc. to shrink. ([5a4d374](https://github.com/fuqunaga/RosettaUI/commit/5a4d37497cbb7a5653b90c69ad3b5658de96f0c3))

# [ga.fuquna.rosettaui.uitoolkit-v1.4.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.3.1...ga.fuquna.rosettaui.uitoolkit-v1.4.0) (2023-05-24)


### Bug Fixes

* **dependency:** RosettaUI 1.3.1 ([6c29282](https://github.com/fuqunaga/RosettaUI/commit/6c29282329ef612fbb5b32800424c91e635b66f9))
* UI.Dropdown would not open when clicked in Unity2021 ([605dc8b](https://github.com/fuqunaga/RosettaUI/commit/605dc8bd4733841d0e2bd02c5099e682db6a3a7d))


### Features

* **design:** Now highlights when a button is pressed ([61ce7ef](https://github.com/fuqunaga/RosettaUI/commit/61ce7efbc4979236981d58462b07e7429f8cb7eb))

# [ga.fuquna.rosettaui.uitoolkit-v1.3.1](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.3.0...ga.fuquna.rosettaui.uitoolkit-v1.3.1) (2023-05-19)


### Bug Fixes

* **dependency:** RosettaUI 1.3.1 ([fb64287](https://github.com/fuqunaga/RosettaUI/commit/fb642874fa51b9c6a23d00e47ea388f940960a59))

# [ga.fuquna.rosettaui.uitoolkit-v1.3.0](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.2.2...ga.fuquna.rosettaui.uitoolkit-v1.3.0) (2023-05-16)


### Features

* **UIToolkit:** depends RosettaUI 1.3.0 ([380b105](https://github.com/fuqunaga/RosettaUI/commit/380b1057b83ee83563fa5eaacd82d1d563d44dd0))

# [ga.fuquna.rosettaui.uitoolkit-v1.2.2](https://github.com/fuqunaga/RosettaUI/compare/ga.fuquna.rosettaui.uitoolkit-v1.2.1...ga.fuquna.rosettaui.uitoolkit-v1.2.2) (2023-03-14)


### Bug Fixes

* **Unity2022:** "List is empty" is not displayed in UI.List() at Unity2022 ([c37cf2c](https://github.com/fuqunaga/RosettaUI/commit/c37cf2cb0ef185c1659810eb5c2b8a596059548e))
* **Unity2022:** Supports suppression of the Input class when entering text in RosettaUI at Unity2022 ([a626134](https://github.com/fuqunaga/RosettaUI/commit/a626134d8e908a239d9381d77b10b19f446c2dc6))
* When adding an element to an empty list in UI.List(), the field only shows a label ([cde3f7e](https://github.com/fuqunaga/RosettaUI/commit/cde3f7e41135954a2e251a66bb7e8c717f70b9b2))
* Window's Close button remains when hidden from RosettaUIRoot ([cafe974](https://github.com/fuqunaga/RosettaUI/commit/cafe97479bc907e5ee3237cb501de903d374ec71))

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
