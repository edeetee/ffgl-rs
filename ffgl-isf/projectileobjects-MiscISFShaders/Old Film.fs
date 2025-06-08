/*{
  "CREDIT": "ProjectileObjects / VIDVOX",
  "DESCRIPTION": "Old film look with sepia false-color, gamma, flicker, optional solid moving film scratches, vignette, and grain overlays.",
  "ISFVSN": "2",
  "CATEGORIES": ["Filter"],
  "INPUTS": [
    {
      "NAME": "inputImage",
      "TYPE": "image"
    },
    {
      "NAME": "shadowColor",
      "TYPE": "color",
      "DEFAULT": [0.2, 0.15, 0.07, 1.0],
      "DESCRIPTION": "False color for shadow regions (e.g. sepia shadows)"
    },
    {
      "NAME": "highlightColor",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.9, 0.7, 1.0],
      "DESCRIPTION": "False color for highlight regions (e.g. sepia highlights)"
    },
    {
      "NAME": "gamma",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 3.0,
      "DESCRIPTION": "Gamma correction"
    },
    {
      "NAME": "flickerAmount",
      "TYPE": "float",
      "DEFAULT": 0.1,
      "MIN": 0.0,
      "MAX": 1.0,
      "DESCRIPTION": "How strong the flicker effect is"
    },
    {
      "NAME": "flickerRate",
      "TYPE": "float",
      "DEFAULT": 10.0,
      "MIN": 1.0,
      "MAX": 15.0,
      "DESCRIPTION": "Frequency of the flicker effect (up to 15 FPS)"
    },
    {
      "NAME": "randomness",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0,
      "DESCRIPTION": "Adds randomness to flicker and scratches"
    },
    {
      "NAME": "enableScratches",
      "TYPE": "bool",
      "DEFAULT": false,
      "DESCRIPTION": "Toggle film scratch lines overlay"
    },
    {
      "NAME": "scratchNumber",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 1.0,
      "MAX": 20.0,
      "DESCRIPTION": "Number of scratch lines"
    },
    {
      "NAME": "scratchSpeed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 5.0,
      "DESCRIPTION": "Speed of scratch movement"
    },
    {
      "NAME": "scratchBrightness",
      "TYPE": "float",
      "DEFAULT": 0.9,
      "MIN": 0.0,
      "MAX": 1.0,
      "DESCRIPTION": "Brightness of the scratches"
    },
    {
      "NAME": "scratchThickness",
      "TYPE": "float",
      "DEFAULT": 0.01,
      "MIN": 0.001,
      "MAX": 0.05,
      "DESCRIPTION": "Thickness of the scratch lines"
    },
    {
      "NAME": "scratchFlickerRate",
      "TYPE": "float",
      "DEFAULT": 5.0,
      "MIN": 1.0,
      "MAX": 15.0,
      "DESCRIPTION": "Flicker rate of scratches (up to 15 FPS)"
    },
    {
      "NAME": "scratchWiggle",
      "TYPE": "float",
      "DEFAULT": 0.05,
      "MIN": 0.0,
      "MAX": 0.1,
      "DESCRIPTION": "Amount of scratch wiggle along X-axis"
    },
    {
      "NAME": "scratchPosition",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": -0.5,
      "MAX": 0.5,
      "DESCRIPTION": "Base X position of the scratches"
    },
    {
      "NAME": "vignetteAmount",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0,
      "DESCRIPTION": "Amount of vignette (dark edges)"
    },
    {
      "NAME": "enableGrain",
      "TYPE": "bool",
      "DEFAULT": true,
      "DESCRIPTION": "Toggle film grain overlay"
    },
    {
      "NAME": "grainAmount",
      "TYPE": "float",
      "DEFAULT": 0.2,
      "MIN": 0.0,
      "MAX": 1.0,
      "DESCRIPTION": "Intensity of film grain"
    }
  ]
}*/

#ifdef GL_ES
precision mediump float;
#endif

// Calculate the luminance of a color (perceived brightness)
float luminance(vec3 c) {
    return dot(c, vec3(0.299, 0.587, 0.114));
}

// Generate pseudo-random noise based on a seed
float randNoise(float seed) {
    return fract(sin(seed) * 43758.5453);
}

// Generate vertical scratch lines
float scratchOverlay(vec2 uv, float scratchCount, float randomness, float speed, float thickness, float flickerRate, float wiggle, float position) {
    float lines = 0.0;

    // Scroll value to move scratches vertically
    float scroll = mod(TIME * speed, 1.0);

    // Loop to generate multiple scratches
    for (float i = 0.0; i < 20.0; i++) {
        if (i >= scratchCount) break; // Limit to the desired number of scratches

        // Generate a random X position for the scratch with wiggle
        float xPos = position + randNoise(i * 0.1234 + randomness) * wiggle * sin(TIME + i);

        // Calculate the distance of the pixel from the scratch line
        float lineDist = abs(uv.x - xPos);

        // Create a solid vertical line with adjustable thickness
        float lineVal = smoothstep(thickness, 0.0, lineDist);

        // Apply flicker effect to scratches
        float flicker = step(0.5, randNoise(i + floor(TIME * flickerRate)));

        // Combine the flicker effect with the scratch line
        lineVal *= flicker;
        lines = max(lines, lineVal); // Keep the strongest line value
    }
    return lines;
}

void main() {
    // Normalize pixel coordinates
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

    // Sample the input image at the current coordinates
    vec4 col = IMG_NORM_PIXEL(inputImage, uv);

    // Apply false color effect
    float lum = luminance(col.rgb); // Calculate luminance of the pixel
    vec3 falseColor = mix(shadowColor.rgb, highlightColor.rgb, lum); // Map luminance to shadow/highlight colors
    col = vec4(falseColor, col.a);

    // Apply gamma correction
    col.rgb = pow(col.rgb, vec3(1.0 / max(0.0001, gamma)));

    // Apply flicker effect by modulating brightness over time
    float flicker = 1.0 + flickerAmount * sin(TIME * flickerRate);
    col.rgb *= flicker;

    // Apply film scratches if enabled
    if (enableScratches) {
        float lines = scratchOverlay(uv, scratchNumber, randomness, scratchSpeed, scratchThickness, scratchFlickerRate, scratchWiggle, scratchPosition);
        float scratchEffect = mix(1.0, 0.0, scratchBrightness); // Adjust scratch brightness
        col.rgb = mix(col.rgb, vec3(scratchEffect), lines); // Overlay scratches on the image
    }

    // Apply vignette effect
    vec2 centeredUV = uv - 0.5;
    centeredUV.x *= RENDERSIZE.x / RENDERSIZE.y; // Adjust for aspect ratio
    float dist = length(centeredUV); // Distance from the center
    float vig = smoothstep(0.7, 0.3, dist * (1.0 + vignetteAmount)); // Create vignette gradient
    col.rgb *= mix(1.0, vig, vignetteAmount); // Darken edges based on vignette amount

    // Apply grain effect if enabled
    if (enableGrain) {
        float noiseVal = randNoise(gl_FragCoord.x * gl_FragCoord.y + TIME * 50.0); // Generate random noise
        noiseVal = (noiseVal - 0.5) * grainAmount; // Adjust grain intensity
        col.rgb += noiseVal; // Add noise to the color
    }

    // Output the final color
    gl_FragColor = vec4(col.rgb, 1.0);
}