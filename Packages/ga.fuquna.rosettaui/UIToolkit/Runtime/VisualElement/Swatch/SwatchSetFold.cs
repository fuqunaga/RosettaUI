using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Swatch;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public abstract class SwatchSetFold<TValue, TSwatch> : Foldout
        where TSwatch : SwatchBase<TValue>, new()
    {
        // private const string PersistantKeyLayout = "Layout";
        private const string PersistantKeyIsOpen = "IsOpen";
        
        private readonly SwatchSetMenuAndTileView<TValue, TSwatch> _swatchSetMenuAndTileView;
        
        public SwatchPersistentService<TValue> PersistentService => _swatchSetMenuAndTileView.PersistentService;
        

        protected SwatchSetFold(string label, Action<TValue> applyValueFunc, string dataKeyPrefix)
        {
            var toggle = this.Q<Toggle>();
            _swatchSetMenuAndTileView = new SwatchSetMenuAndTileView<TValue, TSwatch>(toggle, this, applyValueFunc, dataKeyPrefix);
            
            text = label;
            AddToClassList(SwatchSet.UssClassName);
            
            value = false;
            SetMenuVisible(false);
            this.RegisterValueChangedCallback(OnValueChanged);
            
            schedule.Execute(LoadStatus).ExecuteLater(32);
        }
        
        private void LoadStatus()
        {
            var isOpen = PersistentService.Get<bool>(PersistantKeyIsOpen);
            value = isOpen;
            
            _swatchSetMenuAndTileView.LoadStatus();
        }


        public void SetValue(TValue currentValue) => _swatchSetMenuAndTileView.SetValue(currentValue);

        private void SetMenuVisible(bool v) => _swatchSetMenuAndTileView.SetMenuButtonVisible(v);
        
        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            var isOpen = evt.newValue;
            SetMenuVisible(isOpen);
            
            PersistentService.Set(PersistantKeyIsOpen, isOpen);
        }
    }
}
