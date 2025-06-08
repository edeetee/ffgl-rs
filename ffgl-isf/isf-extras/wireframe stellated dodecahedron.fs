/*{
    "DESCRIPTION": "Stellated dodecahedron wireframe with radial blur (ISF 2.0, 2-pass, buffer example, Jesus what a pain)",
    "CREDIT": "ChunderFPV, https://www.shadertoy.com/view/McfXzr Wireframe by FabriceNeyret2, radial blur adaptation, ISF conversion by Igor Molochevski",
    "ISFVSN": "2.0",
    "CATEGORIES": ["Geometry", "Blur","ShaderToy"],
    "INPUTS": [
        {
            "NAME": "rotationSpeed",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.0,
            "MAX": 5.0
        },
        {
            "NAME": "blurAmount",
            "TYPE": "float",
            "DEFAULT": 25.0,
            "MIN": 1.0,
            "MAX": 60
        },
        {
            "NAME": "usePerspective",
            "TYPE": "bool",
            "DEFAULT": true
        },
        {
            "NAME": "manualRotation",
            "TYPE": "bool",
            "DEFAULT": false
        },
        {
            "NAME": "rotationX",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": -3.14159,
            "MAX": 3.14159
        },
        {
            "NAME": "rotationY",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": -3.14159,
            "MAX": 3.14159
        }
    ],
    "PASSES": [
        {
            "TARGET": "bufferA",
            "PERSISTENT": true,
            "WIDTH": "$WIDTH",
            "HEIGHT": "$HEIGHT"
        },
        {}
    ]
}*/

// --- Utility Functions ---

float hash12(vec2 u) {
    vec3 p = fract(u.xyx * .1031f);
    p += dot(p, p.yzx + 33.33f);
    return fract((p.x + p.y) * p.z);
}

vec3 H(float a) {
    return cos(radians(vec3(0.0f, 60.0f, 120.0f)) + (a) * 6.2832f) * 0.5f + 0.5f;
}

float L(vec2 p, vec3 A, vec3 B) {
    vec2 a = A.xy, b = B.xy - a;
    p -= a;
    float h = clamp(dot(p, b) / dot(b, b), 0.0f, 1.0f);
    return length(p - b * h) + 0.01f * mix(A.z, B.z, h);
}

mat2 A(float v) {
    float c = cos(v * 3.1416f);
    float s = sin(v * 3.1416f);
    return mat2(c, -s, s, c);
}

vec3 K(vec3 p, mat2 v, mat2 h) {
    p.zy = v * p.zy;
    p.zx = h * p.zx;
    if(usePerspective)
        p *= 4.0f / (p.z + 4.0f);
    return p;
}

// Custom tanh for GLSL ES (ISF 2.0)
float tanh_compat(float x) {
    float e1 = exp(x);
    float e2 = exp(-x);
    return (e1 - e2) / (e1 + e2);
}

// --- Main Shader ---

#define s(a,b) col = max(col, 0.006 / abs(L(u, K(a, v, h), K(b, v, h)) + 0.02))

void main() {
    if(PASSINDEX == 0) {
        // --- Wireframe Pass ---
        vec2 R = RENDERSIZE.xy;
        vec2 u = (gl_FragCoord.xy * 2.0f - R) / R.y * 3.0f;
        float t = TIME / 120.0f * rotationSpeed;
        float a = 1.618f;
        vec2 m;
        if(manualRotation) {
            m = vec2(rotationX, rotationY);
        } else {
            m = vec2(sin(t * 6.2832f) * 2.0f, sin(t * 6.2832f * 2.0f) * 0.7f);
        }
        mat2 v = A(m.y);
        mat2 h = A(m.x);
        vec3 col = vec3(0.0f);

        // Stellated dodecahedron segments
        s(vec3(-1, a, 0), vec3(0, -1, -a));
        s(vec3(-1, a, 0), vec3(0, -1, a));
        s(vec3(-1, a, 0), vec3(a, 0, -1));
        s(vec3(-1, a, 0), vec3(a, 0, 1));
        s(vec3(1, a, 0), vec3(1, -a, 0));
        s(vec3(1, a, 0), vec3(0, -1, -a));
        s(vec3(1, a, 0), vec3(0, -1, a));
        s(vec3(1, a, 0), vec3(-a, 0, -1));
        s(vec3(1, a, 0), vec3(-a, 0, 1));
        s(vec3(-1, -a, 0), vec3(-1, a, 0));
        s(vec3(-1, -a, 0), vec3(0, 1, -a));
        s(vec3(-1, -a, 0), vec3(0, 1, a));
        s(vec3(-1, -a, 0), vec3(a, 0, -1));
        s(vec3(-1, -a, 0), vec3(a, 0, 1));
        s(vec3(1, -a, 0), vec3(0, 1, -a));
        s(vec3(1, -a, 0), vec3(0, 1, a));
        s(vec3(1, -a, 0), vec3(-a, 0, -1));
        s(vec3(1, -a, 0), vec3(-a, 0, 1));
        s(vec3(0, 1, -a), vec3(0, 1, a));
        s(vec3(0, 1, -a), vec3(a, 0, 1));
        s(vec3(0, 1, -a), vec3(-a, 0, 1));
        s(vec3(0, -1, -a), vec3(0, -1, a));
        s(vec3(0, -1, -a), vec3(a, 0, 1));
        s(vec3(0, -1, -a), vec3(-a, 0, 1));
        s(vec3(-a, 0, -1), vec3(a, 0, -1));
        s(vec3(-a, 0, 1), vec3(a, 0, 1));
        s(vec3(-a, 0, -1), vec3(0, 1, a));
        s(vec3(-a, 0, -1), vec3(0, -1, a));
        s(vec3(a, 0, -1), vec3(0, 1, a));
        s(vec3(a, 0, -1), vec3(0, -1, a));

        gl_FragColor = vec4(min(col, 1.0f), 1.0f);
    } else {
        // --- Radial Blur Pass ---
        vec2 R = RENDERSIZE.xy;
        vec2 u = gl_FragCoord.xy / R;
        vec2 m = vec2(0.5f);

        vec3 col = IMG_NORM_PIXEL(bufferA, u).rgb * 0.7f;

        float l = blurAmount;
        float s_step = 1.0f;
        float j = hash12(gl_FragCoord.xy + TIME);
        float v = 0.0f;

        // Cap loop at 100 for GLSL ES compatibility
        for(float i = 0.0f; i < 100.0f; i += 1.0f) {
            if(i >= l)
                break;
            float d = 1.0f - i / l;
            float blend = (v + j) / l;
            vec2 offset = mix(u, m, blend);
            vec2 offset2 = mix(u, m, -blend);
            vec3 blurOut = IMG_NORM_PIXEL(bufferA, offset).rgb;
            vec3 blurIn = IMG_NORM_PIXEL(bufferA, offset2).rgb;
            col += (blurOut + blurIn) * H(d) * 0.2f;
            v += s_step;
        }

        // Add red to center
        vec2 screenPos = (gl_FragCoord.xy * 2.0f - R) / R.y * 3.0f;
        col.r += 0.3f - length(screenPos) * 0.1f;

        // tanh for each component (GLSL ES compatible)
        vec3 processed;
        processed.r = tanh_compat(col.r * col.r);
        processed.g = tanh_compat(col.g * col.g);
        processed.b = tanh_compat(col.b * col.b);

        gl_FragColor = vec4(processed, 1.0f);
    }
}
