/*{
    "CATEGORIES": [
        "Generator",
        "3D"
    ],
    "DESCRIPTION": "3D geometric structures with animation",
    "INPUTS": [
        {
            "NAME": "progress",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 1.0,
            "LABEL": "Animation Progress"
        },
        {
            "NAME": "speed",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.0,
            "MAX": 2.0,
            "LABEL": "Animation Speed"
        },
        {
            "NAME": "detail",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.5,
            "MAX": 1.5,
            "LABEL": "Detail Level"
        }
    ]
}*/

const float PI = 3.14159265358979323846f;

float rand(in vec2 p, in float t) {
    return fract(sin(dot(p + mod(t, 1.0f), vec2(12.9898f, 78.2333f))) * 43758.5453f);
}

vec2 rotate(vec2 k, float t) {
    return vec2(cos(t) * k.x - sin(t) * k.y, sin(t) * k.x + cos(t) * k.y);
}

float scene1(vec3 p, float time) {
    float current_time = time * speed;
    float ground = dot(p, vec3(0.0f, 1.0f, 0.0f)) + 0.75f;
    float t1 = length(abs(mod(p.xyz, 2.0f) - 1.0f)) - 1.35f + 0.05f * cos(PI * p.x * 4.0f) + 0.05f * sin(PI * p.z * 4.0f);	// structure
    float t3 = length(max(abs(mod(p.xyz, 2.0f) - 1.0f).xz - 1.0f, 0.5f)) - 0.075f + 0.1f * cos(p.y * 36.0f * detail);	// structure slices
    float t5 = length(abs(mod(p.xyz, 0.5f)) - 0.25f) - 0.975f;
    float bubble_w = 0.8f + 0.2f * cos(PI * p.z) + 0.2f * cos(PI * p.x);
    float bubble = length(mod(p.xyz, 0.125f) - 0.0625f) - bubble_w;
    float hole_w = 0.05f;
    float hole = length(abs(mod(p.xz, 1.0f) - 0.5f)) - hole_w;
    float tube_p = 2.0f - 0.25f * sin(PI * p.z * 0.5f);
    float tube_v = PI * 8.0f * detail;
    float tube_b = tube_p * 0.02f;
    float tube_w = tube_b + tube_b * cos(p.x * tube_v) * sin(p.y * tube_v) * cos(p.z * tube_v) + tube_b * sin(PI * p.z + current_time * 4.0f);
    float tube = length(abs(mod(p.xy, tube_p) - tube_p * 0.5f)) - tube_w;
    return min(max(min(-t1, max(-hole - t5 * 0.375f, ground + bubble)), t3 + t5), tube);
}

void main() {
    // Use progress for animation control (0.0 to 1.0)
    float time = TIME * 0.5f + progress * 10.0f;

    float ground_x = 1.5f * cos(PI * time * 0.125f);
    float ground_y = 4.0f - 3.0f * sin(PI * time * 0.125f) + 0.125f * progress;
    float ground_z = -1.0f - time;

    vec2 position = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = -1.0f + 2.0f * position;
    vec3 dir = normalize(vec3(p * vec2(1.625f, 1.0f), 0.75f));	// screen ratio (x,y) fov (z)

    dir.yz = rotate(dir.yz, PI * 0.25f * sin(PI * time * 0.125f) - progress * 0.25f);	// rotation x
    dir.zx = rotate(dir.zx, PI * cos(-PI * time * 0.05f));		// rotation y
    dir.xy = rotate(dir.xy, PI * 0.125f * cos(PI * time * 0.125f));	// rotation z

    vec3 ray = vec3(ground_x, ground_y, ground_z);
    float t = 0.0f;
    int ray_n = int(96.0f * detail);

    for(int i = 0; i < ray_n; i++) {
        float k = scene1(ray + dir * t, time);
        if(abs(k) < 0.005f)
            break;
        t += k * 0.5f;
    }

    vec3 hit = ray + dir * t;
    vec2 h = vec2(-0.02f, 0.01f); // light
    vec3 n = normalize(vec3(scene1(hit + h.xyy, time), scene1(hit + h.yxx, time), scene1(hit + h.yyx, time)));
    float c = (n.x + n.y + n.z) * 0.1f;
    vec3 color = vec3(c, c, c) - t * 0.0625f;

    // Optional noise effect
    // color *= 0.6 + 0.4 * rand(vec2(t, t), TIME); 

    gl_FragColor = vec4(vec3(c + t * 0.08f, c + t * 0.02f, c * 1.5f - t * 0.01f) + color * color, 1.0f);
}