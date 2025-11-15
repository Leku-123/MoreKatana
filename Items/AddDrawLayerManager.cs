using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;

namespace MoreKatana.Items
{
    public static class AddDrawLayerManager
    {
        private static uint MaxCalls => 1000;

        private static IAddDrawLayer[] AddDrawLayers;

        public static void DrawAdditiveLayers(ref PlayerDrawSet drawinfo)
        {
            List<IAddDrawLayer> CallList = new List<IAddDrawLayer>();

            Player drawPlayer = drawinfo.drawPlayer;

            for (int i = 0; i < drawPlayer.inventory.Length; i++)
            {
                var mI = drawPlayer.inventory[i].ModItem;
                if (mI is IAddDrawLayer)
                    CallList.Add(mI as IAddDrawLayer);
            }

            for (int i = 0; i < MaxCalls; i++)
            {
                if (AddDrawLayers[i] != null)
                    CallList.Add(AddDrawLayers[i]);
            }

            if (CallList.Count > 0)
            {
                foreach (IAddDrawLayer addDrawLayers in CallList)
                    addDrawLayers.AdditiveDrawLayer(ref drawinfo);
            }
        }

        public static int ManualAppend(IAddDrawLayer addDrawLayers)
        {
            for (int i = 0; i < AddDrawLayers.Length; i++)
            {
                if (AddDrawLayers[i] == null)
                {
                    AddDrawLayers[i] = addDrawLayers;
                    return i;
                }
            }
            throw new NullReferenceException("Max Calls Reached. Calm the fuck down");
        }

        public static void Remove(int Index) => AddDrawLayers[Index] = null;
        public static void Load() => AddDrawLayers = new IAddDrawLayer[MaxCalls];
        public static void Unload() => AddDrawLayers = null;
    }

    public interface IAddDrawLayer
    {
        void AdditiveDrawLayer(ref PlayerDrawSet drawinfo);
    }
}