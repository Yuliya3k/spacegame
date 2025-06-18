[System.Serializable] // Allows you to modify the list in the Unity Editor
public class ContainerItem
{
    public ItemData itemData; // The item itself
    public int quantity; // The number of this item in the container

    public ContainerItem(ItemData data, int quantity)
    {
        this.itemData = data;
        this.quantity = quantity;
    }
}
