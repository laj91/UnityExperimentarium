public interface ISlotUI
{
    int SlotIndex { get; }
    void UpdateSlotUI(ItemStack itemStack);
}
