/*{
  "CATEGORIES": [
    "Animation",
    "ID-based",
    "Projection"
  ],
  "DESCRIPTION": "Animates elements in a scene based on their ID values, perfect for projection mapping",
  "INPUTS": [
    {
      "NAME": "inputImage",
      "TYPE": "image"
    },
    {
      "NAME": "progress",
      "LABEL": "Animation Progress",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
        "NAME": "loops",
        "LABEL": "Loops",
        "TYPE": "long",
        "DEFAULT": 1,
        "MIN": 1,
        "MAX": 20
    },
    {
        "NAME": "loopType",
        "LABEL": "Loop Type",
        "TYPE": "long",
        "DEFAULT": 0,
        "VALUES": [
            0,
            1
        ],
        "LABELS": [
            "Sine",
            "Ramp"
        ]
    },
    {
      "NAME": "noiseProgress",
      "LABEL": "Noise Progress",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "noiseAmplitude",
      "LABEL": "Noise Amplitude",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0.0,
      "MAX": 5.0
    },
    {
      "NAME": "noisePeriod",
      "LABEL": "Noise Period",
      "TYPE": "float",
      "DEFAULT": 3.0,
      "MIN": 0.1,
      "MAX": 10.0
    },
    {
      "NAME": "gamma",
      "LABEL": "Gamma/Power",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
        "NAME": "noiseSource",
        "LABEL": "Noise Source",
        "TYPE": "float",
        "DEFAULT": 0.5
    }
  ]
}*/

// Simple 2D noise function
float hash(vec2 p) {
    p = fract(p * vec2(123.34f, 456.21f));
    p += dot(p, p + 45.32f);
    return fract(p.x * p.y);
}

// Value noise with smooth interpolation
float valueNoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);

    // Four corners in 2D of a tile
    float a = hash(i);
    float b = hash(i + vec2(1.0f, 0.0f));
    float c = hash(i + vec2(0.0f, 1.0f));
    float d = hash(i + vec2(1.0f, 1.0f));

    // Smooth interpolation
    vec2 u = f * f * (3.0f - 2.0f * f);

    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

// Generate a looping animation position based on progress
vec2 loopingNoiseOffset(float progress) {
    // Convert progress to radians (0-1 to 0-2π)
    float angle = progress * 6.28318f;

    // Create a circular motion that loops perfectly
    return vec2(cos(angle), sin(angle));
}

void main() {
    // Get normalized coordinates
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

    // Sample the ID from the input image
    vec4 inputColor = IMG_NORM_PIXEL(inputImage, uv);
    float id = inputColor.r; // Using the red channel for the ID

    // Generate a looping noise animation offset based on noiseProgress
    vec2 noiseOffset = loopingNoiseOffset(noiseProgress);

    // Only process if the pixel has some opacity
    if(inputColor.a > 0.001f) {
        // Generate noise based on ID, position, and progress-based offset
        float noiseVal = valueNoise(mix(uv * 2.0f - 1.0f, vec2(0, id * 10.0f), noiseSource) * noisePeriod + noiseOffset * 5.0f);

        float combinedProgress = progress + id * loops + noiseVal * noiseAmplitude;

        // Calculate animation value based on progress and ID
        // This creates a wave effect that moves through different IDs as progress increases
        float animMod = mod(combinedProgress, 1.0f);

        float animWave;

        if(loopType == 0) {
            // Sine wave
            animWave = sin(animMod * 6.28318f) * 0.5f + 0.5f;
        } else {
            // Ramp wave
            animWave = animMod;
        }

        // Apply gamma correction
        animWave = pow(animWave, pow(2, 7 * (gamma - 0.5f)));

        // Output the result
        gl_FragColor = vec4(animWave, animWave, animWave, inputColor.a);
    } else {
        // If the pixel is transparent, output transparent black
        gl_FragColor = vec4(0.0f, 0.0f, 0.0f, 0.0f);
    }
}
