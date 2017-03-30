using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Quests;
using StardewModdingAPI.Framework.Reflection;
using System.Collections.Generic;
using StardewModdingAPI.Inheritance;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using System.Xml.Serialization;


namespace RationMod
{
	[XmlInclude(typeof(RationMod))]
	[XmlInclude(typeof(RationQuest))]

	public class RationMod : Mod
	{
		RationQuest currentQuest = (RationQuest) null;
		int shippingBinCount = 0;
		/*********
		** Public methods
		*********/
		/// <summary>Initialise the mod.</summary>
		/// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
		public override void Entry(IModHelper helper)
		{
			SaveEvents.BeforeSave += preSave;
			SaveEvents.AfterSave += newQuest;
			SaveEvents.AfterLoad += newQuest;
			PlayerEvents.InventoryChanged += this.CheckShouldUpdateLog;
		}


		private void newQuest(object sender, EventArgs e)
		{
			this.currentQuest = new RationQuest();
			Game1.player.questLog.Add(this.currentQuest);
			this.shippingBinCount = 0;
		}

		private void preSave(object sender, EventArgs e)
		{
			if (this.currentQuest != null)
			{
				this.currentQuest.UpdateRelationships();
				Game1.player.GoldPieces += this.currentQuest.totalIncome;
				if (this.currentQuest.ToFeed.Count == this.currentQuest.FedVillagers.Count)
				{
					Game1.player.GoldPieces += 5000;
				}
				Game1.player.questLog.Remove(this.currentQuest);
			}
			currentQuest = null;
		}

		private void CheckShouldUpdateLog(object sender, EventArgsInventoryChanged e)
		{
			if (this.currentQuest.FedVillagers.Count == this.currentQuest.ToFeed.Count)
			{
				return;
			}
			if (e.Removed.Count > 0 && Game1.getFarm().shippingBin.Count != this.shippingBinCount && this.currentQuest.FedVillagers.Count < this.currentQuest.ToFeed.Count)
			{
				foreach (ItemStackChange itemChange in e.Removed)
				{
					Object obj = (Object)itemChange.Item;
					if (obj.edibility > 0)
					{
						for (int i = 0; i < Math.Abs(itemChange.StackChange); i++)
						{
							if (this.currentQuest.FedVillagers.Count == this.currentQuest.ToFeed.Count)
							{
								Item item = (Item)obj;
								item.Stack -= i;
								Game1.getFarm().shippingBin.Add(item);
								break;
							}
							int income = (int)Math.Ceiling(obj.sellToStorePrice() * .5 + 10);
							this.currentQuest.totalIncome += income;
							this.currentQuest.FeedRandomVillager();
						}
					}
				}
				this.shippingBinCount = Game1.getFarm().shippingBin.Count;
			}
			else if (e.QuantityChanged.Count > 0 && Game1.getFarm().shippingBin.Count != this.shippingBinCount && this.currentQuest.FedVillagers.Count < this.currentQuest.ToFeed.Count)
			{
				foreach (ItemStackChange stackChange in e.QuantityChanged)
				{
					Object obj = (Object)stackChange.Item;
					if (stackChange.StackChange < 0 && obj.edibility > 0)
					{
						for (int i = 0; i < Math.Abs(stackChange.StackChange); i++)
						{
							if (this.currentQuest.FedVillagers.Count == this.currentQuest.ToFeed.Count && i < Math.Abs(stackChange.StackChange) - 1)
							{
								stackChange.Item.Stack += i;
								Game1.getFarm().shippingBin.Add(stackChange.Item);
								break;
							}
							int income = (int)(obj.price * .5 + 10);
							this.currentQuest.totalIncome += income;
							this.currentQuest.FeedRandomVillager();
						}
					}
				}
				this.shippingBinCount = Game1.getFarm().shippingBin.Count;
			}
		}
	}
}
