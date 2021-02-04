using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class GameViewUC : UserControl
    {
        public GameViewUC()
        {
            this.InitializeComponent();
        }

        private GameViewModel VM
        {
            get
            {
                return base.DataContext as GameViewModel;
            }
        }

        private void MoreActions_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //if (this.VM.Game.author_group == 0)
            //    return;

            PopUP2 menu = new PopUP2();

            PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = "Перейти в группу" };
            item.Command = new DelegateCommand((args) => {
                //this.ChangePhoto_OnClicked();

                if (this.VM.Game.author_group == 0)
                {
                    NavigatorImpl.Instance.NavigateToWebUri(this.VM.Game.author_url);
                }
                else
                {
                    NavigatorImpl.Instance.NavigateToProfilePage(-this.VM.Game.author_group);
                }
            });
            menu.Items.Add(item);


            

            menu.ShowAt(sender as FrameworkElement);
        }
        //Navigator.Current.NavigateToGameSettings(this.GameHeader.Game.id);
    }
}
