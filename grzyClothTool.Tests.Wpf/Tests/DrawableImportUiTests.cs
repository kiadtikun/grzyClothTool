using System.Collections.ObjectModel;
using System.Windows.Data;
using grzyClothTool.Controls;
using grzyClothTool.Models.Drawable;
using grzyClothTool.Views;

namespace grzyClothTool.Tests.Wpf.Tests;

public class DrawableImportUiTests
{
    [Fact]
    public void DropReview_AllowsGenderThenTypeOverride_AndAddonListsGroupByType()
    {
        WpfTestRunner.Run(() =>
        {
            var app = Application.Current ?? new Application();
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/grzyClothTool;component/Themes/Light.xaml")
            });
            const string path = "mp_f_freemode_01_pack^jbib_000_u.ydd";
            var window = new DrawableImportResolveWindow([path],
                new() { [path] = Enums.SexType.female },
                new() { [path] = (false, 11) },
                showGender: true, showDrawableProperties: true, reviewImport: true);
            Assert.Equal(Visibility.Visible, window.GenderControlsVisibility);
            Assert.Equal(Visibility.Visible, window.DrawableControlsVisibility);
            Assert.Single(window.VisibleItems);
            Assert.True(window.Items[0].IsSelected);

            window.SelectedGender = "Male";
            ((CustomButton)window.FindName("ApplyGenderButton")).RaiseEvent(new RoutedEventArgs(CustomButton.BtnClickEvent));
            Assert.Equal(Enums.SexType.male, window.Items[0].Gender);
            Assert.True(window.Items[0].IsSelected);
            Assert.Contains("Accessories [teef]", window.DrawableTypes);
            Assert.Contains("Hair Styles [hair]", window.DrawableTypes);
            window.SelectedDrawableType = "Legs [lowr]";
            ((CustomButton)window.FindName("ApplyDrawableButton")).RaiseEvent(new RoutedEventArgs(CustomButton.BtnClickEvent));
            Assert.Equal(4, window.Items[0].DrawableType);
            Assert.Equal("lowr", window.Items[0].DrawableTypeName);
            Assert.True(window.IsSubmitEnabled);
            window.Close();

            GDrawable Drawable(int type) => new(Guid.NewGuid(), null!, Enums.SexType.female, false, type, 0, false, []);
            var firstAddon = new ObservableCollection<GDrawable> { Drawable(11), Drawable(4), Drawable(11) };
            var secondAddon = new ObservableCollection<GDrawable> { Drawable(4) };
            var list = new DrawableList { GroupByDrawableType = true, ItemsSource = firstAddon };
            Assert.True(list.IsPrimaryGroupingByTypeName);
            list.Measure(new Size(500, 600));
            list.Arrange(new Rect(0, 0, 500, 600));
            list.UpdateLayout();
            IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                    if (child is T match) yield return match;
                    foreach (var nested in Descendants<T>(child)) yield return nested;
                }
            }
            var groups = list.DrawablesView.Groups.Cast<CollectionViewGroup>().ToArray();
            Assert.Equal(new[] { "jbib", "lowr" }, groups.Select(g => g.Name));
            Assert.Equal(2, groups[0].ItemCount);
            Assert.Equal(2, GroupGenderCountConverter.Count(groups[0], Enums.SexType.female));
            firstAddon[0].Sex = Enums.SexType.male;
            Assert.Equal(1, GroupGenderCountConverter.Count(groups[0], Enums.SexType.male));
            Assert.Equal(1, GroupGenderCountConverter.Count(groups[0], Enums.SexType.female));
            Assert.True(list.HasExpandedGroups);
            // A user can toggle individual headers before using the global button.
            var manualExpander = Descendants<System.Windows.Controls.Expander>(list).First();
            manualExpander.IsExpanded = false;
            manualExpander.IsExpanded = true;
            list.ToggleAllGroups();
            list.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            var expanders = Descendants<System.Windows.Controls.Expander>(list).ToArray();
            Assert.NotEmpty(expanders);
            Assert.All(expanders, expander => Assert.False(expander.IsExpanded));
            var labels = Descendants<System.Windows.Controls.TextBlock>(list).Select(t => t.Text).ToArray();
            Assert.Contains("Tops [jbib]", labels);
            Assert.Contains("Legs [lowr]", labels);
            Assert.All(Descendants<Material.Icons.WPF.MaterialIcon>(list).Where(icon => icon.Name == "GroupIcon"),
                icon => Assert.Equal(Visibility.Collapsed, icon.Visibility));
            Assert.True(labels.Count(label => label == "1") >= 2);
            Assert.DoesNotContain(labels, label => label.Contains('♂') || label.Contains('♀'));
            Assert.False(list.GetGroupExpandedState("jbib"));
            Assert.False(list.GetGroupExpandedState("lowr"));
            list.ItemsSource = secondAddon;
            Assert.True(list.GetGroupExpandedState("lowr"));
            Assert.Single(list.DrawablesView.Groups);
            Assert.Equal("lowr", ((CollectionViewGroup)list.DrawablesView.Groups[0]).Name);
            list.ItemsSource = firstAddon;
            Assert.False(list.GetGroupExpandedState("jbib"));
            Assert.False(list.HasExpandedGroups);
            list.ToggleAllGroups();
            Assert.True(list.GetGroupExpandedState("jbib"));
            Assert.True(list.GetGroupExpandedState("lowr"));
            list.SetAllGroupsExpanded(false);
            list.SearchText = "does-not-match";
            var revealTask = list.RevealAndSelectAsync([firstAddon[0], firstAddon[2]]);
            while (!revealTask.IsCompleted)
            {
                var frame = new System.Windows.Threading.DispatcherFrame();
                list.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() => frame.Continue = false));
                System.Windows.Threading.Dispatcher.PushFrame(frame);
            }
            revealTask.GetAwaiter().GetResult();
            Assert.Equal(string.Empty, list.SearchText);
            Assert.True(list.GetGroupExpandedState("jbib"));
            Assert.Equal(2, list.SelectedItems.Count);
            Assert.Contains(firstAddon[0], list.SelectedItems);
            Assert.Contains(firstAddon[2], list.SelectedItems);
            // Exercise categories beyond the viewport and recycled group containers.
            for (var type = 0; type < 12; type++)
                for (var item = 0; item < 30; item++) firstAddon.Add(Drawable(type));
            list.Measure(new Size(500, 160));
            list.Arrange(new Rect(0, 0, 500, 160));
            list.UpdateLayout();
            list.SetAllGroupsExpanded(false);
            list.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            foreach (var scroll in Descendants<System.Windows.Controls.ScrollViewer>(list)) scroll.ScrollToEnd();
            list.UpdateLayout();
            list.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            Assert.All(Descendants<System.Windows.Controls.Expander>(list), expander => Assert.False(expander.IsExpanded));
            Assert.All(list.DrawablesView.Groups.Cast<CollectionViewGroup>(), group => Assert.False(list.GetGroupExpandedState((string)group.Name)));
            list.ToggleAllGroups();
            list.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            list.UpdateLayout();
            Assert.All(Descendants<System.Windows.Controls.Expander>(list), expander => Assert.True(expander.IsExpanded));
            foreach (var scroll in Descendants<System.Windows.Controls.ScrollViewer>(list)) scroll.ScrollToEnd();
            list.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            list.UpdateLayout();
            var connectors = Descendants<System.Windows.Controls.Border>(list)
                .Where(border => border.Name == "GroupConnector").ToArray();
            Assert.NotEmpty(connectors);
            Assert.All(connectors, border => Assert.Equal(Visibility.Visible, border.Visibility));
            foreach (var scroll in Descendants<System.Windows.Controls.ScrollViewer>(list)) scroll.ScrollToTop();
            list.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            list.UpdateLayout();
            Assert.All(Descendants<System.Windows.Controls.Border>(list).Where(border => border.Name == "GroupConnector"),
                border => Assert.Equal(Visibility.Visible, border.Visibility));
        });
    }
}
