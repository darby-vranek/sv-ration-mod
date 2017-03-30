using Microsoft.Xna.Framework;
using System;
using StardewValley.Quests;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	[XmlInclude(typeof(RationQuest))]
	public class RationQuest : Quest
	{
		public List<string> ToFeed;
		public List<string> FedVillagers;
		public List<string> Villagers;
		public int totalPerVill;
		public int multiplier;
		public int totalIncome = 0;

		public RationQuest()
		{
			this.questTitle = "Daily Ration Quota";
			this.questDescription = "The following villagers need to be fed today: ";
			this.Villagers = this.GetVillagers();
			this.FedVillagers = new List<string>();
			this.ToFeed = this.GetRandomVillagers(this.Villagers);
			this.FedVillagers = new List<string>();
			this.questDescription += String.Join(", ", this.ToFeed);
			this.currentObjective = $"0/{this.ToFeed.Count} rations prepared";
		}

		public List<string> GetVillagers()
		{
			List<string> villagers = new List<string>();
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC character in location.characters)
				{
					if (!character.isInvisible && !character.name.Contains("Qi") && (!character.name.Contains("???") && !character.name.Equals("Sandy")) && (!character.name.Contains("Dwarf") && !character.name.Contains("Gunther") && (!character.name.Contains("Mariner") && !character.name.Contains("Henchman"))) && (!character.name.Contains("Marlon") && !character.name.Contains("Wizard") && (!character.name.Contains("Bouncer") && !character.name.Contains("Krobus")) && character.isVillager()))
						villagers.Add(character.name);
				}
			}
			return villagers;
		}

		public List<string> GetRandomVillagers(List<string> villagerList) 
		{
			Random rand = new Random();
			int randomCount = rand.Next(0, 5);
			List<string> randomVillagers = new List<string>();
			for (int i = 0; i < randomCount; i++)
			{
				bool isUnique = false;
				while (!isUnique)
				{
					int idx = rand.Next(0, this.Villagers.Count);
					if (!randomVillagers.Contains(this.Villagers[idx]))
					{
						isUnique = true;
						randomVillagers.Add(this.Villagers[idx]);
					}
				}
			}
			return randomVillagers;
		}

		public void FeedRandomVillager() 
		{
			Random rand = new Random();
			string fedVillager = "";
			while (fedVillager.Length == 0)
			{
				int idx = rand.Next(0, this.ToFeed.Count);
				if (!this.FedVillagers.Contains(this.ToFeed[idx]))
				{
					fedVillager = this.ToFeed[idx];
				}
			}
			this.FedVillagers.Add(fedVillager);
			Game1.getFarm().shippingBin.Remove(Game1.getFarm().lastItemShipped);
			Game1.getFarm().lastItemShipped = null;
			if (this.FedVillagers.Count != this.ToFeed.Count)
			{
				this.currentObjective = $"{this.FedVillagers.Count}/{this.ToFeed.Count} rations shipped.";
			}
			else
			{
				this.currentObjective = "All villagers have been fed!";
			}
		}

		public void UpdateRelationships() 
		{
			foreach (string villager in this.ToFeed)
			{
				if (Game1.player.friendships.ContainsKey(villager))
				{
					int change = 0;
					if (this.FedVillagers.Contains(villager))
					{
						change = 25;
					}
					else
					{
						change = -75;
					}
					Game1.player.changeFriendship(change, Game1.getCharacterFromName(villager));
				}
			}
		}
	}
}