using LunaVK.Photo.ViewModels;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace LunaVK.Photo.UC
{
    public sealed partial class CollageCreationUC : UserControl
    {
        public bool IsSlideViewOpen;
        CollageController controller;
        public ObservableCollection<SlideMenuItemBase> SelectedShapeMenuItems { get; private set; }
        public Action<bool> InEditCallback;
        private CanvasControl canvasControl;
        //public Action Invalidated;

        public CollageCreationUC()
        {
            this.SelectedShapeMenuItems = new ObservableCollection<SlideMenuItemBase>();
            base.DataContext = this;
            this.InitializeComponent();
        }

        public List<ContainedShapeControlContainer> Shapes
        {
            get
            {
                return this.controller.Shapes;
            }
        }

        public void InitWith(Grid parentGrid)
        {
            this.controller = new CollageController(parentGrid);
            this.controller.ShapeSelected = this.ShapeSelected;
        }

        public void InitWithCanvas(Grid parentGrid, CanvasControl canvas)
        {
            this.InitWith(parentGrid);
            this.canvasControl = canvas;

            canvas.CreateResources += this.Canvas_CreateResources;
            canvas.Draw += this.Canvas_Draw;
        }

        private void AddStickerClick(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSlideView();
            //this._collageController.StartAddTextDialog();
            StickerOverlayShape shape = new StickerOverlayShape();

            this.controller.AddShape(shape);
        }


        private void AddTextClick(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSlideView();
            //this._collageController.SelectAddStickerMenu();

            TextOverlayShape shape = new TextOverlayShape();
            //var ad = shape.ParentContainer as AdornerElementBaseUC;

            this.controller.AddShape(shape);
            shape.Control.SizeChanged += Control_SizeChanged;
            (shape.ParentContainer as AdornerElementBaseUC)._elementPosition += this.MousePosition;
            (shape.ParentContainer as AdornerElementBaseUC)._mousePosition += this.MousePosition;
        }

        private void AddEffects(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSlideView();

            var exists = this.controller.Shapes.FirstOrDefault((s) => s is EffectsContainer);
            if (exists == null)
            {
                EffectsContainer shape = new EffectsContainer();
                shape.PropertyChanged += this.canvasControl.Invalidate;
                this.controller.AddShape(shape);
            }
            else
            {
                this.controller.OnSelected(exists.ParentContainer);
            }
        }

        private void Control_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this.Invalidated?.Invoke();
            this.canvasControl?.Invalidate();
        }

        private void MousePosition(Point pos)
        {
            //this.Invalidated?.Invoke();
            this.canvasControl?.Invalidate();
        }

        private void ShapeSelected(ContainedShapeControlContainer shape)
        {
            this.ThirdLevelMenu.ContentTemplate = null;
            this.ThirdLevelMenu.DataContext = null;

            this.SelectedShapeMenuItems.Clear();
            if (shape != null)
            {
                if (shape.MenuItems != null)
                {
                    foreach (var m in shape.MenuItems)
                    {
                        this.SelectedShapeMenuItems.Add(m);
                    }
                }
            }

            this.InEditCallback?.Invoke(shape != null);

            if (this.canvasControl != null)
                this.canvasControl.Invalidate();
        }

        public void ClearBoard()
        {
            this.ThirdLevelMenu.ContentTemplate = null;
            this.ThirdLevelMenu.DataContext = null;
            this.SelectedShapeMenuItems.Clear();
            this.controller.DeleteAll();
            this.CloseSlideView();
        }

        private void ToggleSlideViewClick(object sender, TappedRoutedEventArgs e)
        {
            this.controller.UnselectAll();

            if (!this.IsSlideViewOpen)
                this.ShowFlyout();
            else
                this.CloseSlideView();
        }


        /// <summary>
        /// Открыть меню
        /// </summary>
        public void ShowFlyout()
        {
            this.OpenAnimatoin.Begin();
            this._back.IsHitTestVisible = true;
            this.IsSlideViewOpen = true;
        }

        /// <summary>
        /// Закрыть меню
        /// </summary>
        public void CloseSlideView()
        {
            this.CloseAnimatoin.Begin();
            this._back.IsHitTestVisible = false;
            this.IsSlideViewOpen = false;
        }

        private void _back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSlideView();

            e.Handled = true;
        }

        private void MenuItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as SlideMenuItemBase;
            vm.ClickCommand?.Invoke();

            this.ThirdLevelMenu.ContentTemplate = vm.SecondaryControlDataTemplate;

            if (vm.GetDataContextFunc != null)
            {
                object viewModel = vm.GetDataContextFunc();
                this.ThirdLevelMenu.DataContext = viewModel;
            }
            else
            {
                this.ThirdLevelMenu.DataContext = null;
            }
        }

























        CanvasBitmap image;

        public async void LoadFromStream(IRandomAccessStream stream)
        {
            this.image = await CanvasBitmap.LoadAsync(this.canvasControl, stream);
            this.canvasControl.Invalidate();
        }

        public async void LoadFromPath(string path)
        {
            this.image = await CanvasBitmap.LoadAsync(this.canvasControl, new Uri(path));
        }

        private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            //args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private async Task CreateResourcesAsync(CanvasControl sender)
        {
            this.image = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/1-3330-256.png"));
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var session = args.DrawingSession)
            {
                /*
                var blur = new GaussianBlurEffect();
                blur.BlurAmount = 5.0f;
                blur.Source = image;

                session.DrawImage(blur, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight),
                    new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);
                    */
                //session.DrawImage(image, new Rect(0,0, sender.ActualWidth, sender.ActualHeight));

                //session.DrawText(lblName.Text, 150, 150, Colors.Black, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 40, FontWeight = Windows.UI.Text.FontWeights.Bold });


                Matrix3x2 str = session.Transform;

                bool useEffects = false;

                foreach (var shape in this.Shapes)
                {
                    /*
                    if (shape.Control is TextBox tb)
                    {
                        var adorner = shape.ParentContainer as AdornerElementBaseUC;
                        var angle = adorner.Rotation;

                        var startAngle = angle * Math.PI / 180;
                        var directionTransform = Matrix3x2.CreateRotation((float)startAngle);




                        var temp = tb.TransformToVisual(this.controller._parent).TransformPoint(new Point());

                        session.Transform = Matrix3x2.CreateRotation((float)startAngle, new Vector2((float)temp.X, (float)temp.Y));

                        session.DrawText(tb.Text, (float)temp.X, (float)temp.Y, Colors.Green, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = (float)tb.FontSize, FontWeight = tb.FontWeight, FontFamily = tb.FontFamily.Source });
                    }
                    else */if (shape is EffectsContainer effectsContainer)
                    {
                        session.Transform = str;


                        var eff = effectsContainer.Effect;



                        if (eff is GaussianBlurEffect blur)
                        {
                            var vm = effectsContainer.VM as BlurEffectViewModel;

                            if (vm.Amount == 0)
                            {
                                continue;
                            }

                            blur.BlurAmount = (float)vm.Amount;
                            blur.Source = image;

                            session.DrawImage(blur, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight),
                                new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);

                            useEffects = true;
                        }
                        else if (eff is SepiaEffect sepia)
                        {
                            var vm = effectsContainer.VM as BlurEffectViewModel;

                            if (vm.Amount == 0)
                            {
                                continue;
                            }

                            sepia.Intensity = (float)vm.Amount;//1 - max
                            sepia.Source = image;

                            session.DrawImage(sepia, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight),
                                new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);

                            useEffects = true;
                        }
                        else if (eff is VignetteEffect vignette)
                        {
                            var vm = effectsContainer.VM as BlurEffectViewModel;

                            if (vm.Amount == 0)
                            {
                                continue;
                            }

                            vignette.Amount = (float)vm.Amount;
                            vignette.Source = image;

                            session.DrawImage(vignette, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight),
                                new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);

                            useEffects = true;
                        }
                    }
                }

                if (!useEffects)
                {
                    session.Transform = str;

                    if(this.image!=null)
                        session.DrawImage(this.image, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight));
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private async void SaveTapped(object sender, TappedRoutedEventArgs e)
        {
            //Uri imageuri = new Uri("ms-appx:///Assets/HelloMyNameIs.jpg");
            //StorageFile inputFile = await StorageFile.GetFileFromApplicationUriAsync(imageuri);
            //BitmapDecoder imagedecoder;
            //using (var imagestream = await inputFile.OpenAsync(FileAccessMode.Read))
            //{
            //    imagedecoder = await BitmapDecoder.CreateAsync(imagestream);
            //}

            

            string filename = "test1.png";
            StorageFolder pictureFolder = KnownFolders.SavedPictures;
            var file = await pictureFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            
                await this.SaveToStream(fileStream);
            await fileStream.FlushAsync();
            fileStream.Dispose();


        }

        public async Task SaveToStream(IRandomAccessStream stream)
        {
            CanvasDevice device = CanvasDevice.GetSharedDevice();

            float dpi = image.Dpi;
            float w = (float)image.Bounds.Width;
            float h = (float)image.Bounds.Height;
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, w, h, dpi);
            using (var session = renderTarget.CreateDrawingSession())
            {
                Matrix3x2 str = session.Transform;

                session.Clear(Colors.White);
                //CanvasBitmap image = await CanvasBitmap.LoadAsync(device, inputFile.Path, 96);
                //ds.DrawImage(image);
                //ds.DrawText(lblName.Text, new System.Numerics.Vector2(150, 150), Colors.Black);




                var effectsContainer = this.Shapes.FirstOrDefault((s) => s is EffectsContainer) as EffectsContainer;

                if (effectsContainer == null)
                {
                    session.Transform = str;

                    if (this.image != null)
                        session.DrawImage(this.image, new Rect(0, 0, w, h));
                }
                else
                {
                    session.Transform = str;


                    var eff = effectsContainer.Effect;



                    if (eff is GaussianBlurEffect blur)
                    {
                        var vm = effectsContainer.VM as BlurEffectViewModel;

                        if (vm.Amount == 0)
                        {
                            //continue;
                        }

                        blur.BlurAmount = (float)vm.Amount;
                        blur.Source = image;

                        session.DrawImage(blur, new Rect(0, 0, image.Bounds.Width, image.Bounds.Height),
                            new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);

                    }
                    else if (eff is SepiaEffect sepia)
                    {
                        var vm = effectsContainer.VM as BlurEffectViewModel;

                        if (vm.Amount == 0)
                        {
                            //continue;
                        }

                        sepia.Intensity = (float)vm.Amount;//1 - max
                        sepia.Source = image;

                        session.DrawImage(sepia, new Rect(0, 0, image.Bounds.Width, image.Bounds.Height),
                            new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);

                    }
                    else if (eff is VignetteEffect vignette)
                    {
                        var vm = effectsContainer.VM as BlurEffectViewModel;

                        if (vm.Amount == 0)
                        {
                            //continue;
                        }

                        vignette.Amount = (float)vm.Amount;
                        vignette.Source = image;

                        session.DrawImage(vignette, new Rect(0, 0, image.Bounds.Width, image.Bounds.Height),
                            new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);

                    }
                }


                foreach (var shape in this.Shapes)
                {
                    if (shape.Control is TextBox tb)
                    {
                        var adorner = shape.ParentContainer as AdornerElementBaseUC;
                        var angle = adorner.Rotation;

                        var startAngle = angle * Math.PI / 180;
                        var directionTransform = Matrix3x2.CreateRotation((float)startAngle);


                        float scale = (float)this.image.Bounds.Width / (float)this.controller._parent.ActualWidth;

                        var temp = tb.TransformToVisual(this.controller._parent).TransformPoint(new Point());

                        session.Transform = Matrix3x2.CreateRotation((float)startAngle, new Vector2((float)temp.X, (float)temp.Y));

                        session.DrawText(tb.Text, (float)temp.X * scale, (float)temp.Y * scale, (tb.Foreground as SolidColorBrush).Color, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = (float)tb.FontSize * scale, FontWeight = tb.FontWeight, FontFamily = tb.FontFamily.Source });
                    }
                }

            }



            await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png, 1f);
        }
    }
}
