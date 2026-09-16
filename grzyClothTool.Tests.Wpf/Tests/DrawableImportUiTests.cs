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
            window.SelectedDrawableType = "lowr";
            ((CustomButton)window.FindName("ApplyDrawableButton")).RaiseEvent(new RoutedEventArgs(CustomButton.BtnClickEvent));
            Assert.Equal(4, window.Items[0].DrawableType);
            Assert.True(window.IsSubmitEnabled);
            window.Close();

            GDrawable Drawable(int type) => new(Guid.NewGuid(), null!, Enums.SexType.female, false, type, 0, false, []);
            var firstAddon = new ObservableCollection<GDrawable> { Drawable(11), Drawable(4), Drawable(11) };
            var secondAddon = new ObservableCollection<GDrawable> { Drawable(4) };
            var list = new DrawableList { GroupByDrawableType = true, ItemsSource = firstAddon };
            Assert.True(list.IsPrimaryGroupingByTypeName);
            var groups = list.DrawablesView.Groups.Cast<CollectionViewGroup>().ToArray();
            Assert.Equal(new[] { "jbib", "lowr" }, groups.Select(g => g.Name));
            Assert.Equal(2, groups[0].ItemCount);
            list.ItemsSource = secondAddon;
            Assert.Single(list.DrawablesView.Groups);
            Assert.Equal("lowr", ((CollectionViewGroup)list.DrawablesView.Groups[0]).Name);
        });
    }
}
