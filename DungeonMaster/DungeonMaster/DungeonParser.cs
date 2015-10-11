﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DungeonMasterParser
{
    public class DungeonParser
    {
        public DungeonData Data { get; private set; } = new DungeonData();

        public const ushort oneBitMask = 0x0001;
        public const ushort twoBitsMask = 0x0003;
        public const ushort threeBitsMask = 0x0007;
        public const ushort fourBitsMask = 0x000F;
        public const ushort fiveBitsMask = 0x001F;
        public const ushort sixBitsMask = 0x003F;
        public const ushort sevenBitsMask = 0x007F;
        public const ushort eightBitsMask = 0x00FF;
        public const ushort nineBitsMask = 0x01FF;
        public const ushort tenBitsMask = 0x03FF;

        public void Parse()
        {
            using (var r = new BinaryReader(new FileStream("DUNGEON.DAT", FileMode.Open)))
            {
                ReadHeader(r);
                Debug.Assert(r.BaseStream.Position == 44);

                Data.Maps = ReadMapsDefinitions(r);
                Debug.Assert(r.BaseStream.Position == 268);

                ReadObjectTilesShortcuts(r);
                Debug.Assert(r.BaseStream.Position == 1086);

                Data.ObjectIDs = ReadObjectIDs(r);
                Debug.Assert(r.BaseStream.Position == 4444);

                Data.TextDataStream = new MemoryStream(r.ReadBytes(3498), false);
                Debug.Assert(r.BaseStream.Position == 7942);

                Data.Doors = ReadDoorsData(r);
                Debug.Assert(r.BaseStream.Position == 8622);

                Data.Teleports = ReadTeleportsData(r);
                Debug.Assert(r.BaseStream.Position == 9696);

                Data.Texts = ReadTextsData(r);
                Debug.Assert(r.BaseStream.Position == 10196);

                Data.Actuators = ReadActuatorsData(r);
                Debug.Assert(r.BaseStream.Position == 15668);

                Data.Creatures = ReadCreaturesData(r);
                Debug.Assert(r.BaseStream.Position == 18580);

                Data.Weapons = ReadWeaponsData(r);
                Debug.Assert(r.BaseStream.Position == 19008);

                Data.Clothes = ReadClothesData(r);
                Debug.Assert(r.BaseStream.Position == 19492);

                Data.Scrolls = ReadScrollsData(r);
                Debug.Assert(r.BaseStream.Position == 19632);

                Data.Potions = ReadPotionsData(r);
                Debug.Assert(r.BaseStream.Position == 19856);

                Data.Containers = ReadContainersData(r);
                Debug.Assert(r.BaseStream.Position == 19952);

                //asuming missiles and clouds empty

                Data.MiscellaneousItems = ReadMiscellaneousItemsData(r);
                Debug.Assert(r.BaseStream.Position == 21072);

                ReadMapData(r);
                ushort checksum = r.ReadUInt16();//TODO Checksum does not fit :OOO
                //Debug.Assert(r.BaseStream.Position == checksum);

                DeployObjects();
            }
        }

        private void DeployObjects()
        {

            int k = 0;
            foreach (Tile t in from i in Data.Maps.SelectMany(x => x.Tiles) where i.HasItemsList select i)
            {
                t.FirstObject = Data.ObjectIDs[k++];

                foreach (var item in t.GetItems(Data))
                {
                    if (item.GetType() == typeof(ActuatorItem))
                        t.Actuators.Add((ActuatorItem)item);
                    else
                        t.Items.Add(item);
                }
            }





        }

        private void ReadHeader(BinaryReader r)
        {
            Data.DungenSeed = r.ReadUInt16();
            Data.GlobalMapDataSize = r.ReadUInt16();
            Data.MapsCount = r.ReadByte();

            r.ReadByte(); //padding

            Data.TextDataSize = r.ReadUInt16();

            Data.StartPosition = ParseStartPosition(r.ReadUInt16());
            Data.ObjectListSize = r.ReadUInt16();

            Data.DoorsCount = r.ReadUInt16();
            Data.TeleportsCount = r.ReadUInt16();
            Data.TextsCount = r.ReadUInt16();
            Data.ActuatorsCount = r.ReadUInt16();
            Data.CreaturesCount = r.ReadUInt16();
            Data.WeaponsCount = r.ReadUInt16();
            Data.ClothesCount = r.ReadUInt16();
            Data.ScrollsCount = r.ReadUInt16();
            Data.PotionsCount = r.ReadUInt16();
            Data.ContainersCount = r.ReadUInt16();
            Data.MiscellaneousItemsCount = r.ReadUInt16();

            r.ReadBytes(6);//unused
            Data.MissilesCount = r.ReadUInt16();
            Data.CloudsCount = r.ReadUInt16();
        }

        private MapPosition ParseStartPosition(ushort value)
        {
            return new MapPosition
            {
                Position = new Position
                {
                    X = value & fiveBitsMask,
                    Y = (value >> 5) & fiveBitsMask
                },

                Direction = (Direction)((value >> 10) & twoBitsMask)
            };
        }

        private IList<DungeonMap> ReadMapsDefinitions(BinaryReader r)
        {
            var maps = new List<DungeonMap>(Data.MapsCount);

            for (int i = 0; i < Data.MapsCount; i++)
                maps.Add(ParseMapDefinition(r));

            return maps;
        }

        private DungeonMap ParseMapDefinition(BinaryReader r)
        {
            var m = new DungeonMap();

            m.GlobalDataOffset = r.ReadUInt16();

            r.ReadUInt16(); //dungeon II only
            r.ReadUInt16(); //unused

            m.OffsetX = r.ReadByte();
            m.OffsetY = r.ReadByte();

            ushort dimensionData = r.ReadUInt16();

            m.Height = ((dimensionData >> 11) & fiveBitsMask) + 1;
            m.Width = ((dimensionData >> 6) & fiveBitsMask) + 1;

            ushort graphicCountData = r.ReadUInt16();

            m.FloorDecorationGraphicsCount = (graphicCountData >> 12) & fourBitsMask;
            m.FloorGraphicsCount = (graphicCountData >> 8) & fourBitsMask;
            m.WallDecorationGraphicsCount = (graphicCountData >> 4) & fourBitsMask;
            m.WallGraphicsCount = graphicCountData & fourBitsMask;

            ushort diffData = r.ReadUInt16();
            m.Difficulty = diffData >> 12;
            m.CreatureTypeCount = (diffData >> 4) & fourBitsMask;
            m.DoorDecorationCount = diffData & fourBitsMask;

            ushort doorIndicesData = r.ReadUInt16();
            m.DoorType = (DoorType)((doorIndicesData >> 12) & fourBitsMask);
            m.DoorType0Index = (doorIndicesData >> 8) & fourBitsMask;

            //TODO
            return m;
        }

        private void ReadObjectTilesShortcuts(BinaryReader r)
        {
            //TODO todo
            r.ReadBytes(818);
        }

        private IList<ObjectID> ReadObjectIDs(BinaryReader r)
        {
            var o = new ObjectID[1300];
            for (int i = 0; i < o.Length; i++)
            {
                ushort data = r.ReadUInt16();

                var t = new ObjectID(data);




                o[i] = t;
            }

            //TODO FF delimiter check
            r.ReadBytes(758); //758 FF unused (unreferenced) objects

            return o;
        }

        private IList<DoorItem> ReadDoorsData(BinaryReader r)
        {
            var doors = new DoorItem[Data.DoorsCount];

            for (int i = 0; i < Data.DoorsCount; i++)
                doors[i] = ParseDoorData(r);

            return doors;
        }

        private DoorItem ParseDoorData(BinaryReader r)
        {
            var d = new DoorItem();
            d.NextObjectID = r.ReadUInt16();

            ushort data = r.ReadUInt16();

            d.IsChoppingDestructible = ((data >> 8) & oneBitMask) == 1;
            d.IsFireballDestructible = ((data >> 7) & oneBitMask) == 1;
            d.HasButton = ((data >> 6) & oneBitMask) == 1;
            d.OpenDirection = (OpenDirection)((data >> 8) & oneBitMask);

            d.OrnamentationID = ((data >> 1) & fourBitsMask);
            if (d.OrnamentationID == 0)
                d.OrnamentationID = null;

            d.DoorAppearance = (data & oneBitMask) == 1;
            return d;
        }

        private IList<TeleporterItem> ReadTeleportsData(BinaryReader r)
        {
            var doors = new TeleporterItem[Data.TeleportsCount];

            for (int i = 0; i < Data.TeleportsCount; i++)
                doors[i] = ParseTeleportData(r);

            return doors;
        }

        private TeleporterItem ParseTeleportData(BinaryReader r)
        {
            var t = new TeleporterItem();
            t.NextObjectID = r.ReadUInt16();

            ushort data1 = r.ReadUInt16();

            t.HasSound = (data1 >> 15) == 1;
            t.Scope = (TeleportScope)((data1 >> 13) & twoBitsMask);
            t.RotationType = (RotationType)((data1 >> 12) & oneBitMask);
            t.Rotation = (Direction)((data1 >> 10) & twoBitsMask);
            t.DestinationPosition = new Position
            {
                Y = (data1 >> 5) & fiveBitsMask,
                X = data1 & fiveBitsMask
            };

            t.MapIndex = (r.ReadUInt16() >> 8) & eightBitsMask;
            return t;
        }

        private IList<TextDataItem> ReadTextsData(BinaryReader r)
        {
            var texts = new TextDataItem[Data.TextsCount];
            for (int i = 0; i < Data.TextsCount; i++)
                texts[i] = ParseTextData(r);
            return texts;
        }

        private TextDataItem ParseTextData(BinaryReader r)
        {
            var t = new TextDataItem();

            t.NextObjectID = r.ReadUInt16();

            ushort data = r.ReadUInt16();

            if (((data >> 1) & twoBitsMask) == 0)
                t.ReferredTextOffset = (data >> 3) * 2; //it is in words

            t.IsVisible = (data & oneBitMask) == 1;

            SetTextData(t);

            return t;
        }

        private void SetTextData(TextDataItem t)
        {
            Data.TextDataStream.Position = t.ReferredTextOffset;
            t.Text = new StreamReader(Data.TextDataStream, new DMEncoding(), false).ReadLine();
        }

        private IList<ActuatorItem> ReadActuatorsData(BinaryReader r)
        {
            var a = new ActuatorItem[Data.ActuatorsCount];
            for (int i = 0; i < Data.ActuatorsCount; i++)
                a[i] = ParseActuatorData(r);

            return a;
        }

        private ActuatorItem ParseActuatorData(BinaryReader r)
        {
            var a = new ActuatorItem();
            a.NextObjectID = r.ReadUInt16();

            ushort data0 = r.ReadUInt16();
            a.Data = (data0 >> 7) & nineBitsMask;
            a.AcutorType = data0 & sevenBitsMask;

            ushort data1 = r.ReadUInt16();
            a.Decoration = (data1 >> 12) & fourBitsMask;
            a.IsLocal = ((data1 >> 11) & oneBitMask) == 1;
            a.ActionDelay = (data1 >> 7) & fourBitsMask;
            a.HasSoundEffect = ((data1 >> 6) & oneBitMask) == 1;
            a.IsRevertable = ((data1 >> 6) & oneBitMask) == 0;
            a.Action = (ActionType)((data1 >> 3) & twoBitsMask);
            a.IsOnceOnly = ((data1 >> 2) & oneBitMask) == 1;

            ushort data2 = r.ReadUInt16();
            if (a.IsLocal)
            {
                int action = (data2 >> 4);
                a.ActionLocation = new LocalTarget
                {
                    RotateAutors = (action == 1 || action == 2),
                    ExperienceGain = action == 10
                };
            }
            else
            {
                a.ActionLocation = new RemoteTarget
                {
                    Position = new MapPosition
                    {
                        Position = new Position
                        {
                            Y = (data2 >> 11) & fiveBitsMask,
                            X = (data2 >> 6) & fiveBitsMask
                        },

                        Direction = (Direction)((data2 >> 4) & twoBitsMask)
                    }
                };
            }

            return a;
        }

        private IList<CreatureItem> ReadCreaturesData(BinaryReader r)
        {
            var c = new CreatureItem[Data.CreaturesCount];

            for (int i = 0; i < c.Length; i++)
                c[i] = ParseCreatureData(r);

            return c;
        }

        private CreatureItem ParseCreatureData(BinaryReader r)
        {
            var c = new CreatureItem();
            c.NextObjectID = r.ReadUInt16();
            c.NextPossessionObjectID = r.ReadUInt16();
            c.Type = (CreatureType)r.ReadByte();

            byte data = r.ReadByte();
            c.Creature1Position = (TilePosition)(data & twoBitsMask);
            c.Creature2Position = (TilePosition)((data >> 2) & twoBitsMask);
            c.Creature3Position = (TilePosition)((data >> 4) & twoBitsMask);
            c.Creature4Position = (TilePosition)((data >> 6) & twoBitsMask);

            c.HitPointsCreature1 = r.ReadUInt16();
            c.HitPointsCreature2 = r.ReadUInt16();
            c.HitPointsCreature3 = r.ReadUInt16();
            c.HitPointsCreature4 = r.ReadUInt16();

            ushort data1 = r.ReadUInt16();
            c.IsImportant = ((data1 >> 10) & oneBitMask) == 1;
            c.Direction = (Direction)((data1 >> 8) & twoBitsMask);
            c.CreaturesCount = ((data1 >> 5) & twoBitsMask);

            return c;
        }

        private IList<WeaponItem> ReadWeaponsData(BinaryReader r)
        {
            var a = new WeaponItem[Data.WeaponsCount];

            for (int i = 0; i < Data.WeaponsCount; i++)
                a[i] = ParseWeaponData(r);

            return a;
        }

        private WeaponItem ParseWeaponData(BinaryReader r)
        {
            var w = new WeaponItem();
            w.NextObjectID = r.ReadUInt16();

            ushort data = r.ReadUInt16();
            w.IsBroken = ((data >> 14) & oneBitMask) == 1;

            w.ChargeCount = (data >> 10) & fourBitsMask;

            w.IsPoisoned = ((data >> 9) & oneBitMask) == 1;
            w.IsCursed = ((data >> 8) & oneBitMask) == 1;
            w.IsImportant = ((data >> 7) & oneBitMask) == 1;

            w.ItemTypeIndex = data & sevenBitsMask;

            return w;
        }

        private IList<ClothItem> ReadClothesData(BinaryReader r)
        {
            var c = new ClothItem[Data.ClothesCount];

            for (int i = 0; i < Data.ClothesCount; i++)
                c[i] = ParseClothData(r);

            return c;
        }

        private ClothItem ParseClothData(BinaryReader r)
        {
            var c = new ClothItem();
            c.NextObjectID = r.ReadUInt16();

            ushort data = r.ReadUInt16();

            c.IsBroken = ((data >> 13) & oneBitMask) == 1;
            c.IsCursed = ((data >> 8) & oneBitMask) == 1;
            c.IsImportant = ((data >> 7) & oneBitMask) == 1;
            c.ItemTypeIndex = data & sixBitsMask;

            return c;
        }

        private IList<ScrollItem> ReadScrollsData(BinaryReader r)
        {
            var c = new ScrollItem[Data.ScrollsCount];

            for (int i = 0; i < Data.ScrollsCount; i++)
                c[i] = ParseScrollData(r);

            return c;
        }

        private ScrollItem ParseScrollData(BinaryReader r)
        {
            var s = new ScrollItem();
            s.NextObjectID = r.ReadUInt16();

            s.ReferredTextIndex = r.ReadUInt16() & nineBitsMask;
            return s;
        }

        private IList<PotionItem> ReadPotionsData(BinaryReader r)
        {
            var c = new PotionItem[Data.PotionsCount];

            for (int i = 0; i < Data.PotionsCount; i++)
                c[i] = ParsePotionData(r);

            return c;
        }

        private PotionItem ParsePotionData(BinaryReader r)
        {
            var p = new PotionItem();
            p.NextObjectID = r.ReadUInt16();

            ushort data = r.ReadUInt16();

            p.IsImportant = (data >> 15) == 1;
            p.ItemTypeIndex = (data >> 8) & sevenBitsMask;
            p.PotionPower = data & eightBitsMask;
            return p;
        }

        private IList<ContainerItem> ReadContainersData(BinaryReader r)
        {
            var c = new ContainerItem[Data.ContainersCount];

            for (int i = 0; i < Data.ContainersCount; i++)
                c[i] = ParseContainerData(r);

            return c;
        }

        private ContainerItem ParseContainerData(BinaryReader r)
        {
            var c = new ContainerItem();
            c.NextObjectID = r.ReadUInt16();

            c.NextContainedObjectID = r.ReadUInt16();
            r.ReadUInt32(); //padding
            return c;
        }

        private IList<MiscellaneousItem> ReadMiscellaneousItemsData(BinaryReader r)
        {
            var c = new MiscellaneousItem[Data.MiscellaneousItemsCount];

            for (int i = 0; i < Data.MiscellaneousItemsCount; i++)
                c[i] = ParseMiscellaneousItemData(r);

            return c;
        }

        private MiscellaneousItem ParseMiscellaneousItemData(BinaryReader r)
        {
            var m = new MiscellaneousItem();
            m.NextObjectID = r.ReadUInt16();

            ushort data = r.ReadUInt16();
            m.AttributeValueIndex = (data >> 14) & twoBitsMask;
            m.IsImportant = ((data >> 7) & oneBitMask) == 1;
            m.ItemTypeIndex = data & sevenBitsMask;
            return m;
        }

        private void ReadMapData(BinaryReader r)
        {
            foreach (var m in Data.Maps)
            {
                m.Tiles = ParseTilesData(r, m.Width * m.Height);
                m.CreaturesDecoration = r.ReadBytes(m.CreatureTypeCount);

                m.WallDecorations = SetupDecoration(Data.WallDecorations, r.ReadBytes(m.WallGraphicsCount));
                m.FloorDecorations = SetupDecoration(Data.FloorDecorations, r.ReadBytes(m.FloorGraphicsCount));
                m.DoorDecorations = SetupDecoration(Data.DoorDecorations, r.ReadBytes(m.DoorDecorationCount));
            }
        }

        public string[] SetupDecoration(IList<string> data, byte[] bytes)
        {
            var res = new string[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                res[i] = data[bytes[i]];

            return res;
        }

        #region Tiles

        private IList<Tile> ParseTilesData(BinaryReader r, int count)
        {
            var t = new Tile[count];
            for (int i = 0; i < count; i++)
                t[i] = ParseTilesData(r);
            return t;
        }

        private Tile ParseTilesData(BinaryReader r)
        {
            byte data = r.ReadByte();

            Tile t = null;

            switch ((data >> 5) & threeBitsMask)
            {
                case 0: t = ParseWallTile(data); break;
                case 1: t = ParseFloorTile(data); break;
                case 2: t = ParsePitTile(data); break;
                case 3: t = ParseStairsTile(data); break;
                case 4: t = ParseDoorTile(data); break;
                case 5: t = ParseTeleporterTile(data); break;
                case 6: t = ParseTrickTile(data); break;
                default: throw new NotSupportedException("Empty tile.Only valid in Dungeon Master II.");
            }

            t.HasItemsList = ((data >> 4) & oneBitMask) == 1;
            return t;
        }

        private WallTile ParseWallTile(byte data)
        {
            return new WallTile
            {
                AllowNorthRandomDecoration = ((data >> 3) & oneBitMask) == 1,
                AllowEastRandomDecoration = ((data >> 2) & oneBitMask) == 1,
                AllowSouthRandomDecoration = ((data >> 1) & oneBitMask) == 1,
                AllowWestRandomDecoration = (data & oneBitMask) == 1
            };
        }

        private FloorTile ParseFloorTile(byte data)
        {
            return new FloorTile
            {
                AllowRandomDecoration = ((data >> 3) & oneBitMask) == 1
            };
        }

        private PitTile ParsePitTile(byte data)
        {
            return new PitTile
            {
                IsImaginary = (data & oneBitMask) == 1,
                IsVisible = ((data >> 2) & oneBitMask) == 1,
                IsOpen = ((data >> 3) & oneBitMask) == 1
            };
        }

        private StairsTile ParseStairsTile(byte data)
        {
            return new StairsTile
            {
                Orientation = (Orientation)((data >> 3) & oneBitMask),
                Direction = (VerticalDirection)((data >> 2) & oneBitMask)
            };
        }

        private DoorTile ParseDoorTile(byte data)
        {
            return new DoorTile
            {
                State = (DoorState)(data & threeBitsMask),
                Orientation = (Orientation)((data >> 3) & oneBitMask)
            };
        }

        private TeleporterTile ParseTeleporterTile(byte data)
        {
            return new TeleporterTile
            {
                IsVisible = ((data >> 2) & oneBitMask) == 1,
                IsOpen = ((data >> 3) & oneBitMask) == 1
            };
        }

        private TrickTile ParseTrickTile(byte data)
        {
            return new TrickTile
            {
                IsImaginary = (data & oneBitMask) == 1,
                IsOpen = ((data >> 2) & oneBitMask) == 1,
                AllowRandomDecoration = ((data >> 3) & oneBitMask) == 1
            };
        }

        #endregion

    }
}