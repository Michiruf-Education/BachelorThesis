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
//     public WindParticle(WindParticle particle) : this()
//     {
//         position = particle.position;
//         height = particle.height;
//         velocity = particle.velocity;
//         sediment = particle.sediment;
//     }
//
//     public int CalculateIndex(FloatField field)
//     {
//         return field.GetIndex(positionOfCell.x, positionOfCell.y);
//     }
//
//     public float CalculateTerrainHeight(FloatField groundMap, FloatField sedimentMap)
//     {
//         return groundMap.GetValue(positionOfCell.x, positionOfCell.y) +
//                sedimentMap.GetValue(positionOfCell.x, positionOfCell.y);
//     }
// }
