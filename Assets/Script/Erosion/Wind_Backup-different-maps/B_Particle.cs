// using MyBox;
// using UnityEngine;
//
// public struct WindParticle
// {
//     public Vector2 position
//     {
//         get => positionValue;
//         set
//         {
//             positionValue = value;
//             positionOfCell = new Vector2Int((int) positionValue.x, (int) positionValue.y);
//             positionOfCellAsFloat = positionOfCell.ToVector2();
//             positionRelativeToCell = positionValue - positionOfCellAsFloat;
//         }
//     }
//     private Vector2 positionValue;
//     public float height;
//     public Vector3 velocity;
//     public float sediment;
//
//     public Vector2Int positionOfCell { get; private set; }
//     public Vector2 positionOfCellAsFloat { get; private set; }
//     public Vector2 positionRelativeToCell { get; private set; }
//
//     public WindParticle(float x, float y) : this()
//     {
//         position = new Vector2(x, y);
//     }
//
//     public void Handle(FloatField heightMap)
//     {
//         while (true)
//         {
//             var heightOfCell = heightMap.GetValue(positionOfCell.x, positionOfCell.y);
//
//             // Particles under the heightmap are moved upwards
//             if (height < heightOfCell)
//                 height = heightOfCell;
//
//             // When flying
//             if (height > heightOfCell)
//             {
//                 velocity.y -= dt * 0.01; //Gravity
//             }
//             // When sliding
//             else
//             {
//                 var normal = heightMap.CalculateNormal(positionOfCell.x, positionOfCell.y);
//                 velocity += dt * Vector3.Cross(Vector3.Cross(velocity, normal), normal);
//             }
//
//             // Accelerate by prevailing wind
//             velocity += 0.1f * dt * (pspeed - speed);
//
//             // Update Position
//             position += dt * new Vector2(velocity.x, velocity.z);
//             height += dt * velocity.y;
//
//             // Prevent out of bounds
//             if (!glm::all(glm::greaterThanEqual(pos, glm::vec2(0))) ||
//                 !glm::all(glm::lessThan((glm::ivec2) pos, dim)))
//                 break;
//
//
//             // Update heightOfCell
//             heightOfCell = heightMap.GetValue(positionOfCell.x, positionOfCell.y);
//             if (height > heightOfCell)
//             {
//                 Flying();
//             }
//             else
//             {
//                 Abrasion();
//                 Suspension();
//             }
//
//
//             //Stop when particle has no speed (equilibrium movement)
//             if (velocity.magnitude < 0.01)
//                 break;
//         }
//     }
//
//     private void Abrasion()
//     {
//         if (s[ind] <= 0)
//         {
//             double force = glm::length(speed) * (s[nind] + h[nind] - height);
//
//             s[ind] = 0;
//             h[ind] -= dt * abrasion * force * sediment;
//             s[ind] += dt * abrasion * force * sediment;
//         }
//     }
//
//     private void Suspension()
//     {
//         // Include: if not Abrasion AND ...
//         if (s[ind] > dt * suspension * force)
//         {
//             s[ind] -= dt * suspension * force;
//             sediment += dt * suspension * force;
//             Cascade(ind, h, s, dim);
//         }
//         else s[ind] = 0; //Set to zero
//     }
//
//     private void Flying()
//     {
//         sediment -= dt * suspension * sediment;
//
//         s[nind] += 0.5 * dt * suspension * sediment;
//         s[ind] += 0.5 * dt * suspension * sediment;
//
//         Cascade(nind, h, s, dim);
//         Cascade(ind, h, s, dim);
//     }
//
//     private void Cascade()
//     {
//         const int size = dim.x * dim.y;
//
//         // Neighbor Position Offsets (8-Way)
//         var nx = new[]
//         {
//             -1, -1, -1, 0, 0, 1, 1, 1
//         };
//         var ny = new[] {
//             -1,0,1,-1,1,-1,0,1
//         };
//
//         // Neighbor Indices (8-Way
//         var n = new []{
//             i - dim.y - 1, i - dim.y, i - dim.y + 1, i - 1, i + 1,
//             i + dim.y - 1, i + dim.y, i + dim.y + 1
//         };
//
//         glm::ivec2 ipos;
//
//         //Iterate over all Neighbors
//         for (int m = 0; m < 8; m++)
//         {
//             ipos = pos;
//
//             //Neighbor Out-Of-Bounds
//             if (n[m] < 0 || n[m] >= size) continue;
//             if (ipos.x + nx[m] >= dim.x || ipos.y + ny[m] >= dim.y) continue;
//             if (ipos.x + nx[m] < 0 || ipos.y + ny[m] < 0) continue;
//
//             //Pile Size Difference and Excess
//             float diff = (h[i] + s[i]) - (h[n[m]] + s[n[m]]);
//             float excess = abs(diff) - roughness;
//
//             //Stable Configuration
//             if (excess <= 0) continue;
//
//             float transfer;
//
//             //Pile is Larger
//             if (diff > 0)
//                 transfer = min(s[i], excess / 2.0);
//
//             //Neighbor is Larger
//             else
//                 transfer = -min(s[n[m]], excess / 2.0);
//
//             //Perform Transfer
//             s[i] -= dt * settling * transfer;
//             s[n[m]] += dt * settling * transfer;
//         }
//     }
// }
