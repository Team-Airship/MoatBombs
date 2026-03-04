using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace MoatBomb
{
    public class BlockMoatBomb : Block, IIgnitable
    {
        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "moatBombInteractions", () =>
            {
                List<ItemStack> canIgniteStacks = BlockBehaviorCanIgnite.CanIgniteStacks(api, false);

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        MouseButton = EnumMouseButton.Right,
                        ActionLangCode = "blockhelp-bomb-ignite",
                        Itemstacks = canIgniteStacks.ToArray(),
                        GetMatchingStacks = (wi, bs, es) => {
                            BlockEntityMoatBomb bebomb = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityMoatBomb;
                            return bebomb == null || bebomb.IsLit ? null : wi.Itemstacks;
                        }
                    }
                };
            });
        }

        EnumIgniteState IIgnitable.OnTryIgniteStack(EntityAgent byEntity, BlockPos pos, ItemSlot slot, float secondsIgniting)
        {
            return EnumIgniteState.NotIgnitable;
        }

        public EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
        {
            BlockEntityMoatBomb bebomb = byEntity.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityMoatBomb;
            if (bebomb == null || bebomb.IsLit) return EnumIgniteState.NotIgnitablePreventDefault;

            if (secondsIgniting > 0.75f)
            {
                return EnumIgniteState.IgniteNow;
            }

            return EnumIgniteState.Ignitable;
        }

        public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            if (secondsIgniting < 0.7f) return;

            handling = EnumHandling.PreventDefault;

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            if (byPlayer == null) return;

            BlockEntityMoatBomb bebomb = byPlayer.Entity.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityMoatBomb;
            bebomb?.OnIgnite(byPlayer);
        }

        public override void OnBlockExploded(IWorldAccessor world, BlockPos pos, BlockPos explosionCenter, EnumBlastType blastType, string ignitedByPlayerUid)
        {
            BlockEntityMoatBomb bebomb = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityMoatBomb;
            bebomb?.OnBlockExploded(pos, ignitedByPlayerUid);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            BlockEntityMoatBomb bebomb = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityMoatBomb;
            if (bebomb != null && bebomb.CascadeLit) return System.Array.Empty<ItemStack>();

            var stacks = base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
            
            return stacks;
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (interactions == null) return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
