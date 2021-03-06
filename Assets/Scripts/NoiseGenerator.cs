using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// This class handle the generation of noise in order to be used to create various HeigtMaps
public static class NoiseGenerator
{
  static private float[,] noiseMap;
 static FastNoiseLite noise = new FastNoiseLite();
 private static bool warping = false;

 private static int mapWidth_;
 private static int mapHeight_;
 private static float scale_;
 private static int octaves_;
 private static  Vector2[] octaveOffsets_;
 public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset,HeightMapGenerator.NoiseType noiseType, bool applyRidges)
 {
    mapWidth_ = mapWidth;
    mapHeight_ = mapHeight;
    scale /= 60f;
    scale_ = scale;
    octaves_ = octaves;
      noiseMap = new float[mapWidth, mapHeight];
      // Use the seed to generate the same map regarding the given seed
      System.Random prng = new System.Random (seed);
      // Give an offset to each octave in order to sample them from random different locations
      Vector2[] octaveOffsets = new Vector2[octaves];
      for (int i = 0; i < octaves; i++) {
         float offsetX = prng.Next (-100000, 100000) + offset.x;
         float offsetY = prng.Next (-100000, 100000) + offset.y;
         octaveOffsets [i] = new Vector2 (offsetX, offsetY);
      }
      octaveOffsets_ = octaveOffsets;

      // Create and configure FastNoise object
      // Currently reducing the scale because of the apparent huge difference in size between
      // the unity perlin noise and Fastnoiselite opensimplexnoise
      if (noiseType == HeightMapGenerator.NoiseType.SIMPLEXNOISE)
      {
         //scale /= 60f;
         noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
      }
      else if (noiseType == HeightMapGenerator.NoiseType.PERLINNOISE)
      {
         noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
      }
      else if (noiseType == HeightMapGenerator.NoiseType.CELLULARNOISE)
      {
         noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
      }
      else if (noiseType == HeightMapGenerator.NoiseType.CUBICNOISE)
      {
         noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
      }
      else if (noiseType == HeightMapGenerator.NoiseType.VALUENOISE)
      {
         noise.SetNoiseType(FastNoiseLite.NoiseType.Value);
      }
      
      if (scale <= 0) {
         scale = 0.0001f;
      }
      
      

      float maxNoiseHeight = float.MinValue;
      float minNoiseHeight = float.MaxValue;
      
      
      
      

      // Calculate the half witdh and half heigth in order to zoom in the center of the map instead of into the corner
      float halfWidth = mapWidth *0.5f;
      float halfHeight = mapHeight *0.5f;
      float noiseValue;

      for (int y = 0; y < mapHeight; y++) {
         for (int x = 0; x < mapWidth; x++) {

            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;
           
           
            
               

               for (int i = 0; i < octaves; i++) {
                  float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                  float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;
                  Vector2 point = new Vector2(sampleX, sampleY);
                  // float noiseValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;
                  noiseValue = noise.GetNoise(point.x, point.y);

                  if (applyRidges)
                  {
                     float n = Mathf.Abs(noiseValue) * amplitude;
                     n = 1f - n;
                     noiseHeight += n * n;
                  }
                  else
                  {
                     noiseHeight += noiseValue * amplitude;
                  }
               
                  amplitude *= persistance;
                  frequency *= lacunarity;
               }

               if (noiseHeight > maxNoiseHeight) {
                  maxNoiseHeight = noiseHeight;
               } else if (noiseHeight < minNoiseHeight) {
                  minNoiseHeight = noiseHeight;
               }
               
               noiseMap [x, y] = noiseHeight;
            
         }
      }
      // This loop purpose is to get the values back to 0-1
      for (int y = 0; y < mapHeight; y++) {
         for (int x = 0; x < mapWidth; x++) {
            noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
         }
      }

      return noiseMap;
   }

   static float SimpleFBM(Vector2 point)
   {
      float noiseSum = 0;
      float amplitude = 1;
      float frequency = 1;
      
      
      for (int i = 0; i < octaves_; ++i)
      {
         float sampleX = (point.x-mapWidth_ *0.5f) / scale_ * frequency + octaveOffsets_[i].x;
         float sampleY = (point.y-mapHeight_ * 0.5f) / scale_ * frequency + octaveOffsets_[i].y;
         noiseSum += noise.GetNoise(sampleX * frequency, sampleY * frequency) * amplitude;
         frequency *= 2;
         amplitude *= 0.5f;
      }

      return noiseSum;
   }

  public static float Warping(Vector2 point)
   {
      Vector2 offset1 = new Vector2(4.2f, 0.5f);
      Vector2 offset2 = new Vector2(5.2f, 1.3f);

      Vector2 q = new Vector2(SimpleFBM(point + offset1),
                              SimpleFBM(point + offset2));

     // return SimpleFBM(point + 5.0f * q);
      return SimpleFBM(point +q * 5.0f);

   }
   
}