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

            if (Attributes?["igniteItem"]?.Exists == true) return EnumIgniteState.NotIgnitablePreventDefault;

            float igniteTime = Attributes?["igniteTime"]?.AsFloat(0.75f) ?? 0.75f;

            if (secondsIgniting > igniteTime)
            {
                return EnumIgniteState.IgniteNow;
            }

            return EnumIgniteState.Ignitable;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null) return base.OnBlockInteractStart(world, byPlayer, blockSel);
            
            BlockEntityMoatBomb bebomb = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityMoatBomb;
            if (bebomb == null || bebomb.IsLit) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            string igniteItem = Attributes?["igniteItem"]?.AsString();
            if (igniteItem != null)
            {
                ItemSlot handSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
                bool isMatch = false;
                if (igniteItem == "empty")
                {
                    if (handSlot.Empty) isMatch = true;
                }
                else if (!handSlot.Empty)
                {
                    isMatch = WildcardUtil.Match(new AssetLocation(igniteItem), handSlot.Itemstack.Collectible.Code);
                }

                if (isMatch)
                {
                    bebomb.StartInteractAnimation();
                    return true;
                }
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null) return false;

            BlockEntityMoatBomb bebomb = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityMoatBomb;
            if (bebomb == null || bebomb.IsLit) return false;

            string igniteItem = Attributes?["igniteItem"]?.AsString();
            if (igniteItem != null)
            {
                if (world.Side == EnumAppSide.Client)
                {
                    if (bebomb.PlayInteractParticles)
                    {
                        BlockEntityMoatBomb.smallSparks.MinPos.Set(blockSel.Position.X + 0.45, blockSel.Position.Y + 0.53, blockSel.Position.Z + 0.45);
                        world.SpawnParticles(BlockEntityMoatBomb.smallSparks);
                    }
                }

                float igniteTime = Attributes?["igniteTime"]?.AsFloat(0.75f) ?? 0.75f;
                if (secondsUsed > igniteTime)
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null) return;
            string igniteItem = Attributes?["igniteItem"]?.AsString();
            float igniteTime = Attributes?["igniteTime"]?.AsFloat(0.75f) ?? 0.75f;
            
            if (igniteItem != null)
            {
                BlockEntityMoatBomb bebomb = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityMoatBomb;
                if (bebomb != null)
                {
                    bebomb.StopInteractAnimation();
                    if (secondsUsed >= igniteTime - 0.05f)
                    {
                        bebomb.OnIgnite(byPlayer);
                    }
                }
            }
        }

        public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            float igniteTime = Attributes?["igniteTime"]?.AsFloat(0.75f) ?? 0.75f;
            if (secondsIgniting < igniteTime - 0.05f) return;

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

            float jsonMultiplier = Attributes?["dropQuantityMultiplier"]?.AsFloat(1) ?? 1f;

            if (jsonMultiplier <= 0)
            {
                world.BulkBlockAccessor.SetBlock(0, pos);
                return;
            }

            base.OnBlockExploded(world, pos, explosionCenter, blastType, ignitedByPlayerUid);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            BlockEntityMoatBomb bebomb = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityMoatBomb;
            if (bebomb != null && bebomb.CascadeLit) return System.Array.Empty<ItemStack>();

            float jsonMultiplier = Attributes?["dropQuantityMultiplier"]?.AsFloat(1) ?? 1f;
            if (jsonMultiplier <= 0)
            {
                return System.Array.Empty<ItemStack>();
            }

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
