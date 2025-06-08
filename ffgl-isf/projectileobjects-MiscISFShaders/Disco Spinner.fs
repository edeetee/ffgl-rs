/*{
  "CREDIT": "ProjectileObjects",
  "DESCRIPTION": "4x Disco Spinner FX: Creates 4 copies of the input image, positioned at cardinal directions with cropping and aspect controls.",
  "ISFVSN": "2",
  "INPUTS": [
    {
      "NAME": "inputImage",
      "TYPE": "image"
    },
    {
      "NAME": "distance",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "spinX",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": -3.14,
      "MAX": 3.14
    },
    {
      "NAME": "spinY",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": -3.14,
      "MAX": 3.14
    },
    {
      "NAME": "spinZ",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": -3.14,
      "MAX": 3.14
    },
    {
      "NAME": "cropX",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 0.5
    },
    {
      "NAME": "cropY",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 0.5
    },
    {
      "NAME": "squeezeX",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.5,
      "MAX": 2.0
    },
    {
      "NAME": "squeezeY",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.5,
      "MAX": 2.0
    }
  ]
}*/

#ifdef GL_ES
precision mediump float;
#endif

// Rotation matrix for X, Y, and Z axes
mat3 rotationMatrix(float angleX, float angleY, float angleZ) {
  mat3 rotX = mat3(
    1.0, 0.0, 0.0,
    0.0, cos(angleX), -sin(angleX),
    0.0, sin(angleX), cos(angleX)
  );

  mat3 rotY = mat3(
    cos(angleY), 0.0, sin(angleY),
    0.0, 1.0, 0.0,
    -sin(angleY), 0.0, cos(angleY)
  );

  mat3 rotZ = mat3(
    cos(angleZ), -sin(angleZ), 0.0,
    sin(angleZ), cos(angleZ), 0.0,
    0.0, 0.0, 1.0
  );

  return rotZ * rotY * rotX;
}

// Adjust UV coordinates for aspect ratio scaling, cropping, and squeezing
vec2 adjustUV(vec2 uv) {
  // Apply cropping
  uv = uv * (1.0 - vec2(cropX, cropY)) + vec2(cropX, cropY) * 0.5;

  // Apply squeezing
  uv.x *= squeezeX;
  uv.y *= squeezeY;

  // Clamp to [0, 1] to avoid sampling outside the image bounds
  uv = clamp(uv, 0.0, 1.0);
  return uv;
}

void main() {
  vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
  uv = uv * 2.0 - 1.0; // Normalize to [-1, 1]

  // Aspect ratio correction
  uv.x *= RENDERSIZE.x / RENDERSIZE.y;

  // Rotation matrix
  mat3 rotation = rotationMatrix(spinX, spinY, spinZ);

  // Center positions for the four spinners
  vec3 north = vec3(0.0, 1.0, 0.0);
  vec3 south = vec3(0.0, -1.0, 0.0);
  vec3 east = vec3(1.0, 0.0, 0.0);
  vec3 west = vec3(-1.0, 0.0, 0.0);

  // Apply rotation to the spinner positions
  north = rotation * north;
  south = rotation * south;
  east = rotation * east;
  west = rotation * west;

  // Calculate UV coordinates for each spinner
  vec2 northUV = uv - north.xy * distance;
  vec2 southUV = uv - south.xy * distance;
  vec2 eastUV = uv - east.xy * distance;
  vec2 westUV = uv - west.xy * distance;

  // Adjust UV coordinates for cropping, clamping, and squeezing
  northUV = adjustUV(0.5 + 0.5 * northUV);
  southUV = adjustUV(0.5 + 0.5 * southUV);
  eastUV = adjustUV(0.5 + 0.5 * eastUV);
  westUV = adjustUV(0.5 + 0.5 * westUV);

  // Fetch the texture colors for each spinner
  vec4 colorNorth = IMG_NORM_PIXEL(inputImage, northUV);
  vec4 colorSouth = IMG_NORM_PIXEL(inputImage, southUV);
  vec4 colorEast = IMG_NORM_PIXEL(inputImage, eastUV);
  vec4 colorWest = IMG_NORM_PIXEL(inputImage, westUV);

  // Combine the colors (use max to layer the images without blending)
  vec4 finalColor = max(max(colorNorth, colorSouth), max(colorEast, colorWest));

  gl_FragColor = finalColor;
}