using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace MoatBomb
{
    public class MoatBombModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            
            api.RegisterBlockClass("BlockMoatBomb", typeof(BlockMoatBomb));
            api.RegisterBlockEntityClass("BlockEntityMoatBomb", typeof(BlockEntityMoatBomb));
        }
    }
}
