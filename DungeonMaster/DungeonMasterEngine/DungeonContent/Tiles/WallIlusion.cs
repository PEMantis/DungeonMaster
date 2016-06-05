﻿using DungeonMasterEngine.Graphics;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.DungeonContent.Tiles
{
    public class WallIlusion: WallIlusion<Message>
    {
        public WallIlusion(Vector3 position, bool isImaginary, bool isOpen) : base(position, isImaginary, isOpen) {}
    }

    public class WallIlusion<TMessage> : Floor<TMessage> where TMessage : Message
    {
        public bool IsImaginary { get; } //TODO  what does it mean ???

        private bool isOpen;
        public bool IsOpen
        {
            get { return isOpen; }
            private set
            {
                isOpen = value;
                UpdateWall();
            }
        }

        public override bool IsAccessible => IsImaginary || IsOpen;

        public WallIlusion(Vector3 position, bool isImaginary, bool isOpen) : base(position)
        {
            IsImaginary = isImaginary;
            this.isOpen = isOpen;

            //todo illusion wall face
            wallGraphic.Texture = wallGraphic.Resources.DefaultTexture;
            wallGraphic.Outter = true;
        }

        public override void ActivateTileContent()
        {
            base.ActivateTileContent();
            IsOpen = true;
        }

        public override void DeactivateTileContent()
        {
            base.DeactivateTileContent();
            IsOpen = false;
        }

        protected override void UpdateWall()
        {
            base.UpdateWall();
            wallGraphic.Outter = !IsOpen;
            if (!IsOpen)
                wallGraphic.DrawFaces |= CubeFaces.Sides;
        }

    }
}
