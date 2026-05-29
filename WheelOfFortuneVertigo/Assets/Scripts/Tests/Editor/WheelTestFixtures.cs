using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    internal static class WheelTestFixtures
    {
        public static WheelGameSettings CreateSettings(WheelTestObjectScope scope)
        {
            var settings = scope.Track(ScriptableObject.CreateInstance<WheelGameSettings>());
            var uiCopy = scope.Track(ScriptableObject.CreateInstance<WheelUiCopyCatalog>());
            var resolveCatalog = scope.Track(ScriptableObject.CreateInstance<WheelSpinResolveCatalog>());
            var motionCatalog = scope.Track(ScriptableObject.CreateInstance<WheelOutcomePopupMotionCatalog>());

            uiCopy.RefreshLookups();

            Sprite icon = CreateTestSprite(scope);
            var skinCatalog = scope.Track(ScriptableObject.CreateInstance<WheelSkinCatalog>());
            var tierSprites = WheelSkinTierSprites.Create(icon, icon);
            skinCatalog.Configure(tierSprites, tierSprites, tierSprites);

            RewardDefinition bomb = RewardDefinition.Create("bomb", "Bomb", icon, 1, RewardTier.Common, Color.red);
            RewardDefinition gold = RewardDefinition.Create("gold", "Gold", icon, 2, RewardTier.Common, Color.yellow);

            settings.SetBombReward(bomb);
            settings.ReplaceRewardPool(ZoneType.Standard, gold);
            settings.ReplaceRewardPool(ZoneType.Safe, gold);
            settings.ReplaceRewardPool(ZoneType.Super, gold);

            settings.ConfigureSkinCatalog(skinCatalog);
            settings.ConfigureUiCopy(uiCopy);
            settings.ConfigureSpinResolveCatalog(resolveCatalog);
            settings.ConfigureOutcomePopupMotionCatalog(motionCatalog);
            settings.InitializeRuntime();
            return settings;
        }

        public static WheelGameState CreateState(WheelTestObjectScope scope, out WheelGameSettings settings)
        {
            return CreateState(scope, out settings, UnityRandomSource.Shared);
        }

        public static WheelGameState CreateState(
            WheelTestObjectScope scope,
            out WheelGameSettings settings,
            IRandomSource randomSource)
        {
            settings = CreateSettings(scope);

            var gameObject = scope.Track(new GameObject("WheelGameStateTest"));
            WheelGameState state = gameObject.AddComponent<WheelGameState>();
            state.InitializeRuntime(settings, randomSource);
            state.Restart();
            return state;
        }

        public static WheelSpinResult CreateRewardResult(WheelTestObjectScope scope, int sliceIndex, string rewardId, string displayName, int amount)
        {
            var reward = RewardDefinition.Create(rewardId, displayName, CreateTestSprite(scope), amount, RewardTier.Common, Color.yellow);
            reward.CacheRuntimeText("Won {0}");

            var slice = new WheelSliceDefinition();
            slice.ApplySlot(false, reward, null, amount, Color.yellow, displayName, amount > 1);
            return new WheelSpinResult(sliceIndex, slice);
        }

        public static WheelSpinResult CreateBombResult(WheelTestObjectScope scope, int sliceIndex)
        {
            RewardDefinition bomb = RewardDefinition.Create("bomb", "Bomb", CreateTestSprite(scope), 1, RewardTier.Common, Color.red);
            bomb.CacheRuntimeText("Won {0}");

            var slice = new WheelSliceDefinition();
            slice.ApplySlot(true, bomb, null, 1, Color.red, "Bomb", false);
            return new WheelSpinResult(sliceIndex, slice);
        }

        private static Sprite CreateTestSprite(WheelTestObjectScope scope)
        {
            var texture = scope.Track(new Texture2D(1, 1, TextureFormat.RGBA32, false));
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            return scope.Track(Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)));
        }
    }
}
