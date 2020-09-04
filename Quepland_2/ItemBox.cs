using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Quepland_2
{
    public class ItemBox : ComponentBase, IDisposable
    {
        [Inject]
        public GameState GameState { get; set; }
        [Parameter]
        public GameItem Item { get; set; }
        [Parameter]
        public bool ShowAmount { get; set; }
        [Parameter]
        public Inventory Inventory { get; set; }
        [Parameter]
        public bool IsSelected { get; set; }
        [Parameter]
        public bool shouldRender { get; set; }
        [Parameter]
        public bool HideTooltip { get; set; }

        protected override void OnAfterRender(bool first)
        {
            if (Item != null)
            {
                Item.Rerender = false;
            }

            shouldRender = false;

        }

        public int boxSize = 50;
        public int boxMargin = 2;
        public string image = "NoImage.png";
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if(Item != null)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "unselectable");
                builder.AddAttribute(2, "style", GetBackground() + "border:solid black 1px;border-radius:0.25rem;position:relative;box-sizing: border-box;height:" + boxSize + "px;width:" + boxSize + "px;margin:" + boxMargin + "px;");
                if (ShowAmount)
                {
                    builder.OpenElement(3, "div");
                    builder.AddAttribute(4, "style", "position:absolute;margin-left:2px;margin-top:-2px;color:white;font-size: 15px;font-weight: bold;text-shadow: 1px 1px 4px black;");
                    builder.AddContent(5, GetAmount());
                    builder.CloseElement();
                }
                if (HideTooltip == false)
                {
                    builder.OpenElement(6, "img");
                    builder.AddAttribute(7, "style", "height: 46px;width: 46px;" + GetMargin());
                    builder.AddAttribute(8, "src", "data/Images/" + Item.Icon + ".png");
                    builder.AddAttribute(9, "draggable", "false");
                    builder.AddAttribute(10, "onmouseover", new Action<MouseEventArgs>(e => GameState.ShowItemTooltip(e, GetTooltipTitle(), Item.Description)));
                    builder.AddAttribute(11, "onmouseout", new Action(() => GameState.HideTooltip()));
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(12, "img");
                    builder.AddAttribute(13, "style", "height: 46px;width: 46px;" + GetMargin());
                    builder.AddAttribute(14, "src", "data/Images/" + Item.Icon + ".png");
                    builder.AddAttribute(15, "draggable", "false");
                    builder.CloseElement();
                }

                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "unselectable");
                builder.AddAttribute(2, "style", "border:solid black 1px;border-radius:0.25rem;position:relative;box-sizing: border-box;height:" + boxSize + "px;width:" + boxSize + "px;margin:" + boxMargin + "px;");

                builder.CloseElement();
            }
        }
        public string GetTooltipTitle()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            string tip = "";

            if (Bank.Instance.IsBanking && Inventory == Bank.Instance.Inventory)
            {
                if (Bank.Instance.Amount == int.MaxValue)
                {
                    builder.Append("Withdraw All ");
                    builder.Append(Item.Name);
                }
                else
                {
                    builder.Append("Withdraw ");
                    builder.Append(Bank.Instance.Amount);
                    builder.Append(" ");
                    builder.Append(Item.Name);
                }
            }
            else
            {
                if (Item.IsStackable)
                {
                    if (Inventory == null)
                    {
                        builder.Append(Player.Instance.Inventory.GetNumberOfItem(Item));
                        builder.Append(" ");
                        builder.Append(Item.Name);

                    }
                    else
                    {
                        builder.Append(Inventory.GetNumberOfItem(Item));
                        builder.Append(" ");
                        builder.Append(Item.Name);
                    }
                }
                else
                {
                    builder.Append(Item.Name);
                }
            }
            return builder.ToString();
        }
        private string GetAmount()
        {
            if (ShowAmount)
            {
                if (Inventory == null)
                {
                    return "" + Player.Instance.Inventory.GetNumberOfItem(Item).CustomFormat();
                }
                return "" + Inventory.GetNumberOfItem(Item).CustomFormat();
            }
            return "";
        }
        private string GetMargin()
        {
            if (IsSelected)
            {
                return "margin:1px;";
            }
            return "margin:2px;";
        }
        private string GetBackground()
        {
            System.Text.StringBuilder styleBuilder = new System.Text.StringBuilder();
            System.Text.StringBuilder borderBuilder = new System.Text.StringBuilder();

            if (IsSelected)
            {
                borderBuilder.Append("border:solid white 2px;");
                styleBuilder.Append("background-color:lightgreen;");
            }
            else
            {

                if (Item != null)
                {
                    styleBuilder.Append("background: linear-gradient(155deg, " + Item.SecondaryColor + " 60%, " + Item.PrimaryColor + " 50%);");

                }
                else
                {
                    styleBuilder.Append("background-color:darkgray;");
                }

            }
            if (Item != null && Item.IsEquipped)
            {
                borderBuilder.Append("border:solid 2px lightgreen;");
            }
            styleBuilder.Append(borderBuilder.ToString());
            return styleBuilder.ToString();
        }
        protected override void OnInitialized()
        {
            GameState.StateChanged += OnGameStateChanged;
        }
        public void Dispose()
        {
            if (Item != null && Item.Rerender == true && TooltipManager.CurrentTip != null && Item.Name == TooltipManager.CurrentTip.Title)
            {
                GameState.HideTooltip();
            }

            GameState.StateChanged -= OnGameStateChanged;
        }
        void OnGameStateChanged(object sender, EventArgs e) => StateHasChanged();
    }

}

