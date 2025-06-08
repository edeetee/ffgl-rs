/*{
  "CATEGORIES": ["Stylize", "Glitch", "Analog"],
  "DESCRIPTION": "Ultimate VHS Chaos with spikes, burnouts, and eject-style disruptions. Based off of V002's original Analog Glitch QC FX Fragment shader. Converted by ProjectileObjects.",
  "ISFVSN": "2",
  "LICENSE": "Custom License - See below",
  "INPUTS": [
    { "NAME": "inputImage", "TYPE": "image" },
    { "NAME": "barsTexture", "TYPE": "image" },
    { "NAME": "barsamount", "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "distortion", "TYPE": "float", "DEFAULT": 0.4, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "distortionScale", "TYPE": "float", "DEFAULT": 0.1, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "resolution", "TYPE": "float", "DEFAULT": 2.0, "MIN": 1.0, "MAX": 20.0 },
    { "NAME": "scanlinesAmount", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "useAnimatedSync", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "vsync", "TYPE": "float", "DEFAULT": 0.0, "MIN": -1.0, "MAX": 1.0 },
    { "NAME": "hsync", "TYPE": "float", "DEFAULT": 0.0, "MIN": -1.0, "MAX": 1.0 },
    { "NAME": "vsyncSpeed", "TYPE": "float", "DEFAULT": 0.2, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "hsyncSpeed", "TYPE": "float", "DEFAULT": 0.3, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "chromaticAberration", "TYPE": "float", "DEFAULT": 0.01, "MIN": 0.0, "MAX": 0.05 },
    { "NAME": "flickerAmount", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "grainAmount", "TYPE": "float", "DEFAULT": 0.05, "MIN": 0.0, "MAX": 0.3 },
    { "NAME": "analogNoiseAmount", "TYPE": "float", "DEFAULT": 0.02, "MIN": 0.0, "MAX": 0.1 },
    { "NAME": "analogNoiseSpeed", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 5.0 },
    { "NAME": "enableSpikes", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "enableBurnout", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "enableEjectEffect", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "enableVerticalJitter", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "enableColorBleed", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "enableDropouts", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "enableTapeWarp", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "enableGlitchJump", "TYPE": "bool", "DEFAULT": false },
    { "NAME": "chaosAmount", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "LABEL": "Master Chaos" }
  ]
}*/

/*
Copyright (c) 2025 ProjectileObjects LLC.

Permission is hereby granted to use, copy, modify, and distribute this software for personal, non-commercial, or educational use, provided the following conditions are met:

1. This software may not be used in, integrated into, or distributed with Resolume, Resolume Wire, or any other software owned or produced by Resolume BV.
2. This software may not be sold or monetized without express written permission from the author.
3. Attribution must be given to “ProjectileObjects” in any published use or adaptation.

THIS SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
*/

float random(vec2 st) {
  return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

void main() {
  vec2 uv = isf_FragNormCoord;
  vec2 texSize = RENDERSIZE;
  vec2 texcoord0 = uv * texSize;

  vec4 bars = texture2D(barsTexture, vec2(0.5 / texSize.x, 0.5));

  float stripe = mod(floor(texcoord0.y / resolution), 2.0);
  float scanlineMask = mix(1.0, 0.6, stripe);
  scanlineMask = mix(1.0, scanlineMask, scanlinesAmount);

  float syncV = useAnimatedSync ? sin(TIME * vsyncSpeed) * 0.01 : vsync;
  float syncH = useAnimatedSync ? cos(TIME * hsyncSpeed) * 0.01 : hsync;

  float yWave = sin((uv.y * 10.0) + TIME * analogNoiseSpeed * 1.5);
  float xWave = sin((uv.x * 8.0) + TIME * analogNoiseSpeed);
  float sweep = sin((uv.y + uv.x) * 4.0 + TIME * analogNoiseSpeed * 0.8);
  float analogNoise = (yWave * 0.5 + xWave * 0.3 + sweep * 0.2) * analogNoiseAmount;
  uv.x += analogNoise;

  if (enableTapeWarp || chaosAmount > 0.1) {
    float wrinkle = sin(uv.y * 60.0 + TIME * 30.0) * 0.003;
    uv.x += wrinkle * sin(TIME * 5.0);
  }

  if (enableGlitchJump || chaosAmount > 0.2) {
    float glitchTrigger = step(0.95, fract(sin(TIME * 3.0) * 10000.0));
    uv.x += glitchTrigger * 0.05 * chaosAmount;
  }

  if (enableVerticalJitter || chaosAmount > 0.2) {
    float band = floor(uv.y * 12.0);
    float bandJitter = sin(band * 3.5 + TIME * 10.0) * 0.002 * chaosAmount;
    uv.y += bandJitter;
  }

  // Original distortion based on luma scan
  vec2 point = uv;
  vec4 key = texture2D(inputImage, vec2(point.y, point.y));
  key += texture2D(inputImage, vec2(1.0 - point.y, 1.0 - point.y));
  key -= bars.r;
  float d = (key.r + key.g + key.b) / 3.0;
  uv.x -= d * distortion * distortionScale;

  // sync offset
  uv += vec2(syncH, syncV);
  uv = mod(uv, 1.0);

  float smearAmount = (enableColorBleed || chaosAmount > 0.2)
    ? sin(uv.y * 25.0 + TIME * 12.0) * 0.005 * chaosAmount : 0.0;

  vec2 redUV  = mod(uv + vec2(smearAmount, 0.0), 1.0);
  vec2 blueUV = mod(uv - vec2(smearAmount, 0.0), 1.0);

  vec4 red   = texture2D(inputImage, redUV  + vec2(chromaticAberration, 0.0));
  vec4 green = texture2D(inputImage, uv);
  vec4 blue  = texture2D(inputImage, blueUV + vec2(-chromaticAberration, 0.0));

  vec4 result = vec4(red.r, green.g, blue.b, 1.0);

  if (enableDropouts || chaosAmount > 0.4) {
    float dropoutZone = step(0.98, fract(sin(uv.y * 50.0 + TIME * 10.0) * 1000.0));
    result.rgb *= 1.0 - dropoutZone * 0.9 * chaosAmount;
  }

  result.rgb *= scanlineMask;
  float grain = (random(uv * texSize + TIME) - 0.5) * grainAmount;
  result.rgb += grain;

  float flicker = 1.0 - (sin(TIME * 60.0) * 0.5 + 0.5) * flickerAmount;
  result.rgb *= flicker;

  if (enableSpikes || chaosAmount > 0.5) {
    if (fract(sin(TIME * 5.0) * 10000.0) > 0.98) {
      result.rgb += 0.4 * chaosAmount;
    }
  }

  if (enableBurnout || chaosAmount > 0.7) {
    if (fract(sin(TIME * 7.0) * 5000.0) > 0.97) {
      result.rgb = vec3(1.0);
    }
  }

  if (enableEjectEffect || chaosAmount > 0.8) {
    if (fract(sin(TIME * 2.0) * 3000.0) > 0.985) {
      result.rgb *= 0.0;
    }
  }

  gl_FragColor = mix(result, bars * result, barsamount);
}
