/*{
  "DESCRIPTION": "Just a simple relaxing sine combination",
  "CREDIT": "Converted from Shadertoy: Gentle Rings",
  "CATEGORIES": [
    "Generators"
  ],
  "INPUTS": [
    {
      "NAME": "progress",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 5.0
    }
  ]
}
*/

#define PI 3.141596

vec3 a = vec3(0.5f, 0.5f, 0.5f);
vec3 b = vec3(0.5f, 0.5f, 0.5f);
vec3 c = vec3(1.0f, 1.0f, 1.0f);
vec3 d = vec3(0.00f, 0.33f, 0.67f);

// iq color mapper
vec3 colorMap(float t) {
    return (a + b * cos(2.f * PI * (c * t + d)));
}

void mainImage(out vec4 o, in vec2 i) {
    vec2 uv = i / RENDERSIZE.xy;
    uv -= 0.5f;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;

    float r = length(uv);
    float a = atan(uv.y, uv.x);

    float ring = 1.5f + 0.8f * sin(PI * 0.25f * (TIME * speed + 10.0f * progress));

    float kr = 0.5f - 0.5f * cos(7.f * PI * r);
    vec3 kq = 0.5f - 0.5f * sin(ring * vec3(30.f, 29.3f, 28.6f) * r - 6.0f * (TIME * speed + 10.0f * progress) + PI * vec3(-0.05f, 0.5f, 1.0f));
    vec3 c = kr * (0.1f + kq * (1.f - 0.5f * colorMap(a / PI))) * (0.5f + 0.5f * sin(11.f * a + 22.5f * r));

    // Output to screen
    o.rgb = mix(vec3(0.0f, 0.0f, 0.2f), c, 0.85f);
}

void main() {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}