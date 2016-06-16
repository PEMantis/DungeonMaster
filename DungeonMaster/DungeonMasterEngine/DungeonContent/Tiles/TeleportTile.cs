﻿using System;
using System.Linq;
using DungeonMasterEngine.DungeonContent.Actuators;
using DungeonMasterEngine.DungeonContent.Items;
using DungeonMasterEngine.Graphics;
using DungeonMasterEngine.Graphics.ResourcesProvides;
using DungeonMasterEngine.Interfaces;
using DungeonMasterParser.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonMasterEngine.DungeonContent.Tiles
{
    public class TeleportTile : Teleport<Message>
    {
        public TeleportTile(TeleprotInitializer initializer) : base(initializer) { }
    }

    public class Teleport<TMessage> : FloorTile<TMessage>, ILevelConnector where TMessage : Message
    {
        public IConstrain ScopeConstrain { get; private set; }
        public bool Visible { get; private set; }
        public int NextLevelIndex { get; private set; }
        public Point TargetTilePosition { get; private set; }
        public MapDirection Direction { get; private set; }
        public bool AbsoluteDirection { get; private set; }

        public ITile NextLevelEnter { get; set; }

        public bool Open { get; private set; }

        public sealed override bool ContentActivated
        {
            get { return Open; }

            protected set
            {
                Open = value;
                TryTeleportAll();
            }
        }


        private void TryTeleportAll()
        {
            if (Open)
            {
                foreach (var i in SubItems.ToArray())
                    TeleportItem(i);
            }
        }

        public Teleport(TeleprotInitializer initializer) : base(initializer)
        {
            initializer.Initializing += Initialize;
        }

        private void Initialize(TeleprotInitializer initializer)
        {
            Direction = initializer.Direction;
            AbsoluteDirection = initializer.AbsoluteDirection;
            NextLevelIndex = initializer.NextLevelIndex;
            ScopeConstrain = initializer.ScopeConstrain;
            TargetTilePosition = initializer.TargetTilePosition;
            Visible = initializer.Visible;
            Open = initializer.Open;

            FloorSide.SubItemsChaned += (sender, args) => TryTeleportAll();

            initializer.Initializing -= Initialize;
        }

        private void TeleportItem(object obj)
        {
            var localizable = obj as ILocalizable<ITile>;
            if (localizable != null && Open && ScopeConstrain.IsAcceptable(obj) && NextLevelEnter != null)//TODO how to set taget location creatures
            {
                if (AbsoluteDirection)
                {
                    localizable.MapDirection = Direction;
                }
                else
                {
                    localizable.MapDirection = localizable.MapDirection.GetRotated(Direction.Index + 1);
                }
                localizable.Location = NextLevelEnter; 
            }
        }

    }


}
