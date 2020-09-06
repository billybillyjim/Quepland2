using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class SmithingManager
{
    public static int SmithingStage;
    public static int AutoSmithedItemCount;
    public static bool DoSmelting(Recipe CurrentSmeltingRecipe)
    {
        if (Player.Instance.Inventory.RemoveRecipeItems(CurrentSmeltingRecipe))
        {
            MessageManager.AddMessage("You smelt the " + CurrentSmeltingRecipe.GetIngredientsOnlyString() + " into a " + CurrentSmeltingRecipe.OutputItemName);
            Player.Instance.Inventory.AddItem(CurrentSmeltingRecipe.Output);
            Player.Instance.GainExperience(CurrentSmeltingRecipe.ExperienceGained);
            
            SmithingStage = 1;
            return true;
        }
        return false;
    }
    public static bool GetAutoSmeltingMaterials(Recipe CurrentSmeltingRecipe)
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                if (Player.Instance.CurrentFollower.Inventory.GetUsedSpaces() == 0)
                {
                    int amtToWithdraw = Player.Instance.CurrentFollower.InventorySize / CurrentSmeltingRecipe.GetNumberOfIngredients();
                    foreach (Ingredient i in CurrentSmeltingRecipe.Ingredients)
                    {
                        int actualAmt = Math.Min(amtToWithdraw * i.Amount, Bank.Instance.Inventory.GetNumberOfItem(i.Item)); 
                        if (actualAmt == 0)
                        {
                            return false;
                        }
                        if(Bank.Instance.Inventory.RemoveItems(i.Item, actualAmt) == actualAmt)
                        {
                            Player.Instance.CurrentFollower.Inventory.AddMultipleOfItem(i.Item, actualAmt);
                            Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " goes to the bank and gathers the resources to smith.");
                    SmithingStage++;
                    return true;
                }
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
                return true;
            }
        }
        return false;
    }
    public static void DoAutoSmelting(Recipe CurrentSmeltingRecipe, Recipe CurrentSmithingRecipe)
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                if (Player.Instance.CurrentFollower.Inventory.RemoveRecipeItemsFromFollower(CurrentSmeltingRecipe))
                {
                    MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " helps smelt the " + CurrentSmeltingRecipe.Output + ".");
                    Player.Instance.CurrentFollower.Inventory.AddItem(CurrentSmeltingRecipe.Output);
                    Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                    AutoSmithedItemCount += CurrentSmithingRecipe.OutputAmount;
                }
                else
                {
                    MessageManager.AddMessage("Everything is ready, so you prepare to hammer the metal.");
                    SmithingStage++;
                    return;
                }
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
            }
        }
    }
    public static bool DoSmithing(Recipe CurrentSmeltingRecipe, Recipe CurrentSmithingRecipe)
    {
        if (Player.Instance.Inventory.RemoveRecipeItems(CurrentSmithingRecipe))
        {
            MessageManager.AddMessage("You hammer the " + CurrentSmeltingRecipe.OutputItemName + " into a " + CurrentSmithingRecipe.OutputItemName + " and place it in water to cool.");
            Player.Instance.GainExperience(CurrentSmithingRecipe.ExperienceGained);
            
            SmithingStage = 2;
            return true;
        }
        return false;
    }
    public static void DoAutoSmithing(Recipe CurrentSmithingRecipe)
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                if (Player.Instance.CurrentFollower.Inventory.RemoveRecipeItemsFromFollower(CurrentSmithingRecipe))
                {
                    MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " helps hammer the heated bars into shape.");
                    Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed * AutoSmithedItemCount;
                    return;
                }
                else
                {
                    MessageManager.AddMessage("Together you finish hammering out the last " + CurrentSmithingRecipe.Output + ".");
                    SmithingStage++;
                    return;
                }
            }
            else
            {
                if (Player.Instance.CurrentFollower.TicksToNextAction % Player.Instance.CurrentFollower.AutoCollectSpeed == 0)
                {
                    MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " helps hammer another bar into shape.");
                }
                Player.Instance.CurrentFollower.TicksToNextAction--;
                return;
            }
        }
    }
    public static void DoAutoWithdrawal(Recipe CurrentSmithingRecipe)
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                Player.Instance.CurrentFollower.Inventory.AddMultipleOfItem(CurrentSmithingRecipe.Output, CurrentSmithingRecipe.OutputAmount);
                AutoSmithedItemCount -= CurrentSmithingRecipe.OutputAmount;
                MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " gathers up a cooled " + CurrentSmithingRecipe.OutputItemName + ".");
                Player.Instance.GainExperience(CurrentSmithingRecipe.ExperienceGained);
                if (GameState.CurrentArtisanTask != null)
                {
                    if (GameState.CurrentArtisanTask.ItemName == CurrentSmithingRecipe.OutputItemName)
                    {
                        if (long.TryParse(CurrentSmithingRecipe.ExperienceGained.Split(':')[1], out long xp))
                        {
                            Player.Instance.GainExperience("Artisan", xp / 5);
                        }
                        else
                        {
                            Player.Instance.GainExperience("Artisan", 15);
                        }
                    }
                }
                Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                if (AutoSmithedItemCount <= 0)
                {
                    SmithingStage = 4;
                }
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
            }
        }
    }
    public static void DepositOutputs()
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                Bank.Instance.DepositAll(Player.Instance.CurrentFollower.Inventory);
                MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " returns to the bank and deposits everything.");
                Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                SmithingStage = 0;
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
            }
        }
    }
}

