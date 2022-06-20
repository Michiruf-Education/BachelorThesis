// using UnityEngine;
// using Random = System.Random;
//
// public class WindErosion : IErosion
// {
//     private readonly WindErosionSettings settings;
//     private readonly Random random;
//
//     public WindErosion(WindErosionSettings settings, int seed)
//     {
//         this.settings = settings;
//         random = new Random(seed);
//     }
//
//     public void ErodeStep(FloatField heightMap, FloatField hardnessMap)
//     {
//         // TODO maxParticleLifetime makes no sense, this should be used in the particle
//         //      And the iteration for cycles is already done in the method that calls ErodeStep
//         for (var lifetime = 0; lifetime < settings.maxParticleLifetime; lifetime++)
//         {
//             // Create wind droplet at random point on map
//             var particle = new WindParticle(
//                 random.Next(0, heightMap.width - 1),
//                 random.Next(0, heightMap.height - 1));
//
//             // Perform Wind Erosion Cycle
//             particle.Handle(heightMap, windpath, sediment, track, dim, scale);
//         }
//
//         // Update Path
//         var lrate = 0.01;
//         for (var i = 0; i < dim.x * dim.y; i++)
//             windpath[i] = (1.0 - lrate) * windpath[i];
//     }
// }
