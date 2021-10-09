using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quepland_2
{
    public class WikiInfo
    {
        public string Name;
        public string Image;
        public List<Requirement> Requirements = new List<Requirement>();
        public List<Area> Areas = new List<Area>();
        public List<Shop> Shops = new List<Shop>();
        public List<Dojo> Dojos = new List<Dojo>();
        public List<Building> Buildings = new List<Building>();
        public List<WikiMonsterDrop> MonsterDrops = new List<WikiMonsterDrop>();
        public GameItem Item;
        public NPC Npc;
        public Area Area;
        public Monster Monster;
        public string Description;

        public WikiInfo(string name)
        {
            Name = name;
        }
        public WikiInfo(GameItem item)
        {
            Item = item;
            Name = item.Name;
            Image = item.Icon;
            Requirements = item.Requirements;

            Description = item.Description;
            Areas = AreaManager.Instance.GetAreasForResource(item.Name, true);
            Shops = AreaManager.Instance.GetShopsForResource(item);
            Buildings = AreaManager.Instance.GetBuildingsForResource(item);
            foreach (Monster m in BattleManager.Instance.GetMonstersWithDrop(item))
            {
                foreach (Drop drop in m.DropTable.GetDropsWithName(item.Name))
                {
                    MonsterDrops.Add(new WikiMonsterDrop(m, drop));
                }

            }

        }
        public WikiInfo(Monster monster)
        {
            Name = monster.Name;
            Description = monster.Description;
            Monster = monster;
            if (monster.IsDojoMember)
            {
                foreach(Dojo dojo in AreaManager.Instance.Dojos)
                {
                    if (dojo.OpponentNames.Contains(monster.Name))
                    {
                        Dojos.Add(dojo);
                    }
                }
            }
            foreach(Area a in AreaManager.Instance.Areas)
            {
                if (a.Monsters.Contains(monster.Name))
                {
                    Areas.Add(a);
                }
            }
        }
        public WikiInfo(Area area)
        {
            Name = area.Name;
            Description = area.Description;
            Area = area;
        }

    }
}
