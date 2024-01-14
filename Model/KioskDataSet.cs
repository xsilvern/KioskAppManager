using System.Collections.ObjectModel;

public class KioskDataSet
{
    public ObservableCollection<Category> CategoryList { get; set; } = new ObservableCollection<Category>();
    public ObservableCollection<Menu> MenuList { get; set; } = new ObservableCollection<Menu>();
}