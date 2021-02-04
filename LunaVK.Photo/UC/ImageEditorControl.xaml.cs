//using ImageEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas;
using LunaVK.Photo.DrawingObjects;
using Windows.Storage;
using Microsoft.Graphics.Canvas.Effects;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using System.Numerics;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;

namespace LunaVK.Photo.UC
{
    public sealed partial class ImageEditorControl : UserControl
    {
        #region Fields
        /// <summary>
        /// Canvas background color (White)
        /// </summary>
        private Color _back_color = Colors.White;
        private Stretch _stretch = Stretch.Fill;  //Basemap picture filling method

        /// <summary>
        /// Canvas aspect ratio  0->1:1  1->4:3  2->3:4 3->Orinal
        /// </summary>
        private byte _size_mode = 3;
        private int _rotate = 0;   //Rotation degree of basemap image (360 degrees == 0 degrees)
        private int _pen_size = 2;   //Graffiti brush thickness
        private Color _pen_color = Colors.Orange;  //涂鸦画笔颜色
        private DoodleUI _current_editing_doodleUI;  //当前涂鸦对象
        private CanvasBitmap _image;  //Base map

        /// <summary>
        /// 0->None 1->Move 2->Zoom
        /// </summary>
        private byte _manipulation_type = 0;
        private IDrawingUI _current_ui;

        private bool IsSlideViewOpen;

        IDrawingUI _cropUI;//剪切UI
        List<IDrawingUI> _stickerUIs;
        List<IDrawingUI> _tagsUIs;  //Tags
        Stack<IDrawingUI> _doodleUIs;  //涂鸦


        public ObservableCollection<StickerPack> Items { get; private set; }//public ObservableCollection<string> WallPapers { get; private set; }
        public ObservableCollection<string> ColorsList { get; private set; }
        public ObservableCollection<int> ThicknessItems { get; private set; }

        public delegate void ImageEditedCompletedEventHandler(BitmapImage image);
        public event ImageEditedCompletedEventHandler ImageEditedCompleted;
        public Action<string> ImageEditCompleted;


        public int CropSizeMode
        {
            get
            {
                return this._size_mode;
            }
            set
            {
                this._size_mode = (byte)value;
                UpdateCropSize();

                //                this._btnCropSize.Flyout.Hide();

                MainCanvas.Invalidate();
                SetCanvas();

            }
        }

        private uint _mode;
        public uint EditMode
        {
            get
            {
                return this._mode;
            }
            set
            {
                if (value == this._mode)
                    value = 0;

                this._mode = value;

                if (this._cropUI != null)
                    this._cropUI = null;

                if (value == 0)
                {
                    this.ClearSelectedUI();



                }

                this.ShowBack(false);

                this._panelStickers.Visibility = Visibility.Collapsed;
                this._panelText.Visibility = Visibility.Collapsed;
                this._panelGraffiti.Visibility = Visibility.Collapsed;
                this._panelEffects.Visibility = Visibility.Collapsed;
                this._panelCrop.Visibility = Visibility.Collapsed;

                if (value == 1)//sticker
                {
                    this.ClearSelectedUI();

                    this.ShowBack(true);
                    this.LoadWallPapers();

                    this._panelStickers.Visibility = Visibility.Visible;
                }
                else if (value == 2)//text
                {
                    this.ShowBack(true);

                    if (this._current_ui == null)
                    {
                        if (_tagsUIs == null)
                            _tagsUIs = new List<IDrawingUI>();

                        IDrawingUI ui = new TextUI()
                        {
                            TagText = "Поделитесь скорее!",
                            X = 50,
                            Y = 50,
                            Font = (this._textFont.SelectedItem as ComboBoxItem).FontFamily.Source,
                            //FontSize = (uint)this._textSize.Value
                            Width = 200,
                            Height = 30
                        };

                        _tagsUIs.Add(ui);
                        this._current_ui = ui;
                        this._current_ui.Editing = true;

                    }

                    this.ActivateTextEdit();
                }
                else if (value == 3)//graffiti
                {
                    this.ClearSelectedUI();

                    if (this._doodleUIs == null)
                        this._doodleUIs = new Stack<IDrawingUI>();

                    this._panelGraffiti.Visibility = Visibility.Visible;
                    this.panelGraffitiAnimate.Begin();
                }
                else if (value == 4)
                {
                    this.ClearSelectedUI();

                    _cropUI = new CropUI() { Y = 20, X = 20, Height = 100, Width = 100, DrawColor = Colors.Orange };
                    this._current_ui = _cropUI;
                    this.SetCanvas();
                    this._panelCrop.Visibility = Visibility.Visible;
                }
                else if (value == 5)
                {
                    this.ClearSelectedUI();

                    this._panelEffects.Visibility = Visibility.Visible;
                    this.panelEffectsAnimate.Begin();
                }

                this.MainCanvas.Invalidate();
            }
        }

        private uint OutputWidth;
        //public uint OutputHeight;

        private double CurrenCanvasScale
        {
            get
            {
                return this.MainCanvas.ActualWidth / this.OutputWidth;
            }
        }

        AppBarButton barButtonCrop;
        AppBarButton barButtonEffects;
        #endregion


        private void UpdateCropSize()
        {
            CropUI ui = _cropUI as CropUI;
            if (this._size_mode == 0)//1 1
            {
                ui.Height = ui.Width;
            }
            else if (this._size_mode == 1) //4 3
            {
                ui.Height = ui.Width * 4 / 3;
            }
            else if (this._size_mode == 2) //3 4
            {
                ui.Height = ui.Width * 3 / 4;
            }
        }



        public ImageEditorControl()
        {
            this.Items = new ObservableCollection<StickerPack>();//this.WallPapers = new ObservableCollection<string>();
            this.ThicknessItems = new ObservableCollection<int>();
            this.ColorsList = new ObservableCollection<string>();

            base.DataContext = this;

            this.InitializeComponent();

            this.ThicknessItems.Add(26);
            this.ThicknessItems.Add(20);
            this.ThicknessItems.Add(12);
            this.ThicknessItems.Add(6);
            this.ThicknessItems.Add(2);

            base.Loaded += ImageEditorControl_Loaded;
            base.Unloaded += ImageEditorControl_Unloaded;

            this.ColorsList.Add("#ffe64646");
            this.ColorsList.Add("#fffe8d49");
            this.ColorsList.Add("#fff8d825");
            this.ColorsList.Add("#ff2cb946");
            this.ColorsList.Add("#ff4089e7");
            this.ColorsList.Add("#ff9b4beb");

            this.ColorsList.Add("#ff68e4b2");
            this.ColorsList.Add("#ffff84a0");
            this.ColorsList.Add("#fff6a877");
            this.ColorsList.Add("#fff7796b");
            this.ColorsList.Add("#ff8a3231");
            this.ColorsList.Add("#ff92b656");
            this.ColorsList.Add("#ff556e34");
            this.ColorsList.Add("#ff5a448f");

            this.ColorsList.Add("#ff000000");
            this.ColorsList.Add("#ff4d4d4d");
            this.ColorsList.Add("#ff666666");
            this.ColorsList.Add("#ff808080");
            this.ColorsList.Add("#ff999999");
            this.ColorsList.Add("#ffb3b3b3");
            this.ColorsList.Add("#ffffffff");

            this.barButtonCrop = new AppBarButton();
            this.barButtonCrop.Click += this.ButtonCrop_Click;
            this.barButtonCrop.Icon = new SymbolIcon(Symbol.Crop);
            this.barButtonCrop.Label = "Crop";

            this.barButtonEffects = new AppBarButton();
            this.barButtonEffects.Click += this.ButtonEffects_Click;
            this.barButtonEffects.Icon = new SymbolIcon(Symbol.Filter);
            this.barButtonEffects.Label = "Effects";
        }



        #region event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetCanvas();

            Window.Current.SizeChanged += Current_SizeChanged;

            this.OutputWidth = (uint)this.MainCanvas.ActualWidth;

#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#elif WINDOWS_UWP
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
#endif

        }

        private void ImageEditorControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
            this.MainCanvas.RemoveFromVisualTree();
            this.MainCanvas = null;

#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#elif WINDOWS_UWP
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
#endif
        }

        public async void LoadStorageFile(StorageFile image)
        {
            try
            {
                this.WaitLoading.IsActive = true;
                CanvasDevice cd = CanvasDevice.GetSharedDevice();
                var stream = await image.OpenAsync(FileAccessMode.Read);
                this._image = await CanvasBitmap.LoadAsync(cd, stream);
                this.WaitLoading.IsActive = false;
                this.MainCanvas.Invalidate();

                if (this._image != null)
                {
                    if (!this._cmdBar.PrimaryCommands.Contains(this.barButtonCrop))
                        this._cmdBar.PrimaryCommands.Add(this.barButtonCrop);

                    if (!this._cmdBar.PrimaryCommands.Contains(this.barButtonEffects))
                        this._cmdBar.PrimaryCommands.Add(this.barButtonEffects);
                    this.SetCanvas();
                }
            }
            catch
            {

            }
        }
        /*
        /// <summary>
        /// 显示编辑器（带local底图参数）
        /// </summary>
        /// <param name="image"></param>
        public async void Show(StorageFile image)
        {
            try
            {
                this.WaitLoading.IsActive = true;
                CanvasDevice cd = CanvasDevice.GetSharedDevice();
                var stream = await image.OpenAsync(FileAccessMode.Read);
                _image = await CanvasBitmap.LoadAsync(cd, stream);
                WaitLoading.IsActive = false;
                MainCanvas.Invalidate();
            }
            catch
            {

            }
        }

        
        /// <summary>
        /// 显示编辑器（带底图片url）
        /// </summary>
        /// <param name="uri"></param>
        public async void Show(Uri uri)
        {
            try
            {
                this.WaitLoading.IsActive = true;
                CanvasDevice cd = CanvasDevice.GetSharedDevice();
                _image = await CanvasBitmap.LoadAsync(cd, uri, 96);
                WaitLoading.IsActive = false;
                MainCanvas.Invalidate();
            }
            catch
            {

            }
        }
        */
        /// <summary>
        /// When the size of the PC window is changed, ensure the center display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.SetCanvas();
        }

        /// <summary>
        /// 画布绘制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            var target = GetDrawings(true, args.DrawingSession);  //
            /*
            if (target != null)
            {
                args.DrawingSession.Transform = Matrix3x2.CreateRotation((float)(this._rotate * Math.PI / 180), new Vector2((float)sender.ActualWidth, (float)sender.ActualHeight));
                args.DrawingSession.DrawImage(target);
            }
            */
        }

        private void ClearSelectedUI()
        {
            if (this._current_ui != null)
            {
                this._current_ui.Editing = false;
                this._current_ui = null;
            }
        }

        private Rect GetRegion(IDrawingUI ui)
        {
            return new Rect(ui.X * this.CurrenCanvasScale, ui.Y * this.CurrenCanvasScale, ui.Width * this.CurrenCanvasScale, ui.Height * this.CurrenCanvasScale);
        }

        private Rect GetCloseRegion(IDrawingUI ui)
        {
            return new Rect((ui.X - 8) * this.CurrenCanvasScale, (ui.Y - 8) * this.CurrenCanvasScale, 16, 16);
        }

        private Rect GetScaleRegion(IDrawingUI ui)
        {
            return new Rect((ui.X * this.CurrenCanvasScale) + (ui.Width * this.CurrenCanvasScale) - 8, (ui.Y * this.CurrenCanvasScale) + (ui.Height * this.CurrenCanvasScale) - 8, 16, 16);
        }

        private void MainCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;


            this._textText.Text = "";
            double sc0 = this.CurrenCanvasScale;

            Point p = e.GetPosition(MainCanvas);

            if (this._cropUI is CropUI crop)
            {
                if (this.GetCloseRegion(this._cropUI).Contains(p))  //cancel
                {
                    this.EditMode = 0;
                    MainCanvas.Invalidate();
                    return;
                }
                /*
                if (crop.RightTopRegion.Contains(p))  //OK
                {
                    //
                    //
                    //






                    if (this._image == null)
                        return;




                    Rect des = GetImageDrawingRect();
                    double scale = 1;
                    double sc = this._image.Bounds.Width / des.Width * scale;

                    Rect rect = crop.Region;

                    Rect rectScaled = new Rect((rect.X - des.X) * sc, (rect.Y - des.Y) * sc, rect.Width * sc, rect.Height * sc);


                    var croppedwidth = rect.Width * sc;
                    var croppedheight = rect.Height * sc;
                    CanvasDevice Cdevice = CanvasDevice.GetSharedDevice();
                    //create a new empty image that has the same size as the desired crop region
                    var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)croppedwidth, (int)croppedheight,
                        BitmapAlphaMode.Premultiplied);

                    //based on this empty software bitmap we create a new CanvasBitmap
                    var croppedimage = CanvasBitmap.CreateFromSoftwareBitmap(Cdevice, softwareBitmap);


                    croppedimage.CopyPixelsFromBitmap(_image, 0, 0, (int)rectScaled.Left, (int)rectScaled.Top, (int)rectScaled.Width, (int)rectScaled.Height);
                    this._image = croppedimage;










                    _cropUI = null;
                    this.SetCanvas();
                    MainCanvas.Invalidate();
                    //
                    //
                    //
                    return;
                }
                */

                if (this.GetRegion(this._cropUI).Contains(p))  //Click on the cut object area
                {
                    return;
                }
            }

            if (this._cropUI != null)
                this._cropUI = null;

            if (this._stickerUIs != null)
            {
                foreach (var ui in this._stickerUIs)
                {
                    if (ui.Editing && this.GetCloseRegion(ui).Contains(p)) //Cancel wallpaper
                    {
                        this._stickerUIs.Remove(ui);
                        if (this._stickerUIs.Count == 0)
                            this._stickerUIs = null;
                        this.MainCanvas.Invalidate();
                        this.EditMode = 0;
                        return;
                    }

                    if (this.GetRegion(ui).Contains(p)) //Click on the wallpaper
                    {
                        this.ClearSelectedUI();
                        this._current_ui = ui;
                        ui.Editing = true;
                        MainCanvas.Invalidate();
                        return;
                    }
                }
            }

            if (this._tagsUIs != null)
            {
                foreach (var ui in _tagsUIs)
                {
                    TextUI text = ui as TextUI;

                    if (ui.Editing && this.GetCloseRegion(ui).Contains(p)) //Cancel
                    {
                        this._panelText.Visibility = Visibility.Collapsed;
                        this._tagsUIs.Remove(ui);
                        if (this._tagsUIs.Count == 0)
                            this._tagsUIs = null;
                        this.MainCanvas.Invalidate();


                        //                        this._panelButtons.SelectedItem = null;
                        this.EditMode = 0;
                        this.ShowBack(false);

                        return;
                    }
                    if (this.GetRegion(ui).Contains(p))  //Clicked on the ui area
                    {
                        this.ClearSelectedUI();

                        this._current_ui = ui;
                        ui.Editing = true;

                        this.EditMode = 2;






                        return;
                    }
                }


            }

            if (this.EditMode == 3)
            {
                if (_doodleUIs == null)
                    _doodleUIs = new Stack<IDrawingUI>();

                _current_editing_doodleUI = new DoodleUI() { DrawingColor = _pen_color, DrawingSize = _pen_size };

                _current_editing_doodleUI.Points.Add(new Point(p.X / sc0, p.Y / sc0));




                _doodleUIs.Push(_current_editing_doodleUI);
                _current_editing_doodleUI = null;
                MainCanvas.Invalidate();
                this.UpdateUndoOpacity();
            }

            if (this.EditMode == 5)
            {
                this.EditMode = 0;
            }

            if (this._current_ui != null)
            {
                this._current_ui.Editing = false;
                this._current_ui = null;

                this.EditMode = 0;

                this.MainCanvas.Invalidate();
            }
        }



        /// <summary>
        /// 涂鸦撤销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SymbolIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_doodleUIs != null && _doodleUIs.Count > 0)
            {
                _doodleUIs.Pop();  //删除最近一次涂鸦 立即重绘
                this.UpdateUndoOpacity();
                MainCanvas.Invalidate();
            }
        }
        /// <summary>
        /// 选择涂鸦画笔粗细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PenSize_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this._pen_size = (int)(sender as FrameworkElement).DataContext;

            this._btnThicknessItems.Flyout.Hide();
        }


        /// <summary>
        /// Operation canvas starts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            this.ClearSelectedUI();
            double sc0 = 1;// this.CurrenCanvasScale;

            Point pos = new Point(e.Position.X / sc0, e.Position.Y / sc0);

            if (this.EditMode == 3)  //Graffiti status
            {
                if (_current_editing_doodleUI == null)
                {
                    _current_editing_doodleUI = new DoodleUI() { DrawingColor = _pen_color, DrawingSize = _pen_size };//todo: alpha
                    _current_editing_doodleUI.InitImageBrush(this._image);  //It may be a picture brush that needs to be initialized in advance
                }
                return;
            }
            else if (this.EditMode == 4) //Crop
            {
                if (_cropUI != null)
                {
                    if (this.GetScaleRegion(_cropUI).Contains(pos))
                    {
                        _manipulation_type = 2;
                    }
                    else if (this.GetRegion(_cropUI).Contains(pos))
                    {
                        _manipulation_type = 1;
                    }

                    this._current_ui = _cropUI;
                }
                return;
            }





            if (this._stickerUIs != null)
            {
                foreach (var ui in this._stickerUIs)
                {
                    if (this.GetScaleRegion(ui).Contains(pos))
                    {
                        _manipulation_type = 2;
                        this._current_ui = ui;
                        this._current_ui.Editing = true;
                        goto work;
                    }
                    else if (this.GetRegion(ui).Contains(pos))
                    {
                        _manipulation_type = 1;
                        this._current_ui = ui;
                        this._current_ui.Editing = true;
                        goto work;

                    }
                }
            }
            if (_tagsUIs != null /*&& this._current_ui == null*/)
            {
                foreach (var ui in _tagsUIs)
                {
                    TextUI text = ui as TextUI;
                    if (this.GetScaleRegion(text).Contains(e.Position))
                    {
                        _manipulation_type = 2;
                        this._current_ui = ui;
                        this._current_ui.Editing = true;
                        goto work;
                    }
                    else if (this.GetRegion(ui).Contains(pos))
                    {
                        _manipulation_type = 1;
                        this._current_ui = ui;
                        this._current_ui.Editing = true;

                        goto work;
                    }
                }
            }

            if (this.EditMode == 2)
            {
                this.EditMode = 0;

                return;
            }

        work:
            this.MainCanvas.Invalidate();
        }

        /// <summary>
        /// While operating the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double sc0 = 1;// this.CurrenCanvasScale;

            var deltaX = e.Delta.Translation.X / sc0;
            var deltaY = e.Delta.Translation.Y / sc0;

            if (_manipulation_type == 2 && this._current_ui != null && !(this._current_ui is TextUI)) //Zoom
            {
                if (this._current_ui.X + this._current_ui.Width + deltaX < 0 || ((this._current_ui.X + this._current_ui.Width) * this.CurrenCanvasScale) + deltaX > this.MainCanvas.ActualWidth || this._current_ui.Width + deltaX < 50)
                {
                    return;
                }
                if (this._current_ui.Y + this._current_ui.Height + deltaY < 0 || ((this._current_ui.Y + this._current_ui.Height) * this.CurrenCanvasScale) + deltaY > this.MainCanvas.ActualHeight || this._current_ui.Height + deltaY < 50)
                {
                    return;
                }
            }

            deltaX /= this.CurrenCanvasScale;
            deltaY /= this.CurrenCanvasScale;

            if (this.EditMode == 3) //Graffiti status
            {
                if (_current_editing_doodleUI != null)
                {
                    _current_editing_doodleUI.Points.Add(new Point(e.Position.X / this.CurrenCanvasScale, e.Position.Y / this.CurrenCanvasScale));
                    this.MainCanvas.Invalidate();
                }
                return;
            }
            else if (this.EditMode == 4)  //Crop
            {
                if (this._cropUI != null)
                {
                    if (_manipulation_type == 1) //Move
                    {
                        if (this._cropUI.X + deltaX < 0 || ((this._cropUI.X + this._cropUI.Width) * this.CurrenCanvasScale) + deltaX > this.MainCanvas.ActualWidth)
                        {
                            return;
                        }
                        if (this._cropUI.Y + deltaY < 0 || ((this._cropUI.Y + this._cropUI.Height) * this.CurrenCanvasScale) + deltaY > this.MainCanvas.ActualHeight)
                        {
                            return;
                        }




                        this._cropUI.X += deltaX;
                        this._cropUI.Y += deltaY;
                    }
                    else if (_manipulation_type == 2) //Zoom
                    {
                        this._cropUI.Width += deltaX;

                        if (this._size_mode == 0)//1 1
                        {
                            this._cropUI.Height = this._cropUI.Width;
                        }
                        else if (this._size_mode == 1) //4 3
                        {
                            this._cropUI.Height = this._cropUI.Width * 4 / 3;
                        }
                        else if (this._size_mode == 2) //3 4
                        {
                            this._cropUI.Height = this._cropUI.Width * 3 / 4;
                        }
                        else
                        {
                            this._cropUI.Height += deltaY;
                        }
                    }

                    MainCanvas.Invalidate();
                }
                return;
            }




            if (this._current_ui is WallPaperUI wall)
            {
                if (this._manipulation_type == 1)
                {
                    this._current_ui.X += deltaX;
                    this._current_ui.Y += deltaY;
                }
                else if (_manipulation_type == 2)  //Zoom
                {
                    this._current_ui.Width += deltaX;
                    if (wall.Image != null)
                    {
                        this._current_ui.Height = wall.Image.Size.Height / wall.Image.Size.Width * wall.Width;
                    }
                }

                MainCanvas.Invalidate();
            }


            if (this._current_ui is TextUI text)
            {
                if (this._manipulation_type == 1)
                {
                    this._current_ui.X += deltaX;
                    this._current_ui.Y += deltaY;
                }
                else if (_manipulation_type == 2)  //Zoom
                {
                    this._current_ui.Width += deltaX;
                    this._current_ui.Height += deltaY;
                }

                MainCanvas.Invalidate();
            }
        }

        /// <summary>
        /// End of operation canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            this._manipulation_type = 0;

            if (this.EditMode == 3)//graffiti
            {
                if (_current_editing_doodleUI != null)
                {
                    _doodleUIs.Push(_current_editing_doodleUI);
                    _current_editing_doodleUI = null;
                    MainCanvas.Invalidate();
                    this.UpdateUndoOpacity();
                }
                return;
            }
        }

        private void UpdateUndoOpacity()
        {
            this.borderUndo.IsEnabled = _doodleUIs != null && _doodleUIs.Count > 0;
        }

        /// <summary>
        /// Layout command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayoutCommand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var layout_command = sender as GridViewItem;
            /*
            if (layout_command == LayoutCommand1) //White border padding
            {
                _stretch = (_stretch == Stretch.Uniform) ? Stretch.UniformToFill : Stretch.Uniform;
                (LayoutCommand1_Panel.Children[1] as TextBlock).Text = (_stretch == Stretch.Uniform) ? "filling" : "White border";
            }
            else if (layout_command == LayoutCommand2)  //Black background white background
            {
                _back_color = (_back_color == Colors.White) ? Colors.Black : Colors.White;
                (LayoutCommand2_Panel.Children[1] as TextBlock).Text = (_back_color == Colors.White) ? "Black background" : "White background";
            }
            else if (layout_command == LayoutCommand3)  //Canvas ratio
            {
                _size_mode = (_size_mode + 1) % 4;
                var t = "";
                if (_size_mode == 0)
                {
                    t = "1:1";
                }
                else if (_size_mode == 1)
                {
                    t = "4:3";
                }
                else if (_size_mode == 2)
                {
                    t = "3:4";
                }
                else
                {
                    t = "Original";
                }
                (LayoutCommand3_Panel.Children[1] as TextBlock).Text = t;
            }
            else */
            /*if (layout_command == LayoutCommand4)  //Spin
            {
                _rotate = (_rotate + 90) % 360;
            }
            else if (layout_command == LayoutCommand5)  //Cut
            {
                if (_cropUI == null)
                {
                    _cropUI = new CropUI() { Top = 20, Left = 20, Height = 100, Width = 100, DrawColor = Colors.Orange };
                }
            }
            else if (layout_command == LayoutCommand6)  //Reselect the base map
            {
                SelectImage();
            }
            
            MainCanvas.Invalidate();
            SetCanvas();*/
        }

        /// <summary>
        /// Slider的值发生变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MainCanvas.Invalidate();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainCanvas.Invalidate();
        }

        /*
        /// <summary>
        /// 选择墙纸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WallPapersList_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.ClearSelectedUI();
            this._panelStickers.Visibility = Visibility.Collapsed;

            //if (_wall_paperUI == null)
            //{
            WallPaperUI _wall_paperUI = new WallPaperUI() { Height = 100, Width = 100, Image = null, X = 150, Y = 150 };

            if (this._stickerUIs == null)
                this._stickerUIs = new List<IDrawingUI>();

            this._current_ui = _wall_paperUI;
            this._current_ui.Editing = true;

            this._stickerUIs.Add(_wall_paperUI);
            //}
            //else
            //{
            //    (_wall_paperUI as WallPaperUI).Image = null;
            //}

            this.MainCanvas.Invalidate();

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            var img = await CanvasBitmap.LoadAsync(device, new Uri(e.ClickedItem.ToString()));
            if (img != null)
            {

                //_wall_paperUI.Width = img.Size.Width;
                //_wall_paperUI.Height = img.Size.Height;
                _wall_paperUI.Image = img;

                this.MainCanvas.Invalidate();

            }
;
        }
        */

        /*
    /// <summary>
    /// 点击取消
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CancelBtn_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (popup != null)
        {
            popup.IsOpen = false;
        }
    }
    */

        /// <summary>
        /// 点击确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.GenerateResultImage();
        }
        #endregion



        #region methods
        /// <summary>
        /// Get drawing results
        /// </summary>
        /// <param name="edit">Whether to edit state, edit state requires drawing editing tool</param>
        /// <returns></returns>
        private CanvasRenderTarget GetDrawings(bool edit, CanvasDrawingSession graphic)
        {
            double w, h;  //Canvas size
            if (edit || this._image == null)  //Edit status
            {
                w = MainCanvas.ActualWidth;
                h = MainCanvas.ActualHeight;
            }
            else  //The final generated image has a certain scale
            {
                //Rect des = GetImageDrawingRect();

                //w = (this._image.Size.Width / des.Width) * MainCanvas.Width;
                //h = (this._image.Size.Height / des.Height) * MainCanvas.Height;
                w = this._image.Size.Width;
                h = this._image.Size.Height;
            }

            //double scale = edit ? 1 : w / MainCanvas.ActualWidth;  //scaling ratio
            //double scale = this.CurrenCanvasScale;
            double scale = edit ? this.CurrenCanvasScale : w / MainCanvas.ActualWidth;  //scaling ratio


            CanvasRenderTarget target = null;
            if (graphic == null)
            {
                CanvasDevice device = CanvasDevice.GetSharedDevice();
                target = new CanvasRenderTarget(device, (float)w, (float)h, 96);
                graphic = target.CreateDrawingSession();
            }

            using (CanvasDrawingSession graphics = graphic)
            {
                graphics.Clear(_back_color); //Draw background

                DrawBackImage(graphics, edit ? 1 : scale); //Draw a base map
                //Draw doodle
                if (_doodleUIs != null && _doodleUIs.Count > 0)
                {
                    //scale = this._image.Size.Width / MainCanvas.Width;
                    var list = _doodleUIs.ToList(); list.Reverse();
                    list.ForEach((d) =>
                    {
                        d.Draw(graphics, (float)scale);
                    });
                }

                if (_current_editing_doodleUI != null)
                {
                    _current_editing_doodleUI.Draw(graphics, (float)scale); //Graffiti object on top
                }
                //绘制贴图
                if (this._stickerUIs != null)
                {
                    //_wall_paperUI.Draw(graphics, (float)scale);
                    this._stickerUIs.ForEach((t) => { t.Draw(graphics, (float)scale); });
                }
                //绘制Tag
                if (_tagsUIs != null)
                {
                    this._tagsUIs.ForEach((t) => { t.Draw(graphics, (float)scale); });
                }
                //绘制Crop裁剪工具
                if (_cropUI != null && edit)
                {
                    _cropUI.Draw(graphics, (float)scale);
                }
            }

            return target;
        }

        /// <summary>
        /// Set canvas size ratio
        /// </summary>
        private void SetCanvas()
        {
            /*
            var w = MainWorkSapce.ActualWidth - 20;
            var h = MainWorkSapce.ActualHeight - 20;

            if (this.IsSlideViewOpen)
                w -= 300;

            if (this._rotate == 90)
            {
                w = MainWorkSapce.ActualHeight - 20;
                h = MainWorkSapce.ActualWidth - 20;
            }

            if (this._image != null)
            {
                var sc = this._image.Bounds.Height / this._image.Bounds.Width;

                var canvasW = w;
                var canvasH = canvasW * sc;

                if (this._rotate == 90)
                {
                    sc = this._image.Bounds.Width / this._image.Bounds.Height;
                    canvasW = h;
                    canvasH = canvasW * sc;
                }

                if (canvasH > h)
                {
                    sc = this._image.Bounds.Width / this._image.Bounds.Height;

                    canvasH = h;
                    canvasW = canvasH * sc;
                }
                MainCanvas.Width = canvasW;
                MainCanvas.Height = canvasH;
            }
            else
            {
                if (w <= h)
                {
                    MainCanvas.Width = w;
                    MainCanvas.Height = MainCanvas.Width * 3 / 4;
                }
                else
                {
                    if (w / h <= (double)4 / 3)
                    {
                        MainCanvas.Width = w;
                        MainCanvas.Height = MainCanvas.Width * 3 / 4;
                    }
                    else
                    {
                        MainCanvas.Height = h;
                        MainCanvas.Width = MainCanvas.Height * 4 / 3;
                    }
                }
            }
            */

            var w = this.MainWorkSapce.ActualWidth - this.MainCanvas.Margin.Left - this.MainCanvas.Margin.Right;
            var h = this.MainWorkSapce.ActualHeight - this.MainCanvas.Margin.Left - this.MainCanvas.Margin.Right;


            if (this._rotate == 90)
            {
                w = this.MainWorkSapce.ActualHeight;
                h = this.MainWorkSapce.ActualWidth;
            }

            if (this._image != null)
            {
                if (w >= h)//Horizontal
                {
                    var ratio = this._image.Bounds.Height / this._image.Bounds.Width;

                    var canvasW = w;
                    var canvasH = canvasW * ratio;
                    /*
                    if (this._rotate == 90)
                    {
                        sc = this._image.Bounds.Width / this._image.Bounds.Height;
                        canvasW = h;
                        canvasH = canvasW * sc;
                    }
                    */
                    double multiplier = 1;
                    if (canvasW > w)
                    {
                        multiplier = w / canvasW;
                    }
                    if (canvasH > h)
                    {
                        multiplier = h / canvasH;
                    }

                    this.MainCanvas.MaxWidth = canvasW * multiplier;
                    this.MainCanvas.MaxHeight = canvasH * multiplier;
                }
                else//Vertical
                {
                    var ratio = this._image.Bounds.Width / this._image.Bounds.Height;

                    var canvasH = h;
                    var canvasW = canvasH * ratio;

                    double multiplier = 1;
                    if (canvasW > w)
                    {
                        multiplier = w / canvasW;
                    }
                    if (canvasH > h)
                    {
                        multiplier = h / canvasH;
                    }

                    this.MainCanvas.MaxWidth = canvasW * multiplier;
                    this.MainCanvas.MaxHeight = canvasH * multiplier;
                }
            }
            else
            {
                if (w <= h)
                {
                    this.MainCanvas.MaxHeight = MainCanvas.ActualWidth * 3 / 4;
                }
                else
                {
                    if (w / h <= (double)4 / 3)
                    {
                        this.MainCanvas.MaxHeight = MainCanvas.ActualWidth * 3 / 4;
                    }
                    else
                    {
                        this.MainCanvas.MaxWidth = MainCanvas.ActualHeight * 4 / 3;
                    }
                }
            }


            this.MainCanvas.Invalidate();
        }
        /// <summary>
        /// Draw a base map
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="scale"></param>
        private void DrawBackImage(CanvasDrawingSession graphics, double scale)
        {
            if (_image != null)
            {
                /*
                Rect des = GetImageDrawingRect();
                des.X *= scale;
                des.Y *= scale;
                des.Width *= scale;
                des.Height *= scale;
                */
                Rect des = new Rect(0, 0, this.MainCanvas.ActualWidth * scale, this.MainCanvas.ActualHeight * scale);

                ICanvasImage image = _image;
                #region Effects
                if (this.SliderBright.Value != 0)
                {
                    var t = SliderBright.Value;
                    image = new ExposureEffect
                    {
                        Source = image,
                        Exposure = (float)t
                    };
                }


                if (SliderSharpen.Value != 0)
                {
                    image = new SharpenEffect
                    {
                        Source = image,
                        Amount = (float)(SliderSharpen.Value)
                    };
                }

                if (this.SliderBlur.Value != 0)
                {
                    var t = SliderBlur.Value;// / 100 * 12;
                    image = new GaussianBlurEffect
                    {
                        Source = image,
                        BlurAmount = (float)t
                    };
                }




                if (this.SliderHueRotation.Value != 0)
                {
                    image = new HueRotationEffect
                    {
                        Source = image,
                        Angle = (float)SliderHueRotation.Value
                    };
                }

                if (this.SliderTemperature.Value != 0)
                {
                    image = new TemperatureAndTintEffect
                    {
                        Source = image,
                        Temperature = (float)SliderTemperature.Value,
                        Tint = 0.3f
                    };
                }

                if (this.SliderVignette.Value != 0)
                {
                    image = new VignetteEffect
                    {
                        Source = image,
                        Amount = (float)SliderVignette.Value,
                        //Color = Color.FromArgb(200, 0x0, 0x0, 0x0)
                    };
                }

                if (this.SliderEmboss.Value != 0)
                {
                    image = new EmbossEffect
                    {
                        Source = image,
                        Amount = (float)SliderEmboss.Value,
                    };
                }

                if (this.SliderSepia.Value != 0)
                {
                    image = new SepiaEffect
                    {
                        Source = image,
                        Intensity = (float)SliderSepia.Value,
                    };
                }

                if (this.BoxGrayScale.IsChecked.Value == true)
                {
                    image = new GrayscaleEffect() { Source = image };
                }

                if (this.BoxInvert.IsChecked.Value == true)
                {
                    image = new InvertEffect() { Source = image };
                }

                #endregion

                var sc = this._image.Bounds.Width / des.Width * scale;

                image = new Transform2DEffect
                {
                    Source = image,
                    TransformMatrix = Matrix3x2.CreateRotation((float)(this._rotate * Math.PI / 180), new Vector2((float)(des.Width / 2 * sc), (float)(des.Height / 2 * sc)))
                };


                //                if (this._cropUI == null)
                //                {
                Rect bounds = _image.Bounds;

                if (this._rotate == 90)
                {
                    bounds.Width = _image.Bounds.Height;
                    bounds.Height = _image.Bounds.Width;
                }
                graphics.DrawImage(image, des, bounds);
                /*                }
                                else
                                {

                                    CropEffect cropEffect = new CropEffect();
                                    cropEffect.Source = image;
                                    Rect rect = this.GetRegion(this._cropUI);



                                    cropEffect.SourceRectangle = new Rect(this._cropUI.X * sc, this._cropUI.Y * sc, this._cropUI.Width * sc, this._cropUI.Height * sc );

                                    graphics.DrawImage(cropEffect, des, _image.Bounds);
                                }
                                */
                //


            }
        }

        /// <summary>
        /// 异步从网络位置加载墙纸
        /// </summary>
        private async void LoadWallPapers()
        {
            /*
            if (this.WallPapers.Count > 0)
                return;
            
            this.WaitLoading.IsActive = true;

            var url = "http://files.cnblogs.com/files/xiaozhi_5638/Papers.zip" + "?t=" + DateTime.Now.Ticks;
            var json = await HttpTool.GetJson(url);
            this.WaitLoading.IsActive = false;
            if (json != null)
            {
                var papers = json["papers"].GetArray();
                var image_url = "";
                foreach (var paper in papers)
                {
                    var p = paper.GetObject();
                    image_url = p["image_url"].GetString();
                    if (!String.IsNullOrEmpty(image_url))
                    {
                        this.WallPapers.Add(image_url);
                    }
                }
            }
            */
            if (this.Items.Count > 0)
                return;

            var vkPack = new StickerPack();
            vkPack.preview = "https://vk.com/sticker/1-3200-64-9";
            List<VKSticker> vkStickers = new List<VKSticker>();
            //<img src="https://vk.com/sticker/1-3230-128-9">
            foreach (int id in ids)
            {
                var s = new VKSticker();
                s.images_with_background = new List<VKImageWithSize>();
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-64-9" });
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-128-9" });
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-256-9" });
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-352-9" });
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-512-9" });
                vkStickers.Add(s);
            }

            vkPack.stickers = vkStickers;
            vkPack.SelectionCallBack = SelectionCallBack;
            this.Items.Add(vkPack);



            List<StoreProductFilter> l = new List<StoreProductFilter>() { StoreProductFilter.Active };
            //l.Add(StoreProductFilter.Active);
            //List<StockItem> temp = StoreService.Instance.Stickers;


            StoreService.Instance.GetStockItems(l, (result) => {
                if (result != null && result.error.error_code == VKErrors.None)
                {
                    Core.Framework.Execute.ExecuteOnUIThread(() => { 
                    foreach (var pack in result.response.items)
                    {
                        var item = new StickerPack();
                        item.preview = pack.photo_35;
                        item.stickers = pack.product.stickers;
                        item.SelectionCallBack = SelectionCallBack;
                        this.Items.Add(item);
                    }
                    });
                }
            });
        }

        private async void SelectImage()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var image_file = await picker.PickSingleFileAsync();
            if (image_file != null)
            {
                this.LoadStorageFile(image_file);
            }
        }

        /// <summary>
        /// 生成最终结果
        /// </summary>
        private async void GenerateResultImage()
        {
            if (this._current_ui != null)
            {
                this._current_ui.Editing = false;
                this._current_ui = null;
                this.MainCanvas.Invalidate();
            }

            var img = GetDrawings(false, null);
            if (img != null)
            {
                IRandomAccessStream stream = new InMemoryRandomAccessStream();
                await img.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg);
                BitmapImage result = new BitmapImage();
                stream.Seek(0);
                await result.SetSourceAsync(stream);
                ImageEditedCompleted?.Invoke(result);
            }





            if (img != null)
            {
                string filename = "test1.png";
                StorageFolder pictureFolder = KnownFolders.SavedPictures;
                var file = await pictureFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);



                await img.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg);

                await fileStream.FlushAsync();
                fileStream.Dispose();

                this.ImageEditCompleted?.Invoke(filename);
            }
        }

        /*
        /// <summary>
        /// Basemap drawing area
        /// </summary>
        /// <returns></returns>
        private Rect GetImageDrawingRect()
        {
            Rect des;


            double image_w = MainCanvas.ActualWidth;
            double image_h = MainCanvas.ActualHeight;

            if (this._image != null)
            {
                image_w = _image.Size.Width;
                image_h = _image.Size.Height;

                if (this._rotate == 90)
                {
                    image_w = _image.Size.Height;
                    image_h = _image.Size.Width;
                }
                else
                {
                    image_w = _image.Size.Width;
                    image_h = _image.Size.Height;
                }
            }

            if (_stretch == Stretch.Uniform)
            {
                double w = MainCanvas.ActualWidth - 20;
                double h = MainCanvas.ActualHeight - 20;
                if (image_w / image_h > w / h)
                {
                    var left = 10;

                    var width = w;
                    var height = (image_h / image_w) * width;

                    var top = (h - height) / 2 + 10;

                    des = new Rect(left, top, width, height);
                }
                else
                {
                    var top = 10;
                    var height = h;
                    var width = (image_w / image_h) * height;
                    var left = (w - width) / 2 + 10;
                    des = new Rect(left, top, width, height);
                }
            }
            else
            {
                double w = MainCanvas.ActualWidth;
                double h = MainCanvas.ActualHeight;
                double left = 0;
                double top = 0;

                if (this._rotate == 90)
                {
                    w = MainCanvas.ActualHeight;
                    h = MainCanvas.ActualWidth;
                }

                if (image_w / image_h > w / h)
                {
                    var height = h;
                    var width = (image_w / image_h) * height;
                    des = new Rect(left, top, width, height);
                }
                else
                {
                    var width = w;
                    var height = (image_h / image_w) * width;

                    des = new Rect(left, top, width, height);
                }
            }

            return des;
        }
        */
        #endregion


        /*
        /// <summary>
        /// 选择滤镜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filters_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (var item in Filters.Items)
            {
                (((item as GridViewItem).Content as StackPanel).Children[1] as Border).Background = new SolidColorBrush(Colors.Transparent);
                ((((item as GridViewItem).Content as StackPanel).Children[1] as Border).Child as TextBlock).Foreground = new SolidColorBrush(Colors.Gray);
            }
            ((e.ClickedItem as StackPanel).Children[1] as Border).Background = new SolidColorBrush(Colors.Orange);
            (((e.ClickedItem as StackPanel).Children[1] as Border).Child as TextBlock).Foreground = new SolidColorBrush(Colors.White);

            _filter_index = int.Parse((e.ClickedItem as StackPanel).Tag.ToString());

            MainCanvas.Invalidate();
        }

        private void Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _filter_index = (sender as GridView).SelectedIndex;

            MainCanvas.Invalidate();
        }
        */
        private void CommonStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.NewState.Name);
        }

        private void PenColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string colorHex = e.AddedItems[0] as string;
            _pen_color = Color.FromArgb(Convert.ToByte(colorHex.Substring(1, 2), 16), Convert.ToByte(colorHex.Substring(3, 2), 16), Convert.ToByte(colorHex.Substring(5, 2), 16), Convert.ToByte(colorHex.Substring(7, 2), 16));
        }

        private void Clear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this._current_editing_doodleUI = null;

            this._doodleUIs.Clear();
            this._doodleUIs = null;
            this.UpdateUndoOpacity();
            MainCanvas.Invalidate();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _pen_color = Colors.Transparent;
        }

        private void Spin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _rotate = (_rotate + 90) % 360;
            MainCanvas.Invalidate();
            SetCanvas();
        }

        private void MainCommandPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_cropUI != null)
            {
                _cropUI = null;
                MainCanvas.Invalidate();
            }

            Pivot pivot = sender as Pivot;
            if (pivot.SelectedIndex == 3)
            {
                if (_cropUI == null)
                {
                    _cropUI = new CropUI() { Y = 20, X = 20, Height = 100, Width = 100, DrawColor = Colors.Orange };

                    if (this._current_ui != null)
                    {
                        this._current_ui.Editing = false;
                        this._current_ui = null;
                        this._current_ui = _cropUI;
                    }

                    MainCanvas.Invalidate();
                    SetCanvas();
                }
            }


        }

        private void OpenFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectImage();
        }

        private void ActivateTextEdit()
        {
            TextUI textUI = this._current_ui as TextUI;
            this._panelText.Visibility = Visibility.Visible;
            this.panelTextAnimate.Begin();

            this._textText.Text = textUI.TagText;
            //            this._textSize.Value = textUI.FontSize;

            object font = this._textFont.Items.FirstOrDefault((f) => ((ComboBoxItem)f).FontFamily.Source == textUI.Font);
            this._textFont.SelectedItem = font;
            string temp = textUI.DrawColor.ToString().ToLower();
            this._textColor.SelectedIndex = this.ColorsList.IndexOf(temp);
        }

        private void _tb_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (this._current_ui is TextUI text)
            {
                text.TagText = (sender as TextBox).Text;
                this.MainCanvas.Invalidate();
            }
        }

        private void _textFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._current_ui is TextUI text)
            {
                text.Font = ((sender as ComboBox).SelectedItem as ComboBoxItem).FontFamily.Source;
                this.MainCanvas.Invalidate();
            }
        }
        /*
        private void _textSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (this._current_ui is TextUI text)
            {
                text.FontSize = (uint)(sender as Slider).Value;
                this.MainCanvas.Invalidate();
            }
        }
        */
        private void _textColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            if (this._current_ui is TextUI text)
            {
                string colorHex = e.AddedItems[0] as string;
                text.DrawColor = Color.FromArgb(Convert.ToByte(colorHex.Substring(1, 2), 16), Convert.ToByte(colorHex.Substring(3, 2), 16), Convert.ToByte(colorHex.Substring(5, 2), 16), Convert.ToByte(colorHex.Substring(7, 2), 16));
                this.MainCanvas.Invalidate();
            }
        }

        private void ShowBack(bool show)
        {
            //this._darkBack.Opacity = show ? 1 : 0;
            if (show)
            {
                if (this._darkBack.Opacity == 0)
                    this.OverlayOpeningAnimation.Begin();
            }
            else
            {
                if (this._darkBack.Opacity > 0)
                    this.OverlayClosingAnimation.Begin();
            }
        }

        private async void _panelStickers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ClearSelectedUI();

            //            this._panelButtons.SelectedItem = null;
            this.EditMode = 0;

            this.ShowBack(false);
            this._panelStickers.Visibility = Visibility.Collapsed;
            this._panelText.Visibility = Visibility.Collapsed;

            if (this._stickerUIs == null)
                this._stickerUIs = new List<IDrawingUI>();


            WallPaperUI _wall_paperUI = new WallPaperUI() { Height = 100, Width = 100, Image = null, X = 150, Y = 150 };

            this._current_ui = _wall_paperUI;
            this._current_ui.Editing = true;

            this._stickerUIs.Add(_wall_paperUI);

            this.MainCanvas.Invalidate();

            CanvasDevice device = CanvasDevice.GetSharedDevice();

            string url = "";

            if (e.AddedItems[0] is VKSticker sticker)
            {
                url = sticker.photo_512;
            }
            else
            {
                url = e.AddedItems[0].ToString();
            }

            var img = await CanvasBitmap.LoadAsync(device, new Uri(url));
            if (img != null)
            {

                //_wall_paperUI.Width = img.Size.Width;
                //_wall_paperUI.Height = img.Size.Height;
                _wall_paperUI.Image = img;

                this.MainCanvas.Invalidate();
            }

        }

        private void _panelButtons_ItemClick(object sender, ItemClickEventArgs e)
        {
            ListView lv = sender as ListView;
            FrameworkElement selected = lv.SelectedItem as FrameworkElement;
            FrameworkElement clicked = e.ClickedItem as FrameworkElement;

            if (selected == null)
                return;

            if (selected.Name == clicked.Name)
            {
                lv.IsEnabled = false;
                lv.SelectedItem = null;
                lv.IsEnabled = true;

            }
        }

        private void ButtonStickers_Click(object sender, RoutedEventArgs e)
        {
            this.EditMode = 1;
        }

        private void ButtonText_Click(object sender, RoutedEventArgs e)
        {
            this.EditMode = 2;
        }

        private void ButtonGraffiti_Click(object sender, RoutedEventArgs e)
        {
            this.EditMode = 3;
        }

        private void ButtonCrop_Click(object sender, RoutedEventArgs e)
        {
            this.EditMode = 4;
        }

        private void ButtonEffects_Click(object sender, RoutedEventArgs e)
        {
            this.EditMode = 5;
        }

        private void ButtonSetBackground_Click(object sender, RoutedEventArgs e)
        {
            this.SelectImage();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.GenerateResultImage();
        }

        private void Cut_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this._image == null)
                return;

            double sc = this._image.Bounds.Width > this._image.Bounds.Height ? (this._image.Bounds.Width / this.MainCanvas.ActualWidth) : (this._image.Bounds.Height / this.MainCanvas.ActualHeight);
            sc *= this.CurrenCanvasScale;

            Rect rect = this.GetRegion(this._cropUI);

            //Rect rectScaled = new Rect((rect.X - des.X) * sc, (rect.Y - des.Y) * sc, rect.Width * sc, rect.Height * sc);

            int croppedwidth = (int)this._cropUI.Width;
            int croppedheight = (int)this._cropUI.Height;

            CanvasDevice Cdevice = CanvasDevice.GetSharedDevice();
            //create a new empty image that has the same size as the desired crop region
            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)(croppedwidth * sc), (int)(croppedheight * sc), BitmapAlphaMode.Premultiplied);

            //based on this empty software bitmap we create a new CanvasBitmap
            CanvasBitmap croppedimage = CanvasBitmap.CreateFromSoftwareBitmap(Cdevice, softwareBitmap);

            croppedimage.CopyPixelsFromBitmap(this._image, 0, 0, (int)(this._cropUI.X * sc), (int)(this._cropUI.Y * sc), (int)(croppedwidth * sc), (int)(croppedheight * sc));
            this._image = croppedimage;










            this.EditMode = 0;
            this.SetCanvas();
        }

        private void MainWorkSapce_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.EditMode = 0;
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
#elif WINDOWS_UWP
        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
#endif
            this.EditMode = 0;
        }



        public class StickerPack
        {
            public string preview { get; set; }

            public Action<string> SelectionCallBack;

            private VKSticker _selectedItem;
            public VKSticker SelectedItem
            {
                get
                {
                    return this._selectedItem;
                }
                set
                {
                    this._selectedItem = value;
                    if (SelectionCallBack != null)
                        SelectionCallBack(this._selectedItem.photo_256);
                }
            }

            public List<VKSticker> stickers { get; set; }
        }

        private static int[] ids = new int[] { 3230, 4025, 6106,3760,4023,3229,
        6105,6389,3231,6107,3998,3206,3996,5383,3997,5638,3218,3234,
        4260,6382,3200,4630,10695,6378,10696,10697,10698,4634,6384,
        5635,3759,4627,3767,5634,4633,4628,5379,4130,4262,5982,4065,
        4063,5979,3949,4264,3743,5981,3749,4027,3922,4632,4638,3241,
        5381,3746,6379,3756,3185,8438,4122,4131,8439,4701,6380,4124,
        3919,6387,4629,5645,4263,5386,6381,4637,6377,6390,4064,3758,
        3750,4265,3771,3768,5378,3177,3770,6105,4636,5380,5637,5633,
        5640,5646,5630,3217,5382,4631,5632,5451,3994,5385,4635,
        3222,3742,4356,4357,4358,4359,4266,4267,4268,3235,4269,4270,
        4271,4272,4273,4128,4123,4061,4062,6108,4066,4024,3239,4026,
        4028,3991,3992,3993,3995,5377,3947,3948,3950,3920,3921,3751,
        3753,3755,3757,3763,3765,3769,3772,3541,3175,3176,3178,3179,
        3180,3181,3182,3183,3184,3186,3188,3189,3190,3191,3192,3193,
        3194,3195,3196,3197,3198,3199,3201,3202,3203,3204,3205,3207,
        3208,3209,3210,3211,3212,3213,3214,3215,3216,3219,3220,3221,
        3223,3224,3225,3226,3227,3228,3232,3233,3236,3237,3238,3240,
        3442,3243,3244};

        private void SelectionCallBack(string selected)
        {
            //this.UpdateSource(selected);
        }
    }


}
