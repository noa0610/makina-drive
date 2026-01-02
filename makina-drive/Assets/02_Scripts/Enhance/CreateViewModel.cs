/// <summary>
/// EnhanceViewModelクラスを与える
/// </summary>
public class EnhanceViewModelFactory
{
    public EnhanceViewModel Create(EnhanceData data, EnhanceInventory inventory)
    {
        return new EnhanceViewModel
        {
            name = data.enhanceName,
            description = data.discription,
            currentLevel = inventory.GetLevel(data)
        };
    }

    /* 使用例 -------------------------------------
    var factory = new EnhanceViewModelFactory();
    var viewModel = factory.Create(enhanceData, inventory); // UI情報クラスの初期化
    ---------------------------------------------- */
}
