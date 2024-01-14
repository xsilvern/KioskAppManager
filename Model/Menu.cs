using System.Windows.Media.Imaging;

public class Menu
{
    public string Name { get; set; }
    public string ImageId { get; set; }
    public decimal Price { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (!(obj is Menu other))
        {
            return false;
        }

        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}