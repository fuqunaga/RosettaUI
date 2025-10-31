using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Swatch;
using UnityEngine;

namespace RosettaUI.Builder
{
    /// <summary>
    /// AnimationCurveSwatchのセーブロード
    /// データ自体がなければデフォルト値をセーブする
    /// </summary>
    public class AnimationCurveSwatchPersistentService : SwatchPersistentService<AnimationCurve>
    {
        public AnimationCurveSwatchPersistentService(string keyPrefix) : base(keyPrefix)
        {
        }

        public override IEnumerable<NameAndValue<AnimationCurve>> LoadSwatches()
        {
            if (!HasSwatches)
            {
                AddFactoryPreset();
            }

            return base.LoadSwatches();
        }

        public void AddFactoryPreset()
        {
            var swatches = base.LoadSwatches() ?? Array.Empty<NameAndValue<AnimationCurve>>();
            SaveSwatches(swatches.Concat(AnimationCurveHelper.FactoryPresets.Select(curve =>
                new NameAndValue<AnimationCurve> { value = curve })));
        }
    }
}