using System.Collections.Generic;
using DungeonMasterEngine.Builders.ItemInitializers;
using DungeonMasterEngine.DungeonContent.Entity.Actions.Factories;
using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.@base;
using DungeonMasterEngine.DungeonContent.GrabableItems.Initializers;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonMasterEngine.DungeonContent.GrabableItems.Factories
{
    public class ContainerItemFactory : GrabableItemFactoryBase
    {
        public ContainerItemFactory(string name, int weight, IEnumerable<IActionFactory> attackCombo, IEnumerable<IStorageType> carryLocation, Texture2D texture) : base(name, weight, attackCombo, carryLocation, texture) {}

        public Container Create<TItemInitializator>(TItemInitializator initializator) where TItemInitializator : IContainerInitializer
        {
            return new Container(initializator, this);
        }

        public override IGrabableItem Create()
        {
            return Create(new ContainerInitializer {content = new IGrabableItem[0]});
        }
    }
}