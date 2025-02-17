
using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Platform.Android;
using Microsoft.Maui.Controls.Platform.Android.FastRenderers;
using AProgressBar = Android.Widget.ProgressBar;
using AView = Android.Views.View;


namespace Microsoft.Maui.Controls.Compatibility.Material.Android
{
	public class MaterialProgressBarRenderer : AProgressBar,
		IVisualElementRenderer, IViewRenderer, ITabStop
	{
		const int MaximumValue = 10000;
		int? _defaultLabelFor;
		bool _disposed;
		ProgressBar _element;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		MotionEventHelper _motionEventHelper;

		public MaterialProgressBarRenderer(Context context)
			: base(MaterialContextThemeWrapper.Create(context), null, Resource.Attribute.materialProgressBarHorizontalStyle)
		{
			Indeterminate = false;
			Max = MaximumValue;

			_visualElementRenderer = new VisualElementRenderer(this);
			_motionEventHelper = new MotionEventHelper();
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_visualElementRenderer.OnTouchEvent(e) || base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		protected AProgressBar Control => this;

		protected ProgressBar Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				var oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<ProgressBar>(oldElement, _element));

				_element?.SendViewInitialized(this);

				_motionEventHelper.UpdateElement(_element);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			_disposed = true;

			if (disposing)
			{
				_visualElementTracker?.Dispose();
				_visualElementTracker = null;

				_visualElementRenderer?.Dispose();
				_visualElementRenderer = null;

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.Android.AppCompat.Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.Android.AppCompat.Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				this.EnsureId();

				if (_visualElementTracker == null)
					_visualElementTracker = new VisualElementTracker(this);

				e.NewElement.PropertyChanged += OnElementPropertyChanged;

				UpdateProgress();
				UpdateColors();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);
			if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
				UpdateProgress();
			else if (e.PropertyName == ProgressBar.ProgressColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateColors();
		}

		void UpdateColors()
		{
			if (Element == null || Control == null)
				return;

			this.ApplyProgressBarColors(Element.ProgressColor, Element.BackgroundColor);
		}

		void UpdateProgress() => Control.Progress = (int)(Element.Progress * MaximumValue);

		// IVisualElementRenderer
		VisualElement IVisualElementRenderer.Element => Element;
		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;
		AView IVisualElementRenderer.View => this;
		void IVisualElementRenderer.SetElement(VisualElement element) =>
			Element = (element as ProgressBar) ?? throw new ArgumentException("Element must be of type ProgressBar.");
		void IVisualElementRenderer.UpdateLayout() =>
			_visualElementTracker?.UpdateLayout();

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(Control.MeasuredWidth, Context.ToPixels(4)), new Size(Context.ToPixels(4), Context.ToPixels(4)));
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		// IViewRenderer
		void IViewRenderer.MeasureExactly() =>
			ViewRenderer.MeasureExactly(this, Element, Context);

		// ITabStop
		AView ITabStop.TabStop => this;
	}
}