using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMinecraft.Model
{
    /// <summary>
    /// Represents the various packets used to communicate
    /// between server and client.
    /// </summary>
    /// <remarks></remarks>
    public enum PacketID : byte
    {
        /// <summary>
        /// The KeepAlive packet preventing a client from Timing out
        /// </summary>
        /// <remarks>The client must respond with the same KeepAlive ID field</remarks>
        KeepAlive = 0x00,
        /// <summary>
        /// Sent after the Handshake to the server as login data
        /// </summary>
        LoginRequest = 0x01,
        /// <summary>
        /// The first packet sent to the server.  Used for Authentication, contains the username
        /// </summary>
        Handshake = 0x02,
        /// <summary>
        /// Sent to update the chatbox generally prepended with the username if coming from the server
        /// </summary>
        /// <remarks>Must be 100 characters or shorter</remarks>
        ChatUpdate = 0x03,
        /// <summary>
        /// Sent to update the clients in world time
        /// </summary>
        /// <remarks>Server to client only</remarks>
        TimeUpdate = 0x04,
        /// <summary>
        /// 5 packets sent of this per entity each containing data for what is being held
        /// </summary>
        EntityEquipment = 0x05,
        /// <summary>
        /// Sent by the server after login to define the spawn point (compass pointing location), which be updated at any time
        /// </summary>
        /// <remarks>Server to client only</remarks>
        SpawnPosition = 0x06,
        /// <summary>
        /// Sent to the server when a client attacks or right clicks an entity
        /// </summary>
        /// <remarks>Client to server only (not fully understood)</remarks>
        UseEntity = 0x07,
        /// <summary>
        /// Sent by the server to set or update the health.
        /// </summary>
        /// <remarks>Server to client only</remarks>
        UpdateHealth = 0x08,
        /// <summary>
        /// Sent from the client for when the player presses the "Respawn" button after death.  Also used by the server to change dimensions.
        /// </summary>
        Respawn = 0x09,
        /// <summary>
        /// Sent from the client to indicate whether the player is on ground (walking/swimming), or airborne (jumping/falling). 
        /// </summary>
        /// <remarks>Client to server only</remarks>
        Player = 0x0A,
        /// <summary>
        /// Sent to update the position of the client
        /// </summary>
        /// <remarks>Client to server (player-controlled movement)</remarks>
        PlayerPosition = 0x0B,
        /// <summary>
        /// Sent to update the player's look direction
        /// </summary>
        /// <remarks>Client to server (player-controlled movement)</remarks>
        PlayerLook = 0x0C,
        /// <summary>
        /// A combination of the PlayerLook and PlayerPosition packets
        /// </summary>
        PlayerPositionAndLook = 0x0D,
        /// <summary>
        /// Sent when a player mines a block.
        /// </summary>
        /// <remarks>Official Minecraft servers accept packets only within a 6 block radius</remarks>
        PlayerDigging = 0x0E,
        /// <summary>
        /// Sent when a player places a block
        /// </summary>
        PlayerBlockPlacement = 0x0F,
        /// <summary>
        /// Sent from the client when the player changes the slot selection 
        /// </summary>
        HoldingChange = 0x10,
        /// <summary>
        /// Associated with players using beds
        /// </summary>
        /// <remarks>Server to client</remarks>
        UseBed = 0x11,
        /// <summary>
        /// Sent whenever an entity needs to change animations
        /// </summary>
        Animation = 0x12,
        /// <summary>
        /// Sent at least when crouching, leaving a bed, or sprinting.
        /// </summary>
        /// <remarks>Client to server only</remarks>
        EntityAction = 0x13,
        /// <summary>
        /// Spawns a named entity on the client.
        /// </summary>
        /// <remarks>Players are currently the only named entity.</remarks>
        NamedEntitySpawn = 0x14,
        /// <summary>
        /// Sent to the client when an item on the ground comes within visual range of the player.
        /// </summary>
        /// <remarks>This packet is not used to pick up items.</remarks>
        PickupSpawn = 0x15,
        /// <summary>
        /// Sent from the server when a player picks up an item on the ground.
        /// </summary>
        /// <remarks>Server to client</remarks>
        CollectItem = 0x16,
        /// <summary>
        /// Sent to the client when an Object or Vehicle is made
        /// </summary>
        /// <remarks>Server to client</remarks>
        AddObjectOrVehicle = 0x17,
        /// <summary>
        /// Sent to the client when an mob is spawned
        /// </summary>
        /// <remarks>Server to client</remarks>
        MobSpawn = 0x18,
        /// <summary>
        /// Sent to spawn a painting entity
        /// </summary>
        EntityPainting = 0x19,
        /// <summary>
        /// Sent to the client to spawn one or more experience orbs in a specific location
        /// </summary>
        ExperienceOrb = 0x1A,
        /// <summary>
        /// Updates player stance
        /// </summary>
        /// <remarks>Unused by the 1.1 client</remarks>
        StanceUpdate = 0x1B,
        /// <summary>
        /// Updates how fast an object is going
        /// </summary>
        EntityVelocity = 0x1C,
        /// <summary>
        /// Sent to notify that an entity has been destroyed
        /// </summary>
        DestroyEntity = 0x1D,
        /// <summary>
        /// Sent to the client to represent information about an entity
        /// </summary>
        Entity = 0x1E,
        /// <summary>
        /// Sent to notify the movement of an entity based on its current position
        /// </summary>
        EntityRelativeMove = 0x1F,
        /// <summary>
        /// This is sent when an entity rotates, such as the player entity looking
        /// </summary>
        EntityLook = 0x20,
        /// <summary>
        /// This packet is sent by the server when an entity rotates and moves.
        /// </summary>
        EntityLookAndRelativeMove = 0x21,
        /// <summary>
        /// This is sent by the server when an entity is moving more than 4 blocks at a time. 
        /// </summary>
        EntityTeleport = 0x22,
        /// <summary>
        /// This is sent by the server to change the direction an entity's head appears to look
        /// </summary>
        EntityHeadLook = 0x23,
        // ...
        /// <summary>
        /// This sends a packet to the client notifying it of the entities status. IE: Hurt, dead, player eating
        /// </summary>
        EntityStatus = 0x26,
        /// <summary>
        /// This packet is sent when the player attaches to an entity such as a minecart
        /// </summary>
        AttachEntity = 0x27,
        /// <summary>
        /// This packet contains information about a certain entity, which is defined by its ID
        /// </summary>
        EntityMetadata = 0x28,
        /// <summary>
        /// Mainly for use with player, determines what effects are given to a player such as digspeed and regeneration
        /// </summary>
        EntityEffect = 0x29,
        /// <summary>
        /// This is used to remove the effects caused when packet 0x29 has been sent
        /// </summary>
        RemoveEntityEffect = 0x2A,
        /// <summary>
        /// Sent to the client upon any experience changes
        /// </summary>
        Experience = 0x2B,
        // ...
        /// <summary>
        /// This packet is used to notify the client either to initialize or unload the following chunk(s)
        /// </summary>
        PreChunk = 0x32,
        /// <summary>
        /// this is date about the map and the chunks are usually 16^3
        /// </summary>
        MapChunk = 0x33,
        /// <summary>
        /// This is called when multiple blocks have been changed within a region at one time
        /// </summary>
        MultiBlockChange = 0x34,
        /// <summary>
        /// This packet is sent when only one block has changed to a new type of block
        /// </summary>
        BlockChange = 0x35,
        /// <summary>
        /// This packet is sent when blocks such as chests have been used in order to show their animation
        /// </summary>
        BlockAction = 0x36,
        // ...
        /// <summary>
        /// This packet is sent when an explosion occurs either by creeper or TNT
        /// </summary>
        Explosion = 0x3C,
        /// <summary>
        /// Sent to the client when it is to play a sound.
        /// </summary>
        SoundOrParticleEffect = 0x3D,
        // ...
        /// <summary>
        /// This packet is currently sent when either a bed cant be used as a spawn point or when the raining state changes 
        /// </summary>
        NewOrInvalidState = 0x46,
        /// <summary>
        /// This is sent to the client to identify the whereabouts of a thunderbolt strike
        /// </summary>
        Thunderbolt = 0x47,
        // ...
        /// <summary>
        /// This is sent to the client when it should open an inventory window 
        /// </summary>
        OpenWindow = 0x64,
        /// <summary>
        /// This packet is sent when a window has been forcibly closed (Chest has been destroyed)
        /// </summary>
        CloseWindow = 0x65,
        /// <summary>
        /// This is sent when the player has clicked a slot 
        /// </summary>
        WindowClick = 0x66,
        /// <summary>
        /// Sent when an item in a slot is either added or removed
        /// </summary>
        SetSlot = 0x67,
        /// <summary>
        /// Sent when an item in a slot, including crafting and equipped armour, is either added or removed
        /// </summary>
        WindowItems = 0x68,
        /// <summary>
        /// Used to increase the progress of the furnace and enchantment table
        /// </summary>
        UpdateProgressBar = 0x69,
        /// <summary>
        /// This packet is sent from the client and server to tell whether it was accepted, rejected or whether there was a conflict due to lag.
        /// </summary>
        Transaction = 0x6A,
        /// <summary>
        /// This packet will be sent when the player drops an item into their quickbar or picks it up from it in creative mode
        /// </summary>
        CreativeInventoryAction = 0x6B,
        /// <summary>
        /// Packet is sent to the server containing the position of the enchantment
        /// </summary>
        EnchantItem = 0x6C,
        // ...
        /// <summary>
        /// On creation of a sign and upon pressing Done, this packets it sent that displays the text on the sign
        /// </summary>
        UpdateSign = 0x82,
        /// <summary>
        /// Sends complex data about maps
        /// </summary>
        ItemData = 0x83,
        // ...
        /// <summary>
        /// Increases the statistic chosend by its ID
        /// </summary>
        IncrementStatistic = 0xC8,
        /// <summary>
        /// this is a packet sent to the client in order to update the player list (when Tab is pressed)
        /// </summary>
        PlayerListItem = 0xC9,
        // ...
        /// <summary>
        /// This packet is used by mods and plugins to send any data that they may need
        /// </summary>
        PluginMessage = 0xFA,
        // ...
        /// <summary>
        /// This packet is used by the client to get a kick response with server information, in order to list the information on the multiplayer menu.
        /// </summary>
        ServerListPing = 0xFE,
        /// <summary>
        /// Sent by the server prior to a disconnection/kick, the packet will contain a reason the client was kicked.
        /// </summary>
        Disconnect = 0xFF,
    }

    /// <summary>
    /// The numerical representation of a mob's type
    /// </summary>
    /// <remarks></remarks>
    public enum MobType
    {
        /// <summary>
        /// ID 50 represents a Creeper
        /// </summary>
        Creeper = 50,
        /// <summary>
        /// ID 51 represents a Skeleton
        /// </summary>
        Skeleton = 51,
        /// <summary>
        /// ID 52 represents a Spider
        /// </summary>
        Spider = 52,
        /// <summary>
        /// ID 53 represents a Giant Zombie
        /// </summary>
        GiantZombie = 53,
        /// <summary>
        /// ID 54 represents a Zombie
        /// </summary>
        Zombie = 54,
        /// <summary>
        /// ID 55 represents a Slime
        /// </summary>
        Slime = 55,
        /// <summary>
        /// ID 56 represents a Ghast
        /// </summary>
        Ghast = 56,
        /// <summary>
        /// ID 57 represents a Zombie Pigman
        /// </summary>
        ZombiePigman = 57,
        /// <summary>
        /// ID 58 represents an Enderman
        /// </summary>
        Enderman = 58,
        /// <summary>
        /// ID 59 represents a Cave Spider
        /// </summary>
        CaveSpider = 59,
        /// <summary>
        /// ID 60 represents a Silverfish
        /// </summary>
        Silverfish = 60,
        /// <summary>
        /// ID 61 represents a Blaze
        /// </summary>
        Blaze = 61,
        /// <summary>
        /// ID 62 represents a Magmacube
        /// </summary>
        MagmaCube = 62,
        /// <summary>
        /// ID 63 represents a EnderDragon
        /// </summary>
        EnderDragon = 63,
        // ...
        /// <summary>
        /// ID 90 represents a Pig
        /// </summary>
        Pig = 90,
    }

    /// <summary>
    /// Represents an effect used in
    /// SoundOrParticleEffectPacket
    /// </summary>
    /// <remarks></remarks>
    public enum SoundOrParticleEffect
    {
        /// <summary>
        /// Right click sound?
        /// </summary>
        Click2 = 1000,
        /// <summary>
        /// Left click sound?
        /// </summary>
        Click1 = 1001,
        /// <summary>
        /// Sound made when a bow has been fired
        /// </summary>
        BowFire = 1002,
        /// <summary>
        /// Sound made when a door has been open/shut
        /// </summary>
        DoorToggle = 1003,
        /// <summary>
        /// Sound made when a fire has been extinguished?
        /// </summary>
        Extinguish = 1004,
        /// <summary>
        /// 
        /// </summary>
        RecordPlay = 1005,
        /// <summary>
        /// 
        /// </summary>
        Charge = 1007,
        /// <summary>
        /// 
        /// </summary>
        Fireball = 1008,
        /// <summary>
        /// 
        /// </summary>
        Fireball2 = 1009,
        /// <summary>
        /// The Sound of smoke
        /// </summary>
        Smoke = 2000,
        /// <summary>
        ///  Sound made when a block has been broken
        /// </summary>
        BlockBreak = 2001,
        /// <summary>
        ///
        /// </summary>
        SplashPotion = 2002,
        /// <summary>
        /// The sound of a portal
        /// </summary>
        Portal = 2003,
        /// <summary>
        /// 
        /// </summary>
        Blaze = 2004,
    }

    /// <summary>
    /// Represents the various ways a
    /// block can be transparent.
    /// </summary>
    /// <remarks></remarks>
    public enum BlockOpacity
    {
        /// <summary>
        /// 
        /// </summary>
        CubeSolid,
        /// <summary>
        /// 
        /// </summary>
        NonCubeSolid,
        /// <summary>
        /// 
        /// </summary>
        NonSolidMechanism,
        /// <summary>
        /// 
        /// </summary>
        NonSolid,
        /// <summary>
        /// 
        /// </summary>
        Plant,
        /// <summary>
        /// 
        /// </summary>
        Fluid,
        /// <summary>
        /// 
        /// </summary>
        Opaque
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    internal enum Tree : byte { Normal = 0, Spruce, Birch,Jungle };
    internal enum Coal : byte { Coal = 0, Charcoal };
    internal enum Jukebox : byte { Nothing = 0, GoldDisk, GreenDisk };
    /// <summary>
    /// Wool colors and their metadata.
    /// </summary>
    public enum Wool : byte {
        /// <summary>
        /// White wool MetaData = 0
        /// </summary>
        White = 0,
        /// <summary>
        /// Orange wool MetaData = 1
        /// </summary>
        Orange = 1,
        /// <summary>
        /// Magenta wool MetaData = 2
        /// </summary>
        Magenta = 2,
        /// <summary>
        /// Light Blue wool MetaData = 3
        /// </summary>
        LightBlue = 3,
        /// <summary>
        /// Yellow wool MetaData = 4
        /// </summary>
        Yellow = 4,
        /// <summary>
        /// Light Green wool MetaData = 5
        /// </summary>
        LightGreen = 5,
        /// <summary>
        /// Pink wool MetaData = 6
        /// </summary>
        Pink = 6,
        /// <summary>
        /// Gray wool MetaData = 7
        /// </summary>
        Gray = 7,
        /// <summary>
        /// Light Gray wool MetaData = 8
        /// </summary>
        LightGray = 8,
        /// <summary>
        /// Cyan wool MetaData = 9
        /// </summary>
        Cyan = 9,
        /// <summary>
        /// Purple wool MetaData = 10
        /// </summary>
        Purple = 10,
        /// <summary>
        /// Blue wool MetaData = 11
        /// </summary>
        Blue = 11,
        /// <summary>
        /// Brown wool MetaData = 12
        /// </summary>
        Brown = 12,
        /// <summary>
        /// Dark Green wool MetaData = 13
        /// </summary>
        DarkGreen = 13,
        /// <summary>
        /// Red wool MetaData = 14
        /// </summary>
        Red = 14,
        /// <summary>
        /// Black wool MetaData = 15
        /// </summary>
        Black = 15
    };
    internal enum Dye : byte { InkSac = 0, RoseRed, CactusGreen, CocoaBeans, LapisLazuli, PurpleDye, CyanDye, LightGrayDye, GrayDye, PinkDye, LimeDye, DandelionYellow, LightBlueDye, MagentaDye, OrangeDye, BoneMeal };
    internal enum Torch : byte { South = 0x1, North = 0x2, West = 0x3, East = 0x4, Standing = 0x5 };
    internal enum Rail : byte { EastWest = 0x0, NorthSouth = 0x1, AscendingSouth = 0x2, AscendingNorth = 0x3, AscendingEast = 0x4, AscendingWest = 0x5, CornerNorthEast = 0x6, CornerSouthEast = 0x7, CornerSouthWest = 0x8, CornerNorthWest = 0x9 };
    internal enum Ladder : byte { East = 0x2, West = 0x3, North = 0x4, South = 0x5 };
    internal enum StairDirections : byte { South = 0x0, North = 0x1, West = 0x2, East = 0x3 };
    internal enum Levers : byte { WallSouth = 0x1, WallNorth = 0x2, WallWest = 0x3, WallEast = 0x4, GroundWest = 0x5, GroundSouth = 0x6, LeverOn = 0x8 };
    internal enum Doors : byte { NorthEast = 0x0, SouthEast = 0x1, SouthWest = 0x2, NorthWest = 0x3, TopHalf = 0x8, Open = 0x4 };
    internal enum Buttons : byte { Pressed = 0x8, West = 0x1, East = 0x2, South = 0x3, North = 0x4 };
    internal enum SignPost : byte { West = 0x0, West_NorthWest = 0x1, NorthWest = 0x2, North_NorthWest = 0x3, North = 0x4, North_NorthEast = 0x5, NorthEast = 0x6, East_NorthEast = 0x7, East = 0x8, East_SouthEast = 0x9, SouthEast = 0xA, South_SouthEast = 0xB, South = 0xC, South_SouthWest = 0xD, SouthWest = 0xE, West_SouthWest = 0xF };
    internal enum WallSigns : byte { East = 0x2, West = 0x3, North = 0x4, South = 0x5 };
    internal enum Furnace : byte { East = 0x2, West = 0x3, North = 0x4, South = 0x5 };
    internal enum Dispenser : byte { East = 0x2, West = 0x3, North = 0x4, South = 0x5 };
    internal enum Chests : byte { East = 0x2, West = 0x3, North = 0x4, South = 0x5 };
    internal enum Pumpkin : byte { East = 0x2, West = 0x3, North = 0x4, South = 0x5 };
    internal enum PressurePlate : byte { NotPressed = 0x0, Pressed = 0x1 };
    internal enum Slab : byte { Stone = 0x0, SandStone = 0x1, Wooden = 0x2, Cobblestone = 0x3, Brick = 0x4, StoneBrick = 0x5, Stone2 = 0x6 };
    internal enum Bed : byte { Isfoot = 0x8, West = 0x0, North = 0x1, East = 0x2, South = 0x3 };
    internal enum Repeater : byte { East = 0x0, South = 0x1, West = 0x2, North = 0x3, Tick1 = 0x5, Tick2 = 0x6, Tick3 = 0x7, Tick4 = 0x8 };
    internal enum TallGrass : byte { DeadShrub = 0x0, TallGrass = 0x1, Fern = 0x2 };
    internal enum TrapDoors : byte { West = 0x0, East = 0x1, South = 0x2, North = 0x3, Open = 0x4 };
    internal enum Piston : byte { Down = 0x0, Up = 0x1, East = 0x2, West = 0x3, North = 0x4, South = 0x5, On = 0x8 };
    internal enum PistonExtension : byte { Down = 0x0, Up = 0x1, East = 0x2, West = 0x3, North = 0x4, South = 0x5, Sticky = 0x8 };
    internal enum StoneBrick : byte { Normal = 0x0, Mossy = 0x1, Cracked = 0x2 }
    internal enum HugeMushroom : byte { Fleshy = 0x0, CornerNorthWest = 0x1, SideNorth = 0x2, CornerNorthEast = 0x3, SideWest = 0x4, Top = 0x5, SideEast = 0x6, CornerSouthWest = 0x7, SideSouth = 0x8, CornerSouthEast = 0x9, Stem = 0xA }
    internal enum Vines : byte { Top = 0x0, West = 0x1, North = 0x2, East = 0x4, South = 0x8 }
    internal enum FenceGate : byte { West = 0x0, North = 0x1, East = 0x2, South = 0x3, Open = 0x4 }
    internal enum Directions : byte { Bottom = 0, Top = 1, East = 2, West = 3, North = 4, South = 5 };
    /// <summary>
    /// Used in the NewOrInvalidStatePacket to
    /// change weather, game mode, or enter credits.
    /// </summary>
    public enum NewOrInvalidState : byte {
        /// <summary>
        /// Sent to the client when respawning when the respawn bed is not found
        /// </summary>
        InvalidBed = 0,
        /// <summary>
        /// Sent to the client when rain starts in-game
        /// </summary>
        BeginRain = 1,
        /// <summary>
        /// Sent to the client when rain stops in-game
        /// </summary>
        EndRain = 2,
        /// <summary>
        /// Sent to the client when the gamemode of the client has changed
        /// </summary>
        ChangeGameMode = 3,
        /// <summary>
        /// Sent to the client to indicate they have entered the credits
        /// </summary>
        EnterCredits = 4
    }
    /// <summary>
    /// Represents an action being undertaken by
    /// an entity (usually a player)
    /// </summary>
    public enum EntityAction : byte {
        /// <summary>
        /// Crouching, Data = 1
        /// </summary>
        Crouch = 1,
        /// <summary>
        /// Uncrouching, Data = 2
        /// </summary>
        UnCrouch = 2,
        /// <summary>
        /// Leaving bed, Data = 3
        /// </summary>
        LeaveBed = 3,
        /// <summary>
        /// Starting sprinting, Data = 4
        /// </summary>
        StartSprinting = 4,
        /// <summary>
        /// Stopping sprinting, Data = 5
        /// </summary>
        StopSprinting = 5,
    }
    /// <summary>
    /// Represents an animation a client has requested
    /// be displayed.
    /// </summary>
    public enum Animation : byte {
        /// <summary>
        /// No animation at all
        /// </summary>
        NoAnimation = 0x00,
        /// <summary>
        /// Swing arms
        /// </summary>
        SwingArm = 0x01,
        /// <summary>
        /// Animation for getting damaged
        /// </summary>
        Damage = 0x02,
        /// <summary>
        /// animation for leaving a bed
        /// </summary>
        LeaveBed = 0x03,
        /// <summary>
        /// animation for eating food
        /// </summary>
        EatFood = 0x05,
        /// <summary>
        /// unknown animation
        /// </summary>
        Unknown = 102,
        /// <summary>
        /// crouching animation
        /// </summary>
        Crouch = 104,
        /// <summary>
        /// returning to normal stance animation
        /// </summary>
        UnCrouch = 105
    }
    /// <summary>
    /// Represents each kind of server-controlled window
    /// that can be opened.
    /// </summary>
    public enum Windows : byte { Chest = 0, Workbench = 1, Furnace = 2, Dispenser = 3, EnchantmentTable = 4 }
    /// <summary>
    /// Represnts the status of an entity.
    /// </summary>
    public enum EntityStatus : byte { Hurt = 2, Dead = 3, Taming = 6, Tamed = 7, ShakingWater = 8, EatingAccepted = 9 }
}
