using DungeonMasterEngine.DungeonContent.Tiles;
using DungeonMasterEngine.Interfaces;

namespace DungeonMasterEngine.DungeonContent.GroupSupport
{
    public interface ISpaceRouteElement : IStopable 
    {
        ISpace Space { get; }
        Tile Tile { get; }
    }
}