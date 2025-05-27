﻿using LabelSharp.Presentation.Maps;
using LabelSharp.Presentation.Models;
using LabelSharp.ViewModels.AnnotationContext;
using LabelSharp.ViewModels.CommonContext;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Models;
using SD.IOC.Core.Mediators;
using SD.OpenCV.Primitives.Extensions;
using SD.Toolkits.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace LabelSharp.ViewModels.HomeContext
{
    /// <summary>
    /// 首页视图模型 - Action部分
    /// </summary>
    public partial class IndexViewModel
    {
        #region # 字段及构造器

        //

        #endregion

        #region # 属性

        #region 鼠标X坐标 —— int? MousePositionX
        /// <summary>
        /// 鼠标X坐标
        /// </summary>
        [DependencyProperty]
        public int? MousePositionX { get; set; }
        #endregion

        #region 鼠标Y坐标 —— int? MousePositionY
        /// <summary>
        /// 鼠标Y坐标
        /// </summary>
        [DependencyProperty]
        public int? MousePositionY { get; set; }
        #endregion

        #region 已选图像标注 —— ImageAnnotation SelectedImageAnnotation
        /// <summary>
        /// 已选图像标注
        /// </summary>
        [DependencyProperty]
        public ImageAnnotation SelectedImageAnnotation { get; set; }
        #endregion

        #region 图像标注列表 —— ObservableCollection<ImageAnnotation> ImageAnnotations
        /// <summary>
        /// 图像标注列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<ImageAnnotation> ImageAnnotations { get; set; }
        #endregion

        #region 标签列表 —— ObservableCollection<string> Labels
        /// <summary>
        /// 标签列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<string> Labels { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Initializations

        #region 初始化 —— Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.DisplayName = Constants.WindowTitle;
            this.BackgroundBrush = new SolidColorBrush(Colors.LightGray);
            this.BorderBrush = new SolidColorBrush(Colors.Red);
            this.BorderThickness = 2;
            this.ShowGuideLines = true;
            this.GuideLinesVisibility = Visibility.Visible;
            this.WithPascal = true;
            this.WithYoloDet = true;
            this.WithYoloSeg = false;
            this.WithYoloObb = false;
            this.ImageAnnotations = new ObservableCollection<ImageAnnotation>();
            this.Labels = new ObservableCollection<string>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 查看标注信息 —— async void LookAnnotation()
        /// <summary>
        /// 查看标注信息
        /// </summary>
        public async void LookAnnotation()
        {
            Annotation annotation = this.SelectedImageAnnotation?.SelectedAnnotation;
            if (annotation != null)
            {
                LookViewModel viewModel = ResolveMediator.Resolve<LookViewModel>();
                viewModel.Load(annotation.Label.Trim(), annotation.GroupId, annotation.Truncated, annotation.Difficult, annotation.ShapeL.Text, annotation.Description);
                await this._windowManager.ShowDialogAsync(viewModel);
            }
        }
        #endregion

        #region 修改标注信息 —— async void UpdateAnnotation()
        /// <summary>
        /// 修改标注信息
        /// </summary>
        public async void UpdateAnnotation()
        {
            Annotation annotation = this.SelectedImageAnnotation?.SelectedAnnotation;
            if (annotation != null)
            {
                UpdateViewModel viewModel = ResolveMediator.Resolve<UpdateViewModel>();
                viewModel.Load(annotation.Label, annotation.GroupId, annotation.Truncated, annotation.Difficult, annotation.Description, this.Labels);
                bool? result = await this._windowManager.ShowDialogAsync(viewModel);
                if (result == true)
                {
                    annotation.Label = viewModel.Label.Trim();
                    annotation.GroupId = viewModel.GroupId;
                    annotation.Truncated = viewModel.Truncated;
                    annotation.Difficult = viewModel.Difficult;
                    annotation.Description = viewModel.Description;
                    if (!this.Labels.Contains(viewModel.Label.Trim()))
                    {
                        this.Labels.Add(viewModel.Label.Trim());
                    }

                    this.SaveAnnotations();
                }
            }
        }
        #endregion

        #region 删除标注信息 —— void RemoveAnnotation()
        /// <summary>
        /// 删除标注信息
        /// </summary>
        public void RemoveAnnotation()
        {
            Annotation annotation = this.SelectedImageAnnotation?.SelectedAnnotation;
            if (annotation != null)
            {
                MessageBoxResult result = MessageBox.Show("确定要删除吗？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    this.SelectedImageAnnotation.Shapes.Remove(annotation.Shape);
                    this.SelectedImageAnnotation.Annotations.Remove(annotation);
                    this.SaveAnnotations();
                }
            }
        }
        #endregion

        #region GrabCut分割 —— async void GrabCutSegment()
        /// <summary>
        /// GrabCut分割
        /// </summary>
        public async void GrabCutSegment()
        {
            Annotation annotation = this.SelectedImageAnnotation?.SelectedAnnotation;
            if (annotation != null)
            {
                this.Busy();

                //数据准备
                Rect rect = annotation.ShapeL is RectangleL rectangleL
                    ? new Rect(rectangleL.X, rectangleL.Y, rectangleL.Width, rectangleL.Height)
                    : new Rect(
                        new Point(annotation.Shape.RenderedGeometry.Bounds.X,
                            annotation.Shape.RenderedGeometry.Bounds.Y),
                        new Size(annotation.Shape.RenderedGeometry.Bounds.Width,
                            annotation.Shape.RenderedGeometry.Bounds.Height));
                using Mat originalImage = this.SelectedImageAnnotation.Image.Value.ToMat();
                using Mat image = originalImage.Channels() == 4
                    ? originalImage.CvtColor(ColorConversionCodes.BGRA2BGR)
                    : originalImage.Channels() == 1
                        ? originalImage.CvtColor(ColorConversionCodes.GRAY2BGR)
                        : originalImage;

                //分割
                Mat mask = null;
                using Mat result = await Task.Run(() => image.GrabCutSegment(rect, out mask));

                //查找轮廓
                Point[][] contours = { };
                await Task.Run(() => Cv2.FindContours(mask, out contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple));
                mask.Dispose();

                //取最大轮廓
                Point[] contour = contours.OrderByDescending(contour => Cv2.ArcLength(contour, true)).FirstOrDefault();
                if (contour != null)
                {
                    PointCollection points = new PointCollection();
                    IList<PointL> pointLs = new List<PointL>();
                    foreach (Point point in contour)
                    {
                        System.Windows.Point point2D = new System.Windows.Point(point.X, point.Y);
                        PointL pointL = new PointL(point.X, point.Y);
                        points.Add(point2D);
                        pointLs.Add(pointL);
                    }

                    Polygon polygon = new Polygon
                    {
                        Points = points,
                        Stroke = this.BorderBrush,
                        StrokeThickness = this.BorderThickness,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    PolygonL polygonL = new PolygonL(pointLs);
                    polygon.Tag = polygonL;
                    polygonL.Tag = polygon;

                    Annotation polyAnnotation = new Annotation(annotation.Label, annotation.GroupId, annotation.Truncated, annotation.Difficult, polygonL, annotation.Description);
                    this.SelectedImageAnnotation.Shapes.Add(polygon);
                    this.SelectedImageAnnotation.Annotations.Add(polyAnnotation);
                }

                this.Idle();
            }
        }
        #endregion

        #region 创建标签 —— async void CreateLabel()
        /// <summary>
        /// 创建标签
        /// </summary>
        public async void CreateLabel()
        {
            LabelViewModel viewModel = ResolveMediator.Resolve<LabelViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                if (!this.Labels.Contains(viewModel.Label.Trim()))
                {
                    this.Labels.Add(viewModel.Label.Trim());
                    this.SaveAnnotations();
                }
                else
                {
                    this.ToastError("标签已存在！");
                }
            }
        }
        #endregion


        //Events

        #region 图像选中事件 —— async void OnImageSelect()
        /// <summary>
        /// 图像选中事件
        /// </summary>
        public async void OnImageSelect()
        {
            this.ClearAnnotations();
            await this.LoadAnnotations();
        }
        #endregion

        #region 标注信息选中事件 —— void OnAnnotationSelect()
        /// <summary>
        /// 标注信息选中事件
        /// </summary>
        public void OnAnnotationSelect()
        {
            Annotation annotation = this.SelectedImageAnnotation?.SelectedAnnotation;
            annotation?.Shape.BlinkStroke();
        }
        #endregion

        #region 标注信息勾选事件 —— void OnAnnotationCheck(Annotation annotation)
        /// <summary>
        /// 标注信息勾选事件
        /// </summary>
        public void OnAnnotationCheck(Annotation annotation)
        {
            annotation.Shape.Visibility = Visibility.Visible;
        }
        #endregion

        #region 标注信息取勾事件 —— void OnAnnotationUncheck(Annotation annotation)
        /// <summary>
        /// 标注信息取勾事件
        /// </summary>
        public void OnAnnotationUncheck(Annotation annotation)
        {
            annotation.Shape.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 拖拽元素事件 —— void OnDragElement(CanvasEx canvas)
        /// <summary>
        /// 拖拽元素事件
        /// </summary>
        public void OnDragElement(CanvasEx canvas)
        {
            if (canvas.SelectedVisual is Shape shape)
            {
                Annotation annotation = this.SelectedImageAnnotation.Annotations.SingleOrDefault(x => x.Shape == shape);
                if (annotation != null)
                {
                    annotation.ShapeL = (ShapeL)shape.Tag;
                }
            }
        }
        #endregion

        #region 改变元素尺寸事件 —— void OnResizeElement(CanvasEx canvas)
        /// <summary>
        /// 改变元素尺寸事件
        /// </summary>
        public void OnResizeElement(CanvasEx canvas)
        {
            if (canvas.SelectedVisual is Shape shape)
            {
                Annotation annotation = this.SelectedImageAnnotation.Annotations.SingleOrDefault(x => x.Shape == shape);
                if (annotation != null)
                {
                    annotation.ShapeL = (ShapeL)shape.Tag;
                }
            }
        }
        #endregion

        #region 形状鼠标左击事件 —— void OnShapeMouseLeftDown(CanvasEx canvas...
        /// <summary>
        /// 形状鼠标左击事件
        /// </summary>
        public void OnShapeMouseLeftDown(CanvasEx canvas, ShapeEventArgs eventArgs)
        {
            if (canvas.Mode != CanvasMode.Draw)
            {
                Annotation annotation = this.SelectedImageAnnotation.Annotations.SingleOrDefault(x => x.Shape == eventArgs.Shape);
                if (annotation != null)
                {
                    this.SelectedImageAnnotation.SelectedAnnotation = null;
                    this.SelectedImageAnnotation.SelectedAnnotation = annotation;
                }
            }
        }
        #endregion

        #region 形状绘制完成事件 —— async void OnShapeDrawn(ShapeEventArgs eventArgs)
        /// <summary>
        /// 形状绘制完成事件
        /// </summary>
        public async void OnShapeDrawn(ShapeEventArgs eventArgs)
        {
            Shape shape = eventArgs.Shape;
            ShapeL shapeL = (ShapeL)shape.Tag;

            AddViewModel viewModel = ResolveMediator.Resolve<AddViewModel>();
            viewModel.Load(this.Labels);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                Annotation annotation = new Annotation(viewModel.Label.Trim(), viewModel.GroupId, viewModel.Truncated, viewModel.Difficult, shapeL, viewModel.Description);
                this.SelectedImageAnnotation.Annotations.Add(annotation);
                if (!this.Labels.Contains(annotation.Label.Trim()))
                {
                    this.Labels.Add(annotation.Label.Trim());
                }
                this.SaveAnnotations();
            }
            else
            {
                this.SelectedImageAnnotation.Shapes.Remove(shape);
            }

            //设置光标
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        #endregion

        #region 画布鼠标移动事件 —— void OnCanvasMouseMove(CanvasEx canvas)
        /// <summary>
        /// 画布鼠标移动事件
        /// </summary>
        public void OnCanvasMouseMove(CanvasEx canvas)
        {
            if (this.SelectedImageAnnotation != null)
            {
                System.Windows.Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;
                this.MousePositionX = (int)Math.Ceiling(rectifiedPosition.X);
                this.MousePositionY = (int)Math.Ceiling(rectifiedPosition.Y);
            }
        }
        #endregion

        #region 画布鼠标左键松开事件 —— void OnCanvasMouseLeftUp(CanvasEx canvas)
        /// <summary>
        /// 画布鼠标左键松开事件
        /// </summary>
        public void OnCanvasMouseLeftUp(CanvasEx canvas)
        {
            if (canvas.Mode == CanvasMode.Drag || canvas.Mode == CanvasMode.Resize)
            {
                this.SaveAnnotations();
            }
        }
        #endregion

        #region 键盘按下事件 —— void OnKeyDown()
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public void OnKeyDown()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                this.SaveAnnotations();
            }
        }
        #endregion


        //Private

        #region 加载标注信息 —— async Task LoadAnnotations()
        /// <summary>
        /// 加载标注信息
        /// </summary>
        private async Task LoadAnnotations()
        {
            #region # 验证

            if (this.SelectedImageAnnotation == null)
            {
                return;
            }

            #endregion

            string annotationName = Path.GetFileNameWithoutExtension(this.SelectedImageAnnotation.ImagePath);
            string annotationPath = $"{this.ImageFolder}/{annotationName}.json";

            #region # 验证

            if (!File.Exists(annotationPath))
            {
                return;
            }

            #endregion

            string meAnnotationJson = await Task.Run(() => File.ReadAllText(annotationPath));
            MeAnnotation meAnnotation = meAnnotationJson.AsJsonTo<MeAnnotation>();
            IEnumerable<Annotation> annotations = meAnnotation.Shapes.Select(x => x.ToAnnotation());
            foreach (Annotation annotation in annotations)
            {
                annotation.Shape.Stroke = this.BorderBrush;
                annotation.Shape.StrokeThickness = this.BorderThickness;

                this.SelectedImageAnnotation.Shapes.Add(annotation.Shape);
                this.SelectedImageAnnotation.Annotations.Add(annotation);
            }
        }
        #endregion

        #region 清空标注信息 —— void ClearAnnotations()
        /// <summary>
        /// 清空标注信息
        /// </summary>
        private void ClearAnnotations()
        {
            if (this.SelectedImageAnnotation != null)
            {
                this.SelectedImageAnnotation.Shapes.Clear();
                this.SelectedImageAnnotation.Annotations.Clear();
                this.SelectedImageAnnotation.SelectedAnnotation = null;
            }
        }
        #endregion

        #region 加载标签 —— async Task LoadLabels()
        /// <summary>
        /// 加载标签
        /// </summary>
        public async Task LoadLabels()
        {
            if (!string.IsNullOrWhiteSpace(this.ImageFolder))
            {
                string labelsPath = $"{this.ImageFolder}/classes.txt";
                if (File.Exists(labelsPath))
                {
                    string[] lines = await Task.Run(() => File.ReadAllLines(labelsPath));
                    foreach (string line in lines)
                    {
                        if (!this.Labels.Contains(line))
                        {
                            this.Labels.Add(line);
                        }
                    }
                }
            }
        }
        #endregion

        #region 保存标签 —— async Task SaveLabels()
        /// <summary>
        /// 保存标签
        /// </summary>
        private async Task SaveLabels()
        {
            string labelsPath = $"{this.ImageFolder}/classes.txt";
            await Task.Run(() => File.WriteAllLines(labelsPath, this.Labels));
        }
        #endregion

        #region 绘制掩膜 —— async Task DrawMask(Mat mask...
        /// <summary>
        /// 绘制掩膜
        /// </summary>
        /// <param name="mask">掩膜</param>
        /// <param name="shapeLs">形状列表</param>
        private async Task DrawMask(Mat mask, IList<ShapeL> shapeLs)
        {
            const int thickness = -1;
            foreach (ShapeL shapeL in shapeLs)
            {
                if (shapeL is RectangleL rectangleL)
                {
                    Rect rect = new Rect(rectangleL.X, rectangleL.Y, rectangleL.Width, rectangleL.Height);
                    await Task.Run(() => mask.Rectangle(rect, Scalar.White, thickness));
                }
                if (shapeL is RotatedRectangleL rotatedRectangleL)
                {
                    Point[] contour = new Point[4];
                    contour[0] = new Point(rotatedRectangleL.TopLeft.X, rotatedRectangleL.TopLeft.Y);
                    contour[1] = new Point(rotatedRectangleL.TopRight.X, rotatedRectangleL.TopRight.Y);
                    contour[2] = new Point(rotatedRectangleL.BottomRight.X, rotatedRectangleL.BottomRight.Y);
                    contour[3] = new Point(rotatedRectangleL.BottomLeft.X, rotatedRectangleL.BottomLeft.Y);
                    await Task.Run(() => mask.DrawContours(new[] { contour }, -1, Scalar.White, thickness));
                }
                if (shapeL is CircleL circleL)
                {
                    await Task.Run(() => mask.Circle(circleL.X, circleL.Y, circleL.Radius, Scalar.White, thickness));
                }
                if (shapeL is EllipseL ellipseL)
                {
                    Point2f center = new Point2f(ellipseL.X, ellipseL.Y);
                    Size2f size = new Size2f(ellipseL.RadiusX * 2, ellipseL.RadiusY * 2);
                    RotatedRect rotatedRect = new RotatedRect(center, size, 0);
                    await Task.Run(() => mask.Ellipse(rotatedRect, Scalar.White, thickness));
                }
                if (shapeL is PolygonL polygonL)
                {
                    Point[] contour = new Point[polygonL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL pointL = polygonL.Points.ElementAt(index);
                        contour[index] = new Point(pointL.X, pointL.Y);
                    }
                    await Task.Run(() => mask.DrawContours(new[] { contour }, 0, Scalar.White, thickness));
                }
                if (shapeL is PolylineL polylineL)
                {
                    Point[] contour = new Point[polylineL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL pointL = polylineL.Points.ElementAt(index);
                        contour[index] = new Point(pointL.X, pointL.Y);
                    }
                    await Task.Run(() => mask.DrawContours(new[] { contour }, 0, Scalar.White, thickness));
                }
            }
        }
        #endregion

        #endregion
    }
}
