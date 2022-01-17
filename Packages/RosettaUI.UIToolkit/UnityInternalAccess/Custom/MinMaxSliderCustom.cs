using UnityEngine.UIElements;

namespace UnityInternalAccess.Custom
{
    /// <summary>
    /// MinMaxSliderCustom
    /// 
    /// MinMaxSlider は dragElement の height が drag[Min/Max]Thumb の height になり
    /// クリックの反応範囲もこれに倣う
    /// Unity の default では drag[Min/Max]Thumb の minHeight を uss で設定して見かけ上の高さを出しているため見た目と反応範囲が一致していない
    ///
    /// 本クラスはこの問題を一致させつつ dragElement と drag[Min/Max]Thumb の間に表示用のエレメントを挿入して dragElement の
    /// 見た目も任意の高さにできるようにする
    /// </summary>
    public class MinMaxSliderCustom : MinMaxSlider
    {
        public MinMaxSliderCustom() : base()
        {
        }
    }
}