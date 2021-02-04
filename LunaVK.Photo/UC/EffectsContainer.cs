using LunaVK.Photo.ViewModels;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.Photo.UC
{
    public class EffectsContainer : ContainedShapeControlContainer
    {
        private List<SlideMenuItemBase> _menuItems;
        public override List<SlideMenuItemBase> MenuItems
        {
            get { return this._menuItems; }
        }

        public ICanvasEffect Effect;
        public object VM;
        public Action PropertyChanged;

        public EffectsContainer()
            : base(null,0,0)
        {
            this._menuItems = new List<SlideMenuItemBase>();
            this._menuItems.Add(new SlideMenuItemBase()
            {
                Name = "Blur",
                IconPath = "ms-appx:///Assets/PhotoEditor/text-edit.png",
                SecondaryControlDataTemplate = (DataTemplate)Application.Current.Resources["BlurSelectorTemplate"],
                GetDataContextFunc = (() =>
                {
                    if (this.Effect is GaussianBlurEffect)
                    {
                        return this.VM;
                    }
                    else
                    { 
                        var vm = new BlurEffectViewModel(0,10) { Amount = 2 };
                        vm.PropertyChanged += this.PropertyChanged;
                        this.AddEffect(new GaussianBlurEffect() { BlurAmount = (float)vm.Amount }, vm);
                        return vm;
                    }
                }),
            });
            this._menuItems.Add(new SlideMenuItemBase()
            {
                Name = "Sepia",
                IconPath = "ms-appx:///Assets/PhotoEditor/text-edit.png",
                SecondaryControlDataTemplate = (DataTemplate)Application.Current.Resources["BlurSelectorTemplate"],
                GetDataContextFunc = (() =>
                {
                    if (this.Effect is SepiaEffect)
                    {
                        return this.VM;
                    }
                    else
                    {
                        var vm = new BlurEffectViewModel(0,1) { Amount = 1 };
                        vm.PropertyChanged += this.PropertyChanged;
                        this.AddEffect(new SepiaEffect() { Intensity = (float)vm.Amount }, vm);
                        return vm;
                    }
                }),
            });

            this._menuItems.Add(new SlideMenuItemBase()
            {
                Name = "Vignette",
                IconPath = "ms-appx:///Assets/PhotoEditor/text-edit.png",
                SecondaryControlDataTemplate = (DataTemplate)Application.Current.Resources["BlurSelectorTemplate"],
                GetDataContextFunc = (() =>
                {
                    if (this.Effect is VignetteEffect)
                    {
                        return this.VM;
                    }
                    else
                    {
                        var vm = new BlurEffectViewModel(0,1) { Amount = 0.5 };
                        vm.PropertyChanged += this.PropertyChanged;
                        this.AddEffect(new VignetteEffect() {  Amount = (float)vm.Amount }, vm);
                        return vm;
                    }
                }),
            });
        }

        private void AddEffect(ICanvasEffect effect, object vm)
        {
            this.Effect = effect;
            this.VM=vm;
            this.PropertyChanged?.Invoke();
        }

        
    }
}
