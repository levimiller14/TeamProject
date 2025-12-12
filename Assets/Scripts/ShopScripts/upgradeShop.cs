using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class upgradeShop : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject shopPanel;
    [SerializeField] Transform upgradeListParent;
    [SerializeField] GameObject upgradeButtonPrefab;

    [Header("Upgrades Available")]
    [SerializeField] upgradeData[] availableUpgrades;

    playerController currentPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop(playerController player)
    {
        currentPlayer = player;

        gameManager.instance.statePause();
        gameManager.instance.menuActive = shopPanel;

        buildUpgradeList();

        if (shopPanel != null)
            shopPanel.SetActive(true);
    }

    public void closeShop()
    {
        gameManager.instance.stateUnpause();
    }

    void buildUpgradeList()
    {
        foreach (Transform child in upgradeListParent)
        {
            Destroy(child.gameObject);
        }

        float itemHeight = 80f;
        float spacing = 25f;
        int index = 0;

        foreach (upgradeData upgrade in availableUpgrades)
        {
            GameObject btnObj = Instantiate(upgradeButtonPrefab, upgradeListParent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);

            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            rt.anchoredPosition = new Vector2(0, -index * (itemHeight + spacing));
            Button btn = btnObj.GetComponent<Button>();

            TMP_Text nameText = btnObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text costText = btnObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
            TMP_Text descText = btnObj.transform.Find("DescText")?.GetComponent<TMP_Text>();
            Image iconImg = btnObj.transform.Find("Icon")?.GetComponent<Image>();

            if (nameText != null) nameText.text = upgrade.upgradeName;
            if (costText != null) costText.text = upgrade.cost.ToString();
            if (descText != null) descText.text = upgrade.description;
            if (iconImg != null && upgrade.icon != null) iconImg.sprite = upgrade.icon;

            upgradeData captured = upgrade;
            btn.onClick.AddListener(() => tryBuyUpgrade(captured));

            index++;
        }

        RectTransform parentRT = (RectTransform)upgradeListParent;
        float totalHeight = index * (itemHeight + spacing);
        parentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

    }
    void tryBuyUpgrade(upgradeData upgrade)
    {
        if (currencyManager.Instance != null)
            currencyManager.Instance.Spend(upgrade.cost);
        if (currentPlayer != null)
            currentPlayer.applyUpgrade(upgrade);
    }
}
