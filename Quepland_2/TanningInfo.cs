using System;

public class TanningInfo
{
    private GameItem tansinto;
	public GameItem TansInto
    {
        get
        {
            if(tansinto == null)
            {
                tansinto = ItemManager.Instance.GetItemByName(TansIntoString);
                
            }
            return tansinto;
        }
    }
    public string TansIntoString { get; set; }
}
