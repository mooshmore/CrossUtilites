using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CrossUtilites.WPF.Behaviors
{
    /// <summary>
    /// A behavior for WPF Popup control.
    /// Automatically updates the popup position whenever the window changes its position or is resized.
    /// Code taken from https://putridparrot.com/blog/automatically-update-a-wpf-popup-position/.
    /// </summary>
    public class AutoRepositionPopup : Behavior<Popup>
    {
        private const int WM_MOVING = 0x0216;

        // should be moved to a helper class
        private DependencyObject GetTopmostParent(DependencyObject element)
        {
            var current = element;
            var result = element;

            while (current != null)
            {
                result = current;
                current = current is Visual || current is Visual3D ?
                   VisualTreeHelper.GetParent(current) :
                   LogicalTreeHelper.GetParent(current);
            }
            return result;
        }

        /// <summary>
        /// Assigns the events when the behavior is attached.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += (sender, e) =>
            {
                if (GetTopmostParent(AssociatedObject.PlacementTarget) is Window root)
                {
                    var helper = new WindowInteropHelper(root);
                    var hwndSource = HwndSource.FromHwnd(helper.Handle);
                    hwndSource?.AddHook(HwndMessageHook);
                }
            };
            AssociatedObject.LayoutUpdated += (sender, e) => Update();
        }

        private nint HwndMessageHook(nint hWnd,
                int msg, nint wParam,
                nint lParam, ref bool bHandled)
        {
            if (msg == WM_MOVING)
            {
                Update();
            }
            return nint.Zero;
        }

        /// <summary>
        /// Updates the popup position.
        /// </summary>
        public void Update()
        {
            // force the popup to update it's position
            var mode = AssociatedObject.Placement;
            AssociatedObject.Placement = PlacementMode.Relative;
            AssociatedObject.Placement = mode;
        }
    }
}