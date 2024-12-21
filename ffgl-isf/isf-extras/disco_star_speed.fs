/*
{
  "CATEGORIES" : [
    "Automatically Converted",
    "GLSLSandbox"
  ],
  "INPUTS" : [

  ],
  "DESCRIPTION" : "Automatically converted from http:\/\/glslsandbox.com\/e#60124.0"
}
*/

/*
 * Original shader from: https://www.shadertoy.com/view/wsKXRK
 */

#ifdef GL_ES
precision highp float;
#endif
// glslsandbox uniforms

// shadertoy emulation
#define iTime TIME
#define iResolution RENDERSIZE
const vec4 iMouse = vec4(0.5f);

// --------[ Original ShaderToy begins here ]---------- //
// V-Drop - Del 19/11/2019 - (Tunnel mix - Enjoy)
// vertical version: https://www.shadertoy.com/view/tdGXWm
#define PI 3.14159

float vDrop(vec2 uv, float t) {
    uv.x = uv.x * 128.0f;						// H-Count
    float dx = fract(uv.x);
    uv.x = floor(uv.x);
    uv.y *= 0.05f;							// stretch
    float o = sin(uv.x * 215.4f);				// offset
    float s = cos(uv.x * 33.1f) * .3f + .7f;			// speed
    float trail = mix(905.0f, 25.0f, s);			// trail length
    float yv = fract(uv.y + t * s + o) * trail;
    yv = 1.0f / yv;
    yv = smoothstep(0.0f, 1.0f, yv * yv);
    yv = sin(yv * PI) * (s * 5.0f);
    float d2 = sin(dx * PI);
    return yv * (d2 * d2);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 p = (fragCoord.xy - 0.5f * iResolution.xy) / iResolution.y;
    float d = length(p) + 0.1f;
    p = vec2(atan(p.x, p.y) / PI, 2.5f / d);
    if(iMouse.z > 0.5f)
        p.y *= 0.5f;
    float t = iTime * 0.4f;
    vec3 col = vec3(1.55f, 0.65f, .225f) * vDrop(p, t);	// red
    col += vec3(0.55f, 0.75f, 1.225f) * vDrop(p, t + 0.33f);	// blue
    col += vec3(0.45f, 1.15f, 0.425f) * vDrop(p, t + 0.66f);	// green
    fragColor = vec4(col * (d * d), 1.0f);
}
// --------[ Original ShaderToy ends here ]---------- //

void main(void) {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}