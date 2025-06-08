/*{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "CREDIT": "",
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/MtcGD7 by CaliCoastReplay.  A big thank you to \tFabriceNeyret2  who told me how to achieve this more vivid look!  Positively conflagarative!  Try it in fire rain config!\n\nRemix 1 here:  https://www.shadertoy.com/view/llc3DM\nOriginal here:  \nhttps://www.shadertoy.com/view/XsXXRN",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "DEFAULT": 1.2,
            "DESCRIPTION": "Adjusts the speed of the fire.",
            "MAX": 5,
            "MIN": 0.1,
            "NAME": "Speed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "DESCRIPTION": "Adjusts the direction of fire movement. (-1.0: Left, 0.0: Center, 1.0: Right)",
            "MAX": 1,
            "MIN": -1,
            "NAME": "Direction",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "NAME": "Negative",
            "TYPE": "bool"
        }
    ],
    "ISFVSN": "2"
}
*/



vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// Function to generate random value based on input coordinates
float rand(vec2 n) {
    return fract(sin(cos(dot(n, vec2(12.9898,12.1414)))) * 83758.5453);
}

// Function to generate noise based on input coordinates
float noise(vec2 n) {
    const vec2 d = vec2(0.0, 1.0);
    vec2 b = floor(n), f = smoothstep(vec2(0.0), vec2(1.0), fract(n));
    return mix(mix(rand(b), rand(b + d.yx), f.x), mix(rand(b + d.xy), rand(b + d.yy), f.x), f.y);
}

// Function to generate fractal brownian motion based on input coordinates
float fbm(vec2 n) {
    float total = 0.0, amplitude = 1.0;
    for (int i = 0; i < 5; i++) {
        total += noise(n) * amplitude;
        n += n * 1.7;
        
     if (Negative == true)
       {amplitude *= -0.47;}
     else  amplitude *= 0.47;
       
    }
    return total;
}

void main() {
    // Define color gradients for fire
    const vec3 c1 = vec3(0.5, 0.0, 0.1);
    const vec3 c2 = vec3(0.9, 0.1, 0.0);
    const vec3 c3 = vec3(0.2, 0.1, 0.7);
    const vec3 c4 = vec3(1.0, 0.9, 0.1);
    const vec3 c5 = vec3(0.1);
    const vec3 c6 = vec3(0.9);

    float alpha = 1.0;

    // Adjusting the speed and direction based on input controls
    vec2 speed = vec2(1.2, 0.1); // Default speed
    float shift = 1.327 + sin(TIME * 2.0) / 2.4;
    float dist = 3.5 - sin(TIME * 0.4) / 1.89;

    speed.x = Speed; // Adjusting speed based on input control

    // Adjusting direction based on input control
    float directionOffset = Direction * RENDERSIZE.x / 2.0;
    
    vec2 p = (gl_FragCoord.xy + vec2(directionOffset, 0.0)) * dist / RENDERSIZE.xx;
    p.x -= TIME / 1.1;

    // Generating fractal brownian motion for fire effect
    float q = fbm(p - TIME * 0.01 + 1.0 * sin(TIME) / 10.0);
    float qb = fbm(p - TIME * 0.002 + 0.1 * cos(TIME) / 5.0);
    float q2 = fbm(p - TIME * 0.44 - 5.0 * cos(TIME) / 7.0) - 6.0;
    float q3 = fbm(p - TIME * 0.9 - 10.0 * cos(TIME) / 30.0) - 4.0;
    float q4 = fbm(p - TIME * 2.0 - 20.0 * sin(TIME) / 20.0) + 2.0;
    q = (q + qb - .4 * q2 - 2.0 * q3 + .6 * q4) / 3.8;

    // Applying turbulence to fire
    vec2 r = vec2(fbm(p + q / 2.0 + TIME * speed.x - p.x - p.y), fbm(p + q - TIME * speed.y));

    // Combining color gradients based on fractal brownian motion
    vec3 c = mix(c1, c2, fbm(p + r)) + mix(c3, c4, r.x) - mix(c5, c6, r.y);

    // Applying color modulation and adjustments
    vec3 color = vec3(c * cos(shift * gl_FragCoord.y / RENDERSIZE.y));
    color += .05;
    color.r *= .8;
    vec3 hsv = rgb2hsv(color);
    hsv.y *= hsv.z * 1.1;
    hsv.z *= hsv.y * 1.13;
    hsv.y = (2.2 - hsv.z * .9) * 1.20;
    color = hsv2rgb(hsv);

    // Setting the final color with alpha
    gl_FragColor = vec4(color.x, color.y, color.z, alpha);
}
