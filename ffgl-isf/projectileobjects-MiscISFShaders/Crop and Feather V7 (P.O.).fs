/*{
  "ISFVSN": 2,
  "CATEGORIES": ["Geometry Adjustment"],
  "CREDIT": "ProjectileObjects modified, based on VIDVOX crop",
  "DESCRIPTION": "ISF crop V4 adjustment created to replace VIDVOX Quartz Composer FX, includes four sided feathering",
  "INPUTS": [
    {
      "NAME": "inputImage",
      "TYPE": "image"
    },
    {
      "NAME": "Top",
      "TYPE": "float",
      "DEFAULT": 1.0
    },
    {
      "NAME": "Bottom",
      "TYPE": "float",
      "DEFAULT": 0.0
    },
    {
      "NAME": "Left",
      "TYPE": "float",
      "DEFAULT": 0.0
    },
    {
      "NAME": "Right",
      "TYPE": "float",
      "DEFAULT": 1.0
    },
    {
      "NAME": "TopFeather",
      "TYPE": "float",
      "DEFAULT": 0.0
    },
    {
      "NAME": "BottomFeather",
      "TYPE": "float",
      "DEFAULT": 0.0
    },
    {
      "NAME": "LeftFeather",
      "TYPE": "float",
      "DEFAULT": 0.0
    },
    {
      "NAME": "RightFeather",
      "TYPE": "float",
      "DEFAULT": 0.0
    }
  ],
  "PASSES": [
    {
      "NAME": "crop",
      "TARGET": "myOutputColor",
      "MAIN": "main"
    }
  ]
}
*/

void main() {
    vec2 uv = isf_FragNormCoord;

    // Define cropping boundaries
    float topCrop = Top;
    float bottomCrop = Bottom;
    float leftCrop = Left;
    float rightCrop = Right;

    // Start with full alpha
    float alpha = 1.0;

    // Feathered top fade
    if (uv.y > topCrop) {
        alpha *= 0.0;
    } else if (uv.y > topCrop - TopFeather) {
        alpha *= smoothstep(topCrop, topCrop - TopFeather, uv.y);
    }

    // Feathered bottom fade
    if (uv.y < bottomCrop) {
        alpha *= 0.0;
    } else if (uv.y < bottomCrop + BottomFeather) {
        alpha *= smoothstep(bottomCrop, bottomCrop + BottomFeather, uv.y);
    }

    // Feathered left fade
    if (uv.x < leftCrop) {
        alpha *= 0.0;
    } else if (uv.x < leftCrop + LeftFeather) {
        alpha *= smoothstep(leftCrop, leftCrop + LeftFeather, uv.x);
    }

    // Feathered right fade
    if (uv.x > rightCrop) {
        alpha *= 0.0;
    } else if (uv.x > rightCrop - RightFeather) {
        alpha *= smoothstep(rightCrop, rightCrop - RightFeather, uv.x);
    }

    vec4 color = IMG_NORM_PIXEL(inputImage, uv);
    color.a *= alpha;

    gl_FragColor = color;
}