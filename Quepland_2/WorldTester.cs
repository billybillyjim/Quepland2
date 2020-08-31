using System;
using System.Collections.Generic;
using System.Linq;

public class WorldTester
{
    public List<GameItem> IncludedItems = new List<GameItem>();
    public List<GameItem> MissingItems = new List<GameItem>();
    public List<Recipe> RemainingRecipes = new List<Recipe>();
    public List<Recipe> RemainingGemRecipes = new List<Recipe>();
    public List<Recipe> RemainingCaboRecipes = new List<Recipe>();
    public List<Recipe> RemainingBakingRecipes = new List<Recipe>();
    public List<Recipe> RemainingSmithingRecipes = new List<Recipe>();
    public void TestWorld()
    {
        RemainingRecipes.AddRange(ItemManager.Instance.Recipes);
        RemainingSmithingRecipes.AddRange(ItemManager.Instance.SmithingRecipes);
        RemainingGemRecipes.AddRange(ItemManager.Instance.GemCuttingRecipes);
        RemainingCaboRecipes.AddRange(ItemManager.Instance.GemCabochonRecipes);
        RemainingBakingRecipes.AddRange(ItemManager.Instance.BakingRecipes);
        Bank.Instance.Inventory.IsLoadingSave = true;
        foreach(Quest q in QuestManager.Instance.Quests)
        {
            q.IsComplete = true;
            q.Progress = 1000;
        }
        foreach (Skill s in Player.Instance.Skills)
        {
            s.SetSkillLevel(250);
        }
        foreach(NPC npc in NPCManager.Instance.NPCs)
        {
            foreach(Dialog d in npc.Dialogs)
            {
                if(d.ItemOnTalk != "None")
                {
                    TryAddItem(d.ItemOnTalk);
                }
            }
            if(npc.Shop != null)
            {
                foreach(GameItem i in npc.Shop.Items)
                {
                    TryAddItem(i);
                }
            }
        }
        UpdateItemCounts();

        foreach (Monster m in BattleManager.Instance.Monsters)
        {
            CheckMonsterDrops(m);
        }
        UpdateItemCounts();
        foreach (Area a in AreaManager.Instance.Areas)
        {
            CheckArea(a);
        }
        UpdateItemCounts();
        foreach (GameItem i in ItemManager.Instance.Items)
        {
            if(i.Name == "Unidentified Gem")
            {
                TryAddItem(i);
                TryAddItem(ItemManager.Instance.GetItemByName(i.Parameter));
            }
        }
        UpdateItemCounts();
        foreach (MinigameDropTable table in ItemManager.Instance.MinigameDropTables)
        {
            CheckMinigame(table);
            UpdateItemCounts();
        }

        
        CheckRecipes(ref RemainingRecipes, false);
        UpdateItemCounts();
        CheckTanning();
        UpdateItemCounts();
        CheckRecipes(ref RemainingSmithingRecipes, false);
        UpdateItemCounts();
        CheckRecipes(ref RemainingSmithingRecipes, false);
        UpdateItemCounts();
        CheckRecipes(ref RemainingRecipes, false);
        UpdateItemCounts();
        CheckRecipes(ref RemainingRecipes, true);
        UpdateItemCounts();
        CheckRecipes(ref RemainingCaboRecipes, true);
        UpdateItemCounts();
        CheckRecipes(ref RemainingGemRecipes, true);
        UpdateItemCounts();
        CheckRecipes(ref RemainingSmithingRecipes, true);
        UpdateItemCounts();
        CheckRecipes(ref RemainingBakingRecipes, true);
        UpdateItemCounts();

        MissingItems = ItemManager.Instance.Items.Where(x => IncludedItems.Contains(x) == false).ToList();
    }
    private void CheckMinigame(MinigameDropTable table)
    {
        foreach(Drop d in table.DropTable.Drops)
        {
            TryAddItem(d.Item);
        }
    }
    private void CheckTanning()
    {
        List<GameItem> ItemsToAdd = new List<GameItem>();
        foreach (GameItem i in IncludedItems)
        {

            if (i.TanningInfo != null)
            {
                ItemsToAdd.Add(i.TanningInfo.TansInto);

            }
        }
        foreach (GameItem i in ItemsToAdd)
        {
            Bank.Instance.Inventory.AddMultipleOfItem(i, 50);
            TryAddItem(i);
        }
    }
    private void CheckRecipes(ref List<Recipe> recipes, bool final)
    {
        List<Recipe> Successes = new List<Recipe>();
        foreach (Recipe r in recipes)
        {
            try
            {
                if (r.CanCreateFromInventory(Bank.Instance.Inventory))
                {
                    TryAddItem(r.Output);
                    
                    if (r.SecondaryOutput != null)
                    {
                        TryAddItem(r.SecondaryOutput);
                    }
                    if (r.TertiaryOutput != null)
                    {
                        TryAddItem(r.TertiaryOutput);
                    }
                }
                else if(final)
                {
                    Console.WriteLine("Cannot Create " + r.Output);
                    foreach(Ingredient i in r.Ingredients)
                    {
                        Console.WriteLine(i.ItemName + ":" + i.Amount + " vs " + Bank.Instance.Inventory.GetNumberOfItem(i.Item));
                    }
                    Console.WriteLine(r.GetRequirementTooltip());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        foreach(Recipe r in Successes)
        {
            recipes.Remove(r);
        }
    }
    private void CheckArea(Area a)
    {
        foreach(string action in a.Actions)
        {
            CheckActionString(action);
        }
        foreach(Building b in a.Buildings)
        {
            foreach(Shop s in b.Shops)
            {
                CheckShop(s);
            }
        }
        if(a.HuntingTripInfo != null)
        {
            foreach(Drop d in a.HuntingTripInfo.DropTable.Drops)
            {
                TryAddItem(d.Item);
            }
        }
        if(a.TrapSlot != null)
        {
            foreach(Drop d in a.TrapSlot.DropTable.Drops)
            {
                TryAddItem(d.Item);
            }
        }
        foreach(Shop s in a.Shops)
        {
            CheckShop(s);
        }
    }
    private void CheckShop(Shop s)
    {
        foreach (GameItem i in s.Items)
        {
            TryAddItem(i);
        }
    }
    private void CheckActionString(string action)
    {
        if (action == null || action.Contains(':') == false)
        {
            Console.WriteLine("Action Text was null or contained no colon.");
            if (action == null)
            {
                Console.WriteLine("ActionText null:" + (action == null));
            }
            else
            {
                Console.WriteLine("ActionText:" + action);
            }
        }
        else
        {
            foreach (string i in action.Split(':')[1].Split(','))
            {
                GameItem item = ItemManager.Instance.GetItemByName(i);
                if (item != null)
                {
                    TryAddItem(item);
                }
                else
                {
                    Console.WriteLine("Item not found:" + i);
                }
            }
        }
    }
    private void CheckMonsterDrops(Monster m)
    {
        if(m.DropTable != null)
        {
            foreach(Drop d in m.DropTable.Drops)
            {
                TryAddItem(d.Item);
            }
        }
    }

    private void TryAddItem(GameItem i)
    {        
        if (IncludedItems.Contains(ItemManager.Instance.GetItemByUniqueID(i.UniqueID)) == false)
        {
            IncludedItems.Add(ItemManager.Instance.GetItemByUniqueID(i.UniqueID));
            Bank.Instance.Inventory.AddMultipleOfItem(ItemManager.Instance.GetItemByUniqueID(i.UniqueID), 10);
        }
    }
    private void TryAddItem(string itemName)
    {
        TryAddItem(ItemManager.Instance.GetItemByName(itemName));
    }
    private void UpdateItemCounts()
    {
        Bank.Instance.Inventory.IsLoadingSave = false;
        Bank.Instance.Inventory.UpdateItemCount();
        Bank.Instance.Inventory.IsLoadingSave = true;

    }

}
