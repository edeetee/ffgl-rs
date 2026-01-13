/*{
    "INPUTS": [
        {
            "DEFAULT": 0.2,
            "NAME": "scale",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "NAME": "camSpacing",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2"
}
*/

mat3 getRotZMat(float a) {
    return mat3(cos(a), -sin(a), 0.f, sin(a), cos(a), 0.f, 0.f, 0.f, 1.f);
}

float dstepf = 0.0f;

float map(vec3 p) {
    p.x += sin(p.z * 1.8f);
    p.y += cos(p.z * .2f) * sin(p.x * .8f);
    p *= getRotZMat(p.z * 0.8f + sin(p.x) + cos(p.y));
    p.xy = mod(p.xy, 0.3f) - 0.15f;
    dstepf += 0.003f;
    return length(p.xy);
}

#define PI 3.14159

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - RENDERSIZE.xy * .5f) / RENDERSIZE.y * scale * 10.0f;

    vec3 rd = normalize(vec3(uv, (1.f - dot(uv, uv) * .5f) * .5f));

    vec3 ro = vec3(0, 0, TIME * 1.26f), col = vec3(0), sp;
    float cs = cos(TIME * 0.375f), si = sin(TIME * 0.375f);

    rd.xz = mat2(cs, si, -si, cs) * rd.xz;
    float t = camSpacing * 5.0f, layers = 0.f, d = 0.f, aD;
    float thD = 0.02f;

    for(float i = 0.f; i < 250.f; i++) {
        if(layers > 15.f || col.x > 1.f || t > 5.6f)
            break;
        sp = ro + rd * t;
        d = map(sp);
        aD = (thD - abs(d) * 15.f / 16.f) / thD;
        if(aD > 0.f) {
            col += aD * aD * (3.f - 2.f * aD) / (1.f + t * t * 0.25f) * .2f;
            layers++;
        }
        t += max(d * .7f, thD * 1.5f) * dstepf;
    }

    col = max(col, 0.f);
    col = mix(col, vec3(min(col.x * 1.5f, 1.f), pow(col.x, 2.5f), pow(col.x, 12.f)), dot(sin(rd.yzx * 8.f + sin(rd.zxy * 8.f)), vec3(.1666f)) + 0.4f);
    col = mix(col, vec3(col.x * col.x * .85f, col.x, col.x * col.x * 0.3f), dot(sin(rd.yzx * 4.f + sin(rd.zxy * 4.f)), vec3(.1666f)) + 0.25f);
    fragColor = vec4(clamp(col, 0.f, 1.f), 1.0f);
}

void main() {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}