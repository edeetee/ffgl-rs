/*{
  "CREDIT": "ProjectileObjects / VIDVOX",
  "DESCRIPTION": "Three toggles to disable Red, Green, or Blue channels on the input image.",
  "ISFVSN": "2",
  "CATEGORIES": [
    "Filter"
  ],
  "INPUTS": [
    {
      "NAME": "inputImage",
      "TYPE": "image"
    },
    {
      "NAME": "enableRed",
      "TYPE": "bool",
      "LABEL": "Red",
      "DEFAULT": true
    },
    {
      "NAME": "enableGreen",
      "TYPE": "bool",
      "LABEL": "Green",
      "DEFAULT": true
    },
    {
      "NAME": "enableBlue",
      "TYPE": "bool",
      "LABEL": "Blue",
      "DEFAULT": true
    }
  ]
}*/

#ifdef GL_ES
precision mediump float;
#endif

void main() {
    // normalized UV coordinates
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    
    // sample input image
    vec4 c = IMG_NORM_PIXEL(inputImage, uv);

    // disable channels if toggles are off
    if (!enableRed)   { c.r = 0.0; }
    if (!enableGreen) { c.g = 0.0; }
    if (!enableBlue)  { c.b = 0.0; }

    gl_FragColor = c;
}