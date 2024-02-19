using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleCropper.Controls.Controls
{
    [TemplatePart(Name = CanvasTemplateName, Type = typeof(Canvas))]
    [TemplatePart(Name = RectangleLeftTemplateName, Type = typeof(System.Windows.Shapes.Rectangle))]
    [TemplatePart(Name = RectangleTopTemplateName, Type = typeof(System.Windows.Shapes.Rectangle))]
    [TemplatePart(Name = RectangleRightTemplateName, Type = typeof(System.Windows.Shapes.Rectangle))]
    [TemplatePart(Name = RectangleBottomTemplateName, Type = typeof(System.Windows.Shapes.Rectangle))]
    [TemplatePart(Name = BorderTemplateName, Type = typeof(Border))]

    public class CropViewer : Control
    {
        private const string CanvasTemplateName = "PART_Canvas";
        private const string RectangleLeftTemplateName = "PART_RectangleLeft";
        private const string RectangleTopTemplateName = "PART_RectangleTop";
        private const string RectangleRightTemplateName = "PART_RectangleRight";
        private const string RectangleBottomTemplateName = "PART_RectangleBottom";
        private const string BorderTemplateName = "PART_Border";

        #region Fields
        private Border _border;
        private Canvas _canvas;
        private System.Windows.Shapes.Rectangle _rectangleLeft, _rectangleTop, _rectangleRight, _rectangleBottom;

        private AdornerLayer _adornerLayer;

        private BitmapFrame _bitmapFrame;
        private bool _isDragging;
        private double _offsetX, _offsetY;
        private ScreenCutAdorner _screenCutAdorner;
        #endregion

        #region Dependency Properties
        /// <summary>Identifies the <see cref="ImageSource"/> dependency property.</summary>
        public static readonly DependencyProperty ImageSourceProperty
            = DependencyProperty.Register(nameof(ImageSource),
                                          typeof(BitmapSource),
                                          typeof(CropViewer),
                                          new PropertyMetadata(null, new PropertyChangedCallback(OnImageSourceChanged)));

        /// <summary>Identifies the <see cref="CurrentRect"/> dependency property.</summary>
        public static readonly DependencyProperty CurrentRectProperty
            = DependencyProperty.Register(nameof(CurrentRect),
                                          typeof(Rect),
                                          typeof(CropViewer),
                                          new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="CurrentAreaBitmap"/> dependency property.</summary>
        public static readonly DependencyProperty CurrentAreaBitmapProperty
            = DependencyProperty.Register(nameof(CurrentAreaBitmap),
                                          typeof(ImageSource),
                                          typeof(CropViewer),
                                          new PropertyMetadata(null));
        #endregion

        #region Dependency Property Callback
        private static void OnImageSourceChanged(DependencyObject property, DependencyPropertyChangedEventArgs e)
        {
            var crop = (CropViewer)property;

            if (crop != null)
                crop.DrawImage();
        }
        #endregion

        #region Properties
        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public Rect CurrentRect
        {
            get => (Rect)GetValue(CurrentRectProperty);
            set => SetValue(CurrentRectProperty, value);
        }

        public ImageSource CurrentAreaBitmap
        {
            get => (ImageSource)GetValue(CurrentAreaBitmapProperty);
            set => SetValue(CurrentAreaBitmapProperty, value);
        }
        #endregion

        #region Event
        public event RoutedEventHandler Cropped
        {
            add { AddHandler(CroppedEvent, value); }
            remove { RemoveHandler(CroppedEvent, value); }
        }

        public static RoutedEvent CroppedEvent
            = EventManager.RegisterRoutedEvent(nameof(Cropped),
                                               RoutingStrategy.Bubble,
                                               typeof(RoutedEventHandler),
                                               typeof(CropViewer));
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = GetTemplateChild(CanvasTemplateName) as Canvas;
            _rectangleLeft = GetTemplateChild(RectangleLeftTemplateName) as System.Windows.Shapes.Rectangle;
            _rectangleTop = GetTemplateChild(RectangleTopTemplateName) as System.Windows.Shapes.Rectangle;
            _rectangleRight = GetTemplateChild(RectangleRightTemplateName) as System.Windows.Shapes.Rectangle;
            _rectangleBottom = GetTemplateChild(RectangleBottomTemplateName) as System.Windows.Shapes.Rectangle;
            _border = GetTemplateChild(BorderTemplateName) as Border;
            DrawImage();
        }

        private void DrawImage()
        {
            if (ImageSource == null)
            {
                _border.Visibility = Visibility.Collapsed;
                if (_adornerLayer == null) return;
                _adornerLayer.Remove(_screenCutAdorner);
                _screenCutAdorner = null;
                _adornerLayer = null;
                return;
            }

            if (_border is null) return;

            try
            {
                _border.Visibility = Visibility.Visible;
                var bitmap = GetImage(ImageSource as BitmapImage);
                _bitmapFrame = CreateResizedImage(ImageSource, (int)ImageSource.Width, (int)ImageSource.Height, 0);
                _canvas.Width = ImageSource.Width;
                _canvas.Height = ImageSource.Height;
                _canvas.Background = new ImageBrush(ImageSource);
                _border.Width = ImageSource.Width * 0.2;
                _border.Height = ImageSource.Height * 0.2;
                var cx = _canvas.Width / 2 - _border.Width / 2;
                var cy = _canvas.Height / 2 - _border.Height / 2;
                Canvas.SetLeft(_border, cx);
                Canvas.SetTop(_border, cy);
                if (_adornerLayer != null) return;
                _adornerLayer = AdornerLayer.GetAdornerLayer(_border);
                _screenCutAdorner = new ScreenCutAdorner(_border);
                _adornerLayer.Add(_screenCutAdorner);
                _border.SizeChanged -= Border_SizeChanged;
                _border.SizeChanged += Border_SizeChanged;
                _border.MouseDown -= Border_MouseDown;
                _border.MouseDown += Border_MouseDown;
                _border.MouseMove -= Border_MouseMove;
                _border.MouseMove += Border_MouseMove;
                _border.MouseUp -= Border_MouseUp;
                _border.MouseUp += Border_MouseUp;
            }
            catch (Exception ex)
            {

            }

        }

        private ImageSource GetImage(BitmapImage bitmapImage)
        {
            //BitmapImage bitmapImage = new BitmapImage();
            //CroppedBitmap croppedBitmap;
            //try
            //{
            //    using (var stream = System.IO.File.OpenRead(path))
            //    {
            //        bitmapImage.BeginInit();
            //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //        bitmapImage.StreamSource = stream;
            //        bitmapImage.EndInit();

            //        Int32Rect sourceRect = new Int32Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight);
            //        croppedBitmap = new CroppedBitmap(bitmapImage, sourceRect);
            //    }

            //    return croppedBitmap;
            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}
            Int32Rect sourceRect = new Int32Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight);
            return new CroppedBitmap(bitmapImage, sourceRect);
        }

        private BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin)
        {
            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(group);
            }

            var resizedImage = new RenderTargetBitmap(
                width, height,
                96, 96,
                PixelFormats.Default);
            resizedImage.Render(drawingVisual);
            return BitmapFrame.Create(resizedImage);
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            var draggableControl = sender as UIElement;
            draggableControl.ReleaseMouseCapture();
            UpdateRoi();
        }

        private void UpdateRoi()
        {
            var width = _border.Width;
            var height = _border.Height;
            if (double.IsNaN(width) || double.IsNaN(height))
                return;
            var left = Canvas.GetLeft(_border);
            var top = Canvas.GetTop(_border);
            CurrentRect = new Rect(left, top, width, height);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
            {
                _isDragging = true;
                var draggableControl = sender as UIElement;
                var position = e.GetPosition(this);
                _offsetX = position.X - Canvas.GetLeft(draggableControl);
                _offsetY = position.Y - Canvas.GetTop(draggableControl);
                draggableControl.CaptureMouse();
            }
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var draggableControl = sender as UIElement;
                var position = e.GetPosition(this);
                var x = position.X - _offsetX;
                x = x < 0 ? 0 : x;
                x = x + _border.Width > _canvas.Width ? _canvas.Width - _border.Width : x;
                var y = position.Y - _offsetY;
                y = y < 0 ? 0 : y;
                y = y + _border.Height > _canvas.Height ? _canvas.Height - _border.Height : y;
                Canvas.SetLeft(draggableControl, x);
                Canvas.SetTop(draggableControl, y);
                Render();
            }
        }

        private void Render()
        {
            if (_border.ActualWidth < 100)
            {
                _border.Width = 100;
            }
            //else if (_border.ActualWidth > 500)
            //{
            //    _border.Width = 500;
            //}

            if (_border.ActualHeight < 100)
            {
                _border.Height = 100;
            }
            //else if (_border.ActualHeight > 500)
            //{
            //    _border.Height = 500;
            //}

            var cy = Canvas.GetTop(_border);
            cy = cy < 0 ? 0 : cy;
            var borderLeft = Canvas.GetLeft(_border);
            borderLeft = borderLeft < 0 ? 0 : borderLeft;
            _rectangleLeft.Width = borderLeft;
            _rectangleLeft.Height = _border.ActualHeight;
            Canvas.SetTop(_rectangleLeft, cy);

            _rectangleTop.Width = _canvas.Width;
            _rectangleTop.Height = cy;

            var rx = borderLeft + _border.ActualWidth;
            rx = rx > _canvas.Width ? _canvas.Width : rx;
            _rectangleRight.Width = _canvas.Width - rx;
            _rectangleRight.Height = _border.ActualHeight;
            Canvas.SetLeft(_rectangleRight, rx);
            Canvas.SetTop(_rectangleRight, cy);

            var by = cy + _border.ActualHeight;
            by = by < 0 ? 0 : by;
            _rectangleBottom.Width = _canvas.Width;
            var rby = _canvas.Height - by;
            _rectangleBottom.Height = rby < 0 ? 0 : rby;
            Canvas.SetTop(_rectangleBottom, by);
            UpdateRoi();
            var bitmap = CutBitmap();
            if (bitmap == null) return;
            var frame = BitmapFrame.Create(bitmap);
            CurrentAreaBitmap = frame;
            RaiseEvent(new RoutedEventArgs(CroppedEvent, null));
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Render();
        }

        private CroppedBitmap CutBitmap()
        {
            var width = _border.Width;
            var height = _border.Height;
            if (double.IsNaN(width) || double.IsNaN(height))
                return null;
            var left = Canvas.GetLeft(_border);
            var top = Canvas.GetTop(_border);
            CurrentRect = new Rect(left, top, width, height);
            return new CroppedBitmap(_bitmapFrame,
                new Int32Rect((int)CurrentRect.X, (int)CurrentRect.Y, (int)CurrentRect.Width,
                    (int)CurrentRect.Height));
        }
    }
}
