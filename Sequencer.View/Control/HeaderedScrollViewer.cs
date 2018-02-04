using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Sequencer.View.Control
{
    [TemplatePart(Name = "PART_TopHeaderScrollViewer", Type = typeof(ScrollViewer))]
    public class HeaderedScrollViewer : ScrollViewer
    {
        static HeaderedScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderedScrollViewer), new FrameworkPropertyMetadata(typeof(HeaderedScrollViewer)));
        }
        public object TopHeader
        {
            get => GetValue(TopHeaderProperty);
            set => SetValue(TopHeaderProperty, value);
        }
        public static readonly DependencyProperty TopHeaderProperty =
            DependencyProperty.Register("TopHeader", typeof(object), typeof(HeaderedScrollViewer), new UIPropertyMetadata(TopHeader_PropertyChanged));

        private static void TopHeader_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HeaderedScrollViewer instance = (HeaderedScrollViewer)d;
            if (e.OldValue != null)
                instance.RemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                instance.AddLogicalChild(e.NewValue);
        }

        public DataTemplate TopHeaderTemplate
        {
            get => (DataTemplate)GetValue(TopHeaderTemplateProperty);
            set => SetValue(TopHeaderTemplateProperty, value);
        }
        public static readonly DependencyProperty TopHeaderTemplateProperty =
            DependencyProperty.Register("TopHeaderTemplate", typeof(DataTemplate), typeof(HeaderedScrollViewer), new UIPropertyMetadata());

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (TopHeader == null)
                {
                    return base.LogicalChildren;
                }

                var children = new ArrayList();
                IEnumerator baseEnumerator = base.LogicalChildren;
                while (baseEnumerator.MoveNext())
                {
                    children.Add(baseEnumerator.Current);
                }

                if (TopHeader != null)
                {
                    children.Add(TopHeader);
                }

                return children.GetEnumerator();
            }
        }

        private ScrollViewer topHeaderScrollViewer;
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            topHeaderScrollViewer = GetTemplateChild("PART_TopHeaderScrollViewer") as ScrollViewer;
            if (topHeaderScrollViewer != null)
                topHeaderScrollViewer.ScrollChanged += OnTopHeaderScrollViewerScrollChanged;
        }
        

        private void OnTopHeaderScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Equals(e.OriginalSource, topHeaderScrollViewer))
            {
                ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            topHeaderScrollViewer?.ScrollToHorizontalOffset(e.HorizontalOffset);
        }
    }
}
