    using UnityEngine;

    public class TesselationDisplacementMaterial
    {
        // private const string HeightMapParameterName = "_HeightMap";
        private const string HeightMapParameterName = "_ParallaxMap";

        public static void Apply(MeshRenderer target, Texture2D heightmap)
        {
            var material = target.sharedMaterial;

            var heightmapTextureId =  Shader.PropertyToID(HeightMapParameterName);
            material.SetTexture(heightmapTextureId, heightmap);

            // material.mainTexture = heightmap;
        }
    }
