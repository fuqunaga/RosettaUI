using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_ListViewItemContainer(Element element, VisualElement visualElement)
        {
            if (element is not ListViewItemContainerElement itemContainerElement ||
                visualElement is not ListView listView) return false;
            
            var option = itemContainerElement.option;           
            var viewBridge = itemContainerElement.GetViewBridge();
            var itemsSource = viewBridge.GetIList();

            var listViewController = new ListViewControllerCustom();
            listView.SetViewController(listViewController);
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight; //　これだけ定数
            listView.reorderable = option.reorderable;
            listView.reorderMode = option.reorderable ? ListViewReorderMode.Animated : ListViewReorderMode.Simple;
            listView.showAddRemoveFooter = !option.fixedSize;
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;

            // UnbindItem処理はしない
            // 
            // 要素がFoldなどサイズが変わるVisualElementの場合、
            // DynamicHeightVirtualizationController.OnScroll()で最後の要素が不必要にUnbind()されてしまうことがある@Unity2021.3.16f1
            // unbindItemされても表示されている状態になる
            // 
            // 上記挙動がなければ本来は念のためUnbind(Element)を呼んでおきたいが以下のように呼ばなくても大丈夫そう
            // ・VisualElementが再利用されている場合はBindItem()内のBind()がよばれる。Bind()は防衛的にUnbind()するので、ここでUnbind(Element)しなくても問題ない
            // ・VisualElementが再利用されない場合はElementは無効なVisualElementとBind()されたままになる。が表示されていないので問題ない・・・と思う
            // 
            // listView.unbindItem = UnbindItem;
            
#if UNITY_6000_0_OR_NEWER
            SuppressListViewStackOverFlow(listView);
#endif
            
            SetCallbacks();

            listView.itemsSource = itemsSource;
            listView.Rebuild();
            
            return true;
            
            
            #region Local functions

            void SetCallbacks()
            {
                // 参照か要素数がUIの外で変化した
                viewBridge.SubscribeListChanged(list =>
                {
                    listView.itemsSource = list;
                    listView.Rebuild();
                    
                    RequestResizeWindowEvent.Send(listView);
                });
                
                //　itemsRemoved は要素が削除される前に呼ばれるので注意@Unity6
                // 要素変化後の itemSource を参照するために itemsSourceSizeChanged を登録する
                // listView.itemsSourceSizeChangedはinternalなのでListViewControllerCustomに登録ｓ
                listView.itemsAdded += OnItemsAdded;
                listView.itemsRemoved += OnItemsRemoved;
                listView.itemIndexChanged += OnItemIndexChanged;
                listViewController.itemsSourceSizeChanged += OnItemsSourceSizeChanged;

                // ListView 内での参照先変更を通知
                listView.itemsSourceChanged += OnItemsSourceChanged;

                viewBridge.onUnsubscribe += () =>
                {
                    listView.itemsAdded -= OnItemsAdded;
                    listView.itemsRemoved -= OnItemsRemoved;
                    listView.itemIndexChanged -= OnItemIndexChanged;
                    listViewController.itemsSourceSizeChanged -= OnItemsSourceSizeChanged;
                    listView.itemsSourceChanged -= OnItemsSourceChanged;
                    
                    listView.itemsSource = Array.Empty<int>(); // null だとエラーになるので空配列で
                };
            }
            
            // 各要素ごとに空のVisualElementを作り、
            // その子供にElementの型に応じたVisualElementを生成する
            // Bind対象のElementが変わったら型ごとのVisualElementは非表示にして再利用を待つ
            VisualElement MakeItem()
            {
                var itemVe = new VisualElement();
                
                // リストの要素は見栄えを気にしてとりあえず強制インデント
                // marginでのインデントだとreorderable==falseのとき
                // 選択時の青色がmarginのスペースには反映されないのでpadding
                if (!option.suppressAutoIndent)
                {
                    ApplyIndent(itemVe, padding: true);
                }

                return itemVe;
            }
            
            void BindItem(VisualElement ve, int idx)
            {
                var e = viewBridge.GetOrCreateItemElement(idx);
                
                // Debug.Log($"Bind Idx[{idx}] FirstLabel[{e.FirstLabel()?.Value}]");
                
                e.SetEnable(true);
                e.Update();　// 表示前に最新の値をUIに通知

                var targetVe = ve.Children().FirstOrDefault();
                var success = Bind(e, targetVe);
                if (success) return;
                
                var itemVe = Build(e);
                ve.Add(itemVe);
            }

#if false
            void UnbindItem(VisualElement _, int idx)
            {
                var e = itemContainerElement.GetItemElementAt(idx);
                // Debug.Log($"Unbind Idx[{idx}] FirstLabel[{e?.FirstLabel()?.Value}]");
                if (e == null) return;
                
                // UnbindItemでVisualElementに影響を与えてはダメそう
                // 最後尾にスクロールしたときに隙間ができる（本来表示されるVisualElementが内容がない→height==0→非表示になっている
                // e.SetEnable(false);
                // Unbind(e);
            }
#endif

            // リストの最後への追加しかこないはず
            // 右クリックメニューはRosettaUI側が担当
            void OnItemsAdded(IEnumerable<int> idxes)
            {
                viewBridge.OnItemsAdded(idxes);
                RequestResizeWindowEvent.Send(listView);
            }
            
            // 複数選択できないので１つか、最後の要素の複数削除しかこないはず
            void OnItemsRemoved(IEnumerable<int> idxes)
            {
                viewBridge.OnItemsRemoved(idxes.OrderByDescending(i => i).ToArray().AsSpan());
            }

            // OnItemsRemoved()がlistView.itemsSource変更前に呼ばれてしまうので、
            // listView.itemsSource変更後のOnItemsSourceSizeChanged()で変更を通知する
            void OnItemsSourceSizeChanged()
            {
                OnViewListValueChanged();
            }
            
            void OnItemIndexChanged(int srcIdx, int dstIdx)
            {
                viewBridge.OnItemIndexChanged(srcIdx, dstIdx);
                OnViewListValueChanged();
                
                // ドラッグ中アニメーションでアイテムが移動中にドロップすると
                // コンテナのサイズがおかしくなる
                // 原因不明だがとりあえずリビルドで治るので対処療法
                // 副作用として（おそらく）１フレームのちらつきがある
                listView.Rebuild();
            }

            
            // List になにか変更があった場合の通知
            
            // 参照先変更
            void OnItemsSourceChanged() => viewBridge.OnViewListChanged(listView.itemsSource);
            // サイズ変更、アイテムの値変更
            void OnViewListValueChanged() => viewBridge.OnViewListValueChanged(listView.itemsSource);
            
            #endregion
        }

        // + ボタンで要素を追加したとき高確率でスタックオーバーフローする
        // 1. Scroller.OnSliderValueChange() で
        //  a. Slider.value の値を変更しNotifyする（ChangedEventを発行する）
        //  b. BaseVerticalCollectionView.OnScroll() が呼ばれSlider.highValueとSlider.valueをNotifyなしで更新する
        // 2.のChangedEventがDispatchされてScroller.OnSliderValueChange()が再度呼ばれる
        //　　再度、a.の処理が行われるが通常は1.と2. で同じ Slider.value となり、値が同じなのでNotifyが行われないが
        //   b. が常に a. と異なる値で更新する場合無限ループになる
        //
        // Unity6で発生。Unity2022では大丈夫
        // 現状Slider.highValueがかなりゼロに近い値で起こるっぽいのでそこだけイベントを止める
        private static void SuppressListViewStackOverFlow(ListView listView)
        {
            var scrollView = listView.Q<ScrollView>();
            var slider = scrollView.verticalScroller.slider;
            
            slider.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                if ((evt.previousValue == 0f || evt.newValue == 0f)
                    && Mathf.Abs(evt.previousValue - evt.newValue) < 0.0001f)
                {
                    // Debug.Log($"pre:{evt.previousValue} new:{evt.newValue} {evt.target.GetType()}");
                    if ( evt.target == slider)
                    {
                        // Debug.Log("StopImmediatePropagation");
                        evt.StopImmediatePropagation();
                    }
                }
            }, TrickleDown.TrickleDown);
        }
    }
}