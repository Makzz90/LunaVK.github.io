﻿using System;
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

// Шаблон элемента пользовательского элемента управления задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234236
using LunaVK.Core.DataObjects;

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachGiftUC : UserControl
    {
        public AttachGiftUC()
        {
            this.InitializeComponent();
        }

        public AttachGiftUC(VKGift a) : this()
        {
            this.DataContext = a;
        }
    }
}
